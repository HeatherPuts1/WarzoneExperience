using UnityEngine;

public class TriggerAudioClip : MonoBehaviour
{
    public AudioClip clip;    
    public float volume = 1f;  

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = clip;
        audioSource.volume = volume;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && clip != null)
        {
            audioSource.Play();
        }
    }
}
