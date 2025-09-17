using UnityEngine;

public class TriggerExplosion : MonoBehaviour
{
    [Header("Explosion Prefab in Scene (disabled by default)")]
    public GameObject explosionPrefab; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && explosionPrefab != null)
        {
            explosionPrefab.SetActive(true);

            ParticleSystem[] particles = explosionPrefab.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }

            AudioSource[] audios = explosionPrefab.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                audio.Stop();
                audio.Play();
            }
        }
    }
}
