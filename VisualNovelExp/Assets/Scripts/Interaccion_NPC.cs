using System.Collections;
using UnityEngine;
using TMPro;

public class Interaccion_NPC : MonoBehaviour, IInteractuable
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

    [Header("Objetivo en pantalla")]
    public string nombreVisible = "Vecino";
    public int orden = 1;
    public bool cuentaComoObjetivo = true;
    public bool Completado => completado;

    public NPC_Entrevistado npcEntrevistado;
    public NPC_Director npcDirector;
    public NPC_Comerciante npcComerciante;

    [Tooltip("Marcado: atiende aunque sea de noche (konbini, Director, entrevistado)")]
    public bool atiendeDeNoche = false;

    [Header("Sonido")]
    public AudioClip sonidoInteractuar;
    private AudioSource audioSource;

    [Header("Trabajo repetible")]
    [Tooltip("Nunca queda completado: paga monedas fijas cada vez que se gana")]
    public bool repetible = false;
    public int pagoTrabajo = 12;

    [Header("Retrato en el diálogo")]
    public Sprite retratoDialogo;
    [Tooltip("Marcado: el NPC aparece a la izquierda y el jugador a la derecha")]
    public bool retratoALaIzquierda = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (playerProgress != null && playerProgress.npcsCompletados.Contains(gameObject.name))
            completado = true;

        ActualizarIndicador();
    }

    public void Interact()
    {
        if (completado)
            return;
        if (!atiendeDeNoche && Manager_Dia.I != null && Manager_Dia.I.EsDeNoche)
        {
            if (Mensaje_UI.I) Mensaje_UI.I.Mostrar("Es de noche y ya no atiende. Vuelve mañana.", 3f);
            return;
        }

        if (playerProgress != null && !playerProgress.PuedeInteractuar(nivelRequerido))
        {
            MostrarMensajeNivel();
            return;
        }
        if (dialogueManager != null) dialogueManager.SetNPCActual(this);
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
        if (playerProgress != null && !playerProgress.npcsCompletados.Contains(gameObject.name))
            playerProgress.npcsCompletados.Add(gameObject.name);
        ActualizarIndicador();

    }

    public void ActualizarIndicador()
    {
        if (indicadorExclamacion == null) return;


        bool deNoche = !atiendeDeNoche && Manager_Dia.I != null && Manager_Dia.I.EsDeNoche;
        bool mostrar = !completado && !deNoche &&
                       playerProgress != null &&
                       playerProgress.PuedeInteractuar(nivelRequerido);

        indicadorExclamacion.SetActive(mostrar);
    }


}