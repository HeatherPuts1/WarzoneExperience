using UnityEngine;
using UnityEngine.UI; // Only needed if you’re using a UI Image

public class VoidFade : MonoBehaviour
{
    public Renderer overlayRenderer;   // Quad with black transparent material
    public float fadeDuration = 2f;

    [Header("Testing")]
    public bool fadeOutTrigger = false;  // Toggle in Inspector to fade out
    public bool fadeInTrigger = false;   // Toggle in Inspector to fade in

    private bool fading = false;

    void Update()
    {
        // Inspector toggle triggers
        if (fadeOutTrigger)
        {
            fadeOutTrigger = false; // reset so it only runs once
            FadeOut();
        }

        if (fadeInTrigger)
        {
            fadeInTrigger = false; // reset so it only runs once
            FadeIn();
        }
    }

    public void FadeOut()
    {
        if (!fading && overlayRenderer != null)
            StartCoroutine(FadeRoutine(1f, 0f));
    }

    public void FadeIn()
    {
        if (!fading && overlayRenderer != null)
            StartCoroutine(FadeRoutine(0f, 1f));
    }

    private System.Collections.IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        fading = true;

        float t = 0f;
        Color color = overlayRenderer.material.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            overlayRenderer.material.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Ensure exact end value
        overlayRenderer.material.color = new Color(color.r, color.g, color.b, endAlpha);

        fading = false;
    }
}
