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

    [Header("UI nivel insuficiente")]
    public GameObject panelNivelInsuficiente;
    public TextMeshProUGUI textoNivelInsuficiente;

    private bool completado = false;

    public void Interact()
    {
        if (completado)
            return;

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
    }


}