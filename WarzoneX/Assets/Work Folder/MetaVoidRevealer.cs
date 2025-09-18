using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

public class MetaVoidRevealer : MonoBehaviour
{
    [Header("Cameras & Layers")]
    [Tooltip("Optional. If left empty, Camera.main will be used for quick setups.")]
    public Camera xrCamera;
    [Tooltip("Layer(s) that contain the hidden world (the scene that should be revealed).")]
    public LayerMask hiddenWorldLayer;
    [Tooltip("Layer(s) that should remain visible while in the void.")]
    public LayerMask voidObjectsLayer;

    [Header("Fallback overlay (quad)")]
    [Tooltip("Optional: the Quad Renderer you used previously. If left empty and OVRScreenFade exists, OVRScreenFade is used.")]
    public Renderer overlayRenderer;

    [Header("Timing")]
    [Tooltip("How long the fallback fade takes (seconds). If OVRScreenFade is present, that component's fadeTime is used instead.")]
    public float fallbackFadeDuration = 1.0f;

    [Header("Inspector test toggles (Play mode)")]
    public bool triggerReveal = false; // flip in inspector to test (reveals world)
    public bool triggerHide = false;   // flip in inspector to test (hides world)

    // runtime state
    bool isFading = false;
    bool isRevealed = false;

    // reflection handles for OVRScreenFade (so this script compiles even if Oculus integration isn't present)
    object ovrFadeInstance = null;
    MethodInfo ovrFadeIn = null;
    MethodInfo ovrFadeOut = null;
    MethodInfo ovrSetExplicit = null;
    FieldInfo ovrFadeTimeField = null;
    PropertyInfo ovrFadeTimeProp = null;

    void Awake()
    {
        if (xrCamera == null) xrCamera = Camera.main;

        // discover OVRScreenFade via reflection (no compile-time dependency)
        FindOVRScreenFade();

        // start with world hidden (only show VoidObjects)
        SetOnlyLayersOnAllCameras(voidObjectsLayer);

        // ensure screen starts black if OVRScreenFade is available, else ensure overlay quad is fully opaque
        if (ovrSetExplicit != null && ovrFadeInstance != null)
        {
            // set the fade to 1 (fully black)
            try { ovrSetExplicit.Invoke(ovrFadeInstance, new object[] { 1f }); } catch { }
        }
        else if (overlayRenderer != null)
        {
            Color c = overlayRenderer.material.color;
            overlayRenderer.material.color = new Color(c.r, c.g, c.b, 1f);
            overlayRenderer.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // inspector debug toggles (auto-reset so it only fires once)
        if (triggerReveal)
        {
            triggerReveal = false;
            RevealWorld();
        }

        if (triggerHide)
        {
            triggerHide = false;
            HideWorld();
        }
    }

    /// <summary>
    /// Public method: reveal the hidden world (fade out the void / fade in the scene).
    /// Wire this to an Interaction SDK event (e.g. your interactable's UnityEvent).
    /// </summary>
    public void RevealWorld()
    {
        if (!isFading && !isRevealed)
            StartCoroutine(RevealCoroutine());
    }

    /// <summary>
    /// Public method: hide the world again (fade to black / return to the void).
    /// </summary>
    public void HideWorld()
    {
        if (!isFading && isRevealed)
            StartCoroutine(HideCoroutine());
    }

    IEnumerator RevealCoroutine()
    {
        isFading = true;

        // ensure hidden world is already being rendered underneath the fade, so it appears as the fade fades out
        AddLayerToAllCameras(hiddenWorldLayer);

        float duration = GetOVRFadeTimeOrFallback();

        // use OVRScreenFade if present (preferred)
        if (ovrFadeIn != null && ovrFadeInstance != null)
        {
            try { ovrFadeIn.Invoke(ovrFadeInstance, null); } catch { }
        }
        else
        {
            // fallback: fade the quad from alpha=1 -> alpha=0
            if (overlayRenderer != null)
                yield return StartCoroutine(FadeOverlayAlpha(1f, 0f, duration));
        }

        // wait the duration to make sure fade finished
        yield return new WaitForSeconds(duration);

        isFading = false;
        isRevealed = true;
    }

    IEnumerator HideCoroutine()
    {
        isFading = true;

        float duration = GetOVRFadeTimeOrFallback();

        // use OVRScreenFade to fade to black (preferred)
        if (ovrFadeOut != null && ovrFadeInstance != null)
        {
            try { ovrFadeOut.Invoke(ovrFadeInstance, null); } catch { }
        }
        else
        {
            // fallback: fade the quad from alpha=0 -> alpha=1
            if (overlayRenderer != null)
                yield return StartCoroutine(FadeOverlayAlpha(0f, 1f, duration));
        }

        // wait for the fade to complete
        yield return new WaitForSeconds(duration);

        // after fade to black, hide the world by removing its layers from camera culling masks
        SetOnlyLayersOnAllCameras(voidObjectsLayer);

        // ensure overlay is active if using fallback
        if (overlayRenderer != null)
            overlayRenderer.gameObject.SetActive(true);

        isFading = false;
        isRevealed = false;
    }

    IEnumerator FadeOverlayAlpha(float startA, float endA, float duration)
    {
        if (overlayRenderer == null) yield break;
        overlayRenderer.gameObject.SetActive(true);
        Material mat = overlayRenderer.material;
        Color col = mat.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startA, endA, Mathf.Clamp01(t / duration));
            mat.color = new Color(col.r, col.g, col.b, a);
            yield return null;
        }
        mat.color = new Color(col.r, col.g, col.b, endA);

