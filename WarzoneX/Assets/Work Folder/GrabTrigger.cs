using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;


public class GrabTrigger : MonoBehaviour
{

    private BoxCollider boxCollider; // BoxCollider to modify

    [SerializeField] public bool isGrabbing = false; // Determines if the cube can turn green

    // Set initial conditions
    void Start()
    {
        // Store the original material
        

        // Get the BoxCollider component
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("No BoxCollider found on the GameObject.");
        }
    }

    // When the player enters the box collider
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isGrabbing)
        {
            // Change the material
            if (isGrabbing)
            {
                Debug.Log("enterCollider");
            }

            // Expand the BoxCollider
            if (boxCollider != null)
            {
                boxCollider.size *= 3f; // Increase the size by 3x
            }
        }
    }

    // Continuously check if the player is staying in the collider
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !isGrabbing)
        {
            
        }
        if (other.CompareTag("Player") && isGrabbing)
        {
           
        }
    }

    // When the player exits the box collider
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           

            // Reset the BoxCollider size back to normal
            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(1, 1, 1); // Reset to the original size
            }
        }
    }

    // Set isGrabbing to true
    public void EnableGrabbing()
    {
        isGrabbing = true;
        Debug.Log("detectGrab");
    }

    // Set isGrabbing to false
    public void DisableGrabbing()
    {
        isGrabbing = false;
    }
}
