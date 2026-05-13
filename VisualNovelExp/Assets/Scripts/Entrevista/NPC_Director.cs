using UnityEngine;

public class NPC_Director : MonoBehaviour
{
    [Header("Referencias")]
    public Manager_Interaccion managerInteraccion;
    public Manager_Camara managerCamara;

    [Header("Nodos de diálogo")]
    public Nodo_Dialogo nodoInicio;           
    public Nodo_Dialogo nodoMisionActiva;     
    public Nodo_Dialogo nodoEntrega;          
    public Nodo_Dialogo nodoMisionCompleta;   

    private bool misionCompletada = false;
    public PlayerProgress playerProgress;
    public int interaccionesAlCompletar = 2;
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

            // Actualizar UI
            managerInteraccion.ActualizarUIProgreso();

            Debug.Log($" Misión completada. Nivel actual: {playerProgress.nivelActual}");
        }
        else if (!exitosa)
        {
            Debug.Log("Entrevista entregada pero puntaje insuficiente");
        }
    }
}