using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;


public class GrabTrigger : MonoBehaviour
{
    private BoxCollider boxCollider; // BoxCollider to modify
    private Vector3 originalSize;     // Store original size

    [SerializeField] public bool isGrabbing = false;
    [SerializeField] private MetaVoidRevealer revealer; // Reference to MetaVoidRevealer
    public AudioSource audioSource;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("No BoxCollider found on the GameObject.");
        }
        else
        {
            originalSize = boxCollider.size;
        }

        if (revealer == null)
        {
            revealer = FindObjectOfType<MetaVoidRevealer>();
            if (revealer == null)
                Debug.LogError("No MetaVoidRevealer found in scene. Assign one in the inspector.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isGrabbing)
        {
            Debug.Log("enterCollider");

            if (boxCollider != null)
                boxCollider.size *= 3f; // Increase the size by 3x

            // 🔑 Call RevealWorld when grabbing is detected
            if (revealer != null)
                revealer.RevealWorld();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (boxCollider != null)
                boxCollider.size = originalSize; // Reset to original size
        }
    }

    public void EnableGrabbing()
    {
        isGrabbing = true;
        Debug.Log("detectGrab");

        // 🔑 Optionally reveal immediately when grabbing starts
        if (revealer != null)
            revealer.RevealWorld();
    }

    public void DisableGrabbing()
    {
        isGrabbing = false;
    }
}
