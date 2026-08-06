using System.Collections;
using UnityEngine;
using TMPro;

public class Interaccion_NPC : MonoBehaviour
{
    [Header("Diálogo")]
    public Manager_Interaccion dialogueManager;
    public Nodo_Dialogo startNode;

    [Header("Nivel requerido")]
    public int nivelRequerido = 1;
    public PlayerProgress playerProgress;

    [Header("UI")]
    public GameObject panelNivelInsuficiente;
    public TextMeshProUGUI textoNivelInsuficiente;
    public GameObject indicadorExclamacion;

    [Header("Animación")]
    public Animator animator;
    public string triggerInteractuar = "Interact";
    private Coroutine corrutinaAnimacion;

    private bool completado = false;

    public NPC_Entrevistado npcEntrevistado;
    public NPC_Director npcDirector;
    public NPC_Comerciante npcComerciante;

    [Header("Sonido")]
    public AudioClip sonidoInteractuar;
    private AudioSource audioSource;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        ActualizarIndicador();
    }

    public void Interact()
    {
        if (completado)
            return;

        if (playerProgress != null && !playerProgress.PuedeInteractuar(nivelRequerido))
        {
            MostrarMensajeNivel();
            return;
        }

        // Reproducir animación del NPC una sola vez
        if (animator != null && !string.IsNullOrEmpty(triggerInteractuar))
            animator.SetTrigger(triggerInteractuar);

        if (audioSource != null && sonidoInteractuar != null)
            audioSource.PlayOneShot(sonidoInteractuar);

        if (npcEntrevistado != null)
        {
            npcEntrevistado.Interact();
            return;
        }

        if (npcDirector != null)
        {
            npcDirector.Interact();
            return;
        }

        if (npcComerciante != null)
        {
            npcComerciante.Interact();
            return;
        }

        dialogueManager.SetNPCActual(this);
        dialogueManager.StartDialogue(startNode);
    }

    void MostrarMensajeNivel()
    {
        if (panelNivelInsuficiente != null)
        {
            panelNivelInsuficiente.SetActive(true);
            textoNivelInsuficiente.text =
                $"Necesitás nivel {nivelRequerido} para hablar con este NPC.\n";

            Invoke(nameof(OcultarMensajeNivel), 2.5f);
        }
    }

    void OcultarMensajeNivel()
    {
        if (panelNivelInsuficiente != null)
            panelNivelInsuficiente.SetActive(false);
    }

    public void MarcarCompletado()
    {
        completado = true;
        ActualizarIndicador();

    }

    public void ActualizarIndicador()
    {
        if (indicadorExclamacion == null) return;


        bool mostrar = !completado &&
                       playerProgress != null &&
                       playerProgress.PuedeInteractuar(nivelRequerido);

        indicadorExclamacion.SetActive(mostrar);
    }


}