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

    private bool completado = false;

    public NPC_Entrevistado npcEntrevistado;   
    public NPC_Director npcDirector;          

    void Start()
    {
        ActualizarIndicador();
    }

    public void Interact()
    {
        if (completado)
            return;

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

        if (playerProgress != null && !playerProgress.PuedeInteractuar(nivelRequerido))
        {
            MostrarMensajeNivel();
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
                $"Necesitás nivel {nivelRequerido} para hablar con este NPC.\n" +
                $"Tu nivel actual es {playerProgress.nivelActual}.";

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