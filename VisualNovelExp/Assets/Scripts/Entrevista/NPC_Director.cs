using UnityEngine;

public class NPC_Director : MonoBehaviour
{
    [Header("Referencias")]
    public Manager_Interaccion managerInteraccion;
    public Manager_Camara managerCamara;

    [Header("Nodos de di�logo")]
    public Nodo_Dialogo nodoInicio;
    public Nodo_Dialogo nodoMisionActiva;
    public Nodo_Dialogo nodoEntrega;
    public Nodo_Dialogo nodoMisionCompleta;

    private bool misionCompletada = false;
    public PlayerProgress playerProgress;
    public int interaccionesAlCompletar = 1; // Ajustado a 3 interacciones por nivel (ahora es solo el último de 9 NPCs)
    public int recompensaMonedas = 20;
    public void Interact()
    {
        // Misi�n ya terminada del todo
        if (misionCompletada)
        {
            managerInteraccion.StartDialogue(nodoMisionCompleta);
            return;
        }

        // Jugador vuelve con la entrevista lista
        if (managerCamara.entrevistaCompletada)
        {
            managerInteraccion.StartDialogue(nodoEntrega);
            return;
        }

        // Misi�n en curso (ya tiene c�mara)
        if (managerCamara.tieneCamara)
        {
            managerInteraccion.StartDialogue(nodoMisionActiva);
            return;
        }

        // Primera interacci�n: dar c�mara
        managerInteraccion.StartDialogue(nodoInicio);
    }


    public void IniciarMision()
    {
        managerCamara.RecibirCamara();
    }

    // Llamado desde el evento del nodo de entrega
    public void CompletarMision()
    {
        int puntaje = managerCamara.puntajeEntrevista;
        misionCompletada = true;
        managerCamara.EntregarEntrevista();

        bool exitosa = puntaje >= managerCamara.puntajeMinimoExito;

        if (exitosa && playerProgress != null)
        {

            for (int i = 0; i < interaccionesAlCompletar; i++)
            {
                playerProgress.CompletarInteraccion();
            }

            playerProgress.GanarMonedas(recompensaMonedas);

            // Actualizar UI
            managerInteraccion.ActualizarUIProgreso();

            Debug.Log($" Misi�n completada. Nivel actual: {playerProgress.nivelActual}");
        }
        else if (!exitosa)
        {
            Debug.Log("Entrevista entregada pero puntaje insuficiente");
        }
    }
}