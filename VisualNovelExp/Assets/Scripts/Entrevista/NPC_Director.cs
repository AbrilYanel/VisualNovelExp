using UnityEngine;

public class NPC_Director : MonoBehaviour
{
    [Header("Referencias")]
    public Manager_Interaccion managerInteraccion;
    public Manager_Camara managerCamara;
    public PlayerProgress playerProgress;

    [Header("Nodos de diálogo")]
    [Tooltip("Primera vez: presenta el festival y da la cámara (marcar Da Camara)")]
    public Nodo_Dialogo nodoInicio;
    [Tooltip("Ya tiene la cámara pero todavía no hizo la entrevista")]
    public Nodo_Dialogo nodoMisionActiva;
    [Tooltip("Vuelve con la entrevista (marcar Entrega Entrevista y Ends Dialogue)")]
    public Nodo_Dialogo nodoEntrega;
    [Tooltip("Después del festival")]
    public Nodo_Dialogo nodoMisionCompleta;

    [Header("Recompensa (solo si la entrevista fue exitosa)")]
    public int interaccionesAlCompletar = 1;
    public int recompensaMonedas = 20;

    // Se guarda en PlayerProgress, así no se pierde al ir a la casa y volver
    bool MisionCompletada => playerProgress != null && playerProgress.festivalCompletado;

    public void Interact()
    {
        if (MisionCompletada)
        {
            managerInteraccion.StartDialogue(nodoMisionCompleta);
            return;
        }

        if (managerCamara.entrevistaCompletada)
        {
            managerInteraccion.StartDialogue(nodoEntrega);
            return;
        }

        if (managerCamara.tieneCamara)
        {
            managerInteraccion.StartDialogue(nodoMisionActiva);
            return;
        }

        managerInteraccion.StartDialogue(nodoInicio);
    }

    public void IniciarMision()
    {
        managerCamara.RecibirCamara();
    }

    // Lo llama Manager_Interaccion al mostrar el nodo con "Entrega Entrevista"
    public void CompletarMision()
    {
        if (MisionCompletada) return; // evita pagar dos veces

        int puntaje = managerCamara.puntajeEntrevista;
        managerCamara.EntregarEntrevista();

        bool exitosa = puntaje >= managerCamara.puntajeMinimoExito;
        if (exitosa && playerProgress != null)
        {
            for (int i = 0; i < interaccionesAlCompletar; i++)
                playerProgress.CompletarInteraccion();

            playerProgress.GanarMonedas(recompensaMonedas);
            managerInteraccion.ActualizarUIProgreso();
        }
    }
}
