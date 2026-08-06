using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class NpcProximityTrigger : MonoBehaviour
{
    [Header("Audios")]
    [SerializeField] private AudioClip[] murmurClips;

    [Header("Rotacion hacia el jugador")]
    [SerializeField] private float velocidadRotacion = 5f;
    [SerializeField] private bool rotarSoloEnY = true;

    [Header("Timing")]
    [SerializeField] private float minTimeBetweenClips = 3f;
    [SerializeField] private float maxTimeBetweenClips = 7f;
    [SerializeField] private float initialDelay = 0.2f;

    private AudioSource audioSource;
    private Coroutine murmurRoutine;
    private Transform playerTransform;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector3 direccion = playerTransform.position - transform.position;

        if (rotarSoloEnY)
            direccion.y = 0f;

        if (direccion.magnitude < 0.01f) return;

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerTransform = other.transform;

        if (murmurRoutine != null) StopCoroutine(murmurRoutine);
        murmurRoutine = StartCoroutine(MurmurLoop());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerTransform = null;

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