        // optionally disable overlay when fully transparent
        if (Mathf.Approximately(endA, 0f))
            overlayRenderer.gameObject.SetActive(false);
    }

    float GetOVRFadeTimeOrFallback()
    {
        float dur = fallbackFadeDuration;
        try
        {
            if (ovrFadeTimeProp != null && ovrFadeInstance != null)
            {
                object val = ovrFadeTimeProp.GetValue(ovrFadeInstance);
                if (val is float f) dur = f;
            }
            else if (ovrFadeTimeField != null && ovrFadeInstance != null)
            {
                object val = ovrFadeTimeField.GetValue(ovrFadeInstance);
                if (val is float f2) dur = f2;
            }
        }
        catch { }
        return dur;
    }

    // --- Camera layer helpers ---
    void SetOnlyLayersOnAllCameras(LayerMask mask)
    {
        int m = mask.value;
        Camera[] cams = Camera.allCameras;
        for (int i = 0; i < cams.Length; ++i)
        {
            // if you prefer to preserve UI layers, extend this to include them explicitly
            cams[i].cullingMask = m;
        }
    }

    void AddLayerToAllCameras(LayerMask maskToAdd)
    {
        int add = maskToAdd.value;
        Camera[] cams = Camera.allCameras;
        for (int i = 0; i < cams.Length; ++i)
            cams[i].cullingMask |= add;
    }

    // --- Reflection: find OVRScreenFade type and methods at runtime (no compile dependency) ---
    void FindOVRScreenFade()
    {
        try
        {
            Type found = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // try common type names
                found = asm.GetType("OVRScreenFade");
                if (found != null) break;
            }
            if (found == null) return;

            // locate instance (static field or property)
            object instanceObj = null;
            var prop = found.GetProperty("instance", BindingFlags.Static | BindingFlags.Public);
            if (prop != null) instanceObj = prop.GetValue(null);
            else
            {
                var fld = found.GetField("instance", BindingFlags.Static | BindingFlags.Public);
                if (fld != null) instanceObj = fld.GetValue(null);
            }

            ovrFadeInstance = instanceObj;
            ovrFadeIn = found.GetMethod("FadeIn", BindingFlags.Public | BindingFlags.Instance);
            ovrFadeOut = found.GetMethod("FadeOut", BindingFlags.Public | BindingFlags.Instance);
            ovrSetExplicit = found.GetMethod("SetExplicitFade", BindingFlags.Public | BindingFlags.Instance);

            // fadeTime might be a field or property
            ovrFadeTimeField = found.GetField("fadeTime", BindingFlags.Public | BindingFlags.Instance);
            if (ovrFadeTimeField == null)
                ovrFadeTimeProp = found.GetProperty("fadeTime", BindingFlags.Public | BindingFlags.Instance);
        }
        catch
        {
            // ignore, fallback will be used
        }
    }
}
