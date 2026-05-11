using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC_Entrevistado : MonoBehaviour
{
    [Header("Referencias")]
    public Manager_Interaccion managerInteraccion;
    public Manager_Camara managerCamara;
    public Manager_Entrevista managerEntrevista;

    [Header("Nodos de diálogo")]
    public Nodo_Dialogo nodoSinCamara;       // "¿Qué querés?"
    public Nodo_Dialogo nodoConCamara;       // "Ah, venís de parte del Director..."
    public Nodo_Dialogo nodoYaEntrevistado;  // "Gracias por la entrevista"

    [Header("Indicador de acción")]
    // Este es el prompt "Presioná R" que aparece cuando el jugador se acerca
    public GameObject indicadorAccion;
    private bool jugadorCerca = false;

    void Start()
    {
        if (indicadorAccion != null)
            indicadorAccion.SetActive(false);
    }

    public void Interact()
    {
        // Si ya terminó la entrevista
        if (managerCamara.entrevistaCompletada)
        {
            managerInteraccion.StartDialogue(nodoYaEntrevistado);
            return;
        }

        // Si no tiene cámara
        if (!managerCamara.tieneCamara)
        {
            managerInteraccion.StartDialogue(nodoSinCamara);
            return;
        }

        // Si ya tiene permiso, solo esperamos que presione R
        if (managerCamara.permisoObtenido)
            return;

        // Primera vez con cámara: dar permiso
        managerInteraccion.StartDialogue(nodoConCamara);
        // El nodo debe tener un evento que llame a DarPermiso()
    }

    // Llamado desde el evento del nodo de diálogo "nodoConCamara"
    public void DarPermiso()
    {
        managerCamara.ObtenerPermiso();
    }

    void Update()
    {
        if (!managerCamara.permisoObtenido) return;
        if (managerCamara.entrevistaCompletada) return;
        // Detección de proximidad del jugador
        float distancia = Vector3.Distance(
            transform.position,
            Camera.main.transform.position
        );

        jugadorCerca = distancia < 3f;

        // Mostrar/ocultar indicador de "Presioná R"
        if (indicadorAccion != null)
        {
            bool mostrarIndicador = jugadorCerca
                && managerCamara.permisoObtenido
                && !managerCamara.entrevistaCompletada;

            indicadorAccion.SetActive(mostrarIndicador);
        }

        // Iniciar entrevista con R
        if (jugadorCerca
            && managerCamara.permisoObtenido
            && !managerCamara.entrevistaCompletada
            && Input.GetKeyDown(KeyCode.R))
        {
            managerEntrevista.IniciarEntrevista();
        }
    }
}