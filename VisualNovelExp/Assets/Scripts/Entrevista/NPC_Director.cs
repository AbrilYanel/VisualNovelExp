using UnityEngine;

public class NPC_Director : MonoBehaviour
{
    [Header("Referencias")]
    public Manager_Interaccion managerInteraccion;
    public Manager_Camara managerCamara;

    [Header("Nodos de diálogo")]
    public Nodo_Dialogo nodoInicio;           // Primera vez que habla
    public Nodo_Dialogo nodoMisionActiva;     // Ya le dio la cámara, aún no terminó
    public Nodo_Dialogo nodoEntrega;          // Jugador vuelve con entrevista completa
    public Nodo_Dialogo nodoMisionCompleta;   // Ya entregó todo

    private bool misionCompletada = false;

    public void Interact()
    {
        // Misión ya terminada del todo
        if (misionCompletada)
        {
            managerInteraccion.StartDialogue(nodoMisionCompleta);
            return;
        }

        // Jugador vuelve con la entrevista lista
        if (managerCamara.entrevistaCompletada)
        {
            managerInteraccion.StartDialogue(nodoEntrega);
            // El nodo de entrega debe tener un evento que llame a EntregarEntrevista()
            return;
        }

        // Misión en curso (ya tiene cámara)
        if (managerCamara.tieneCamara)
        {
            managerInteraccion.StartDialogue(nodoMisionActiva);
            return;
        }

        // Primera interacción: dar cámara
        managerInteraccion.StartDialogue(nodoInicio);
        // El nodo de inicio debe tener un evento que llame a IniciarMision()
    }

    // Llamado desde el evento del nodo de diálogo de inicio
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

        // Aquí podés disparar lógica según el resultado
        if (puntaje >= managerCamara.puntajeMinimoExito)
        {
            Debug.Log($"¡Entrevista exitosa! Puntaje: {puntaje}");
            // Desbloquear siguiente capítulo, dar recompensa, etc.
        }
        else
        {
            Debug.Log($"Entrevista mediocre. Puntaje: {puntaje}");
        }
    }
}