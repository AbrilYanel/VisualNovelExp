using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class NpcProximityTrigger : MonoBehaviour
{
    [Header("Audios")]
    [SerializeField] private AudioClip[] murmurClips;

    [Header("Timing")]
    [SerializeField] private float minTimeBetweenClips = 3f;
    [SerializeField] private float maxTimeBetweenClips = 7f;
    [SerializeField] private float initialDelay = 0.2f; // pequeño delay al entrar

    private AudioSource audioSource;
    private Coroutine murmurRoutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Por si acaso había una corrutina anterior corriendo
        if (murmurRoutine != null) StopCoroutine(murmurRoutine);
        murmurRoutine = StartCoroutine(MurmurLoop());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (murmurRoutine != null)
        {
            StopCoroutine(murmurRoutine);
            murmurRoutine = null;
        }
    }

    IEnumerator MurmurLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (murmurClips.Length > 0)
            {
                AudioClip clip = murmurClips[Random.Range(0, murmurClips.Length)];
                audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            yield return new WaitForSeconds(Random.Range(minTimeBetweenClips, maxTimeBetweenClips));
        }
    }
}