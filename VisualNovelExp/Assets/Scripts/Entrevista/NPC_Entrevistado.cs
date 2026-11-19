using UnityEngine;

public class NPC_Entrevistado : MonoBehaviour
{
    [Header("Referencias")]
    public Manager_Interaccion managerInteraccion;
    public Manager_Camara managerCamara;
    public Manager_Entrevista managerEntrevista;

    [Header("Nodos de diálogo")]
    [Tooltip("Todavía no tiene la cámara")]
    public Nodo_Dialogo nodoSinCamara;
    [Tooltip("Llega con la cámara: acepta la entrevista (marcar Da Permiso)")]
    public Nodo_Dialogo nodoConCamara;
    [Tooltip("Después de la entrevista")]
    public Nodo_Dialogo nodoYaEntrevistado;

    [Header("Indicador de acción")]
    [Tooltip("Aviso 'Presiona R' que aparece al acercarse")]
    public GameObject indicadorAccion;
    public float distanciaParaGrabar = 3f;

    void Start()
    {
        if (indicadorAccion != null) indicadorAccion.SetActive(false);
    }

    public void Interact()
    {
        if (managerCamara.entrevistaCompletada)
        {
            managerInteraccion.StartDialogue(nodoYaEntrevistado);
            return;
        }

        if (!managerCamara.tieneCamara)
        {
            managerInteraccion.StartDialogue(nodoSinCamara);
            return;
        }

        // Ya dio permiso: la entrevista empieza con R
        if (managerCamara.permisoObtenido)
        {
            if (Mensaje_UI.I) Mensaje_UI.I.Mostrar("Presiona R para grabar.", 3f);
            return;
        }

        managerInteraccion.StartDialogue(nodoConCamara);
    }

    // También lo puede llamar un evento del nodo "nodoConCamara"
    public void DarPermiso()
    {
        managerCamara.ObtenerPermiso();
    }

    void Update()
    {
        bool disponible = managerCamara != null
                          && managerCamara.permisoObtenido
                          && !managerCamara.entrevistaCompletada;

        if (!disponible)
        {
            if (indicadorAccion && indicadorAccion.activeSelf) indicadorAccion.SetActive(false);
            return;
        }

        var cam = Camera.main;
        if (cam == null) return;

        bool cerca = Vector3.Distance(transform.position, cam.transform.position) < distanciaParaGrabar;
        bool libre = !EstadoUI.Bloqueada; // no durante diálogos, minijuegos ni paneles

        if (indicadorAccion) indicadorAccion.SetActive(cerca && libre);

        if (cerca && libre && Input.GetKeyDown(KeyCode.R))
            managerEntrevista.IniciarEntrevista();
    }
}