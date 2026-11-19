using UnityEngine;
using TMPro;

public class Manager_Camara : MonoBehaviour
{
    public static Manager_Camara Instance;

    [Header("Progreso (guarda la misión entre escenas)")]
    public PlayerProgress playerProgress;

    [Header("Reglas")]
    [Tooltip("Puntaje mínimo para que la entrevista sea exitosa. Es el único valor que usa todo el juego")]
    public int puntajeMinimoExito = 6;

    [Header("UI - HUD")]
    public GameObject iconoCamara;
    public GameObject panelTextoGrabar;    // Panel que contiene el texto de grabar
    public TextMeshProUGUI textoGrabar;
    public TextMeshProUGUI textoEstado;    // Estado actual de la misión

    [Header("UI - Panel Item Cámara")]
    public GameObject panelItemRecibido;          // Aparece brevemente al recibir la cámara
    public TextMeshProUGUI textoItemRecibido;

    [Header("NPC Entrevistado")]
    public Interaccion_NPC npcEntrevistado;

    const string TEXTO_GRABAR = "Presiona R cerca de Sakamoto-san para empezar la entrevista";

    // ---------- Estado: vive en PlayerProgress para no perderse al ir a casa y volver ----------

    public bool tieneCamara
    {
        get => playerProgress != null && playerProgress.tieneCamara;
        set { if (playerProgress != null) playerProgress.tieneCamara = value; }
    }

    public bool permisoObtenido
    {
        get => playerProgress != null && playerProgress.permisoObtenido;
        set { if (playerProgress != null) playerProgress.permisoObtenido = value; }
    }

    public bool entrevistaCompletada
    {
        get => playerProgress != null && playerProgress.entrevistaCompletada;
        set { if (playerProgress != null) playerProgress.entrevistaCompletada = value; }
    }

    public int puntajeEntrevista
    {
        get => playerProgress != null ? playerProgress.puntajeEntrevista : 0;
        set { if (playerProgress != null) playerProgress.puntajeEntrevista = value; }
    }

    // ---------- Ciclo de vida ----------

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (playerProgress == null)
            Debug.LogError("[Manager_Camara] Falta asignar Player Progress", this);

        if (panelItemRecibido) panelItemRecibido.SetActive(false);
        RestaurarHUD();
    }

    // Al entrar a la ciudad, el HUD muestra el punto de la misión en que quedó el jugador
    void RestaurarHUD()
    {
        bool misionTerminada = playerProgress != null && playerProgress.festivalCompletado;

        if (iconoCamara) iconoCamara.SetActive(tieneCamara && !misionTerminada);

        bool puedeGrabar = permisoObtenido && !entrevistaCompletada;
        if (panelTextoGrabar) panelTextoGrabar.SetActive(puedeGrabar);
        if (puedeGrabar && textoGrabar) textoGrabar.text = TEXTO_GRABAR;

        if (misionTerminada) ActualizarEstado("");
        else if (entrevistaCompletada) ActualizarEstado("Lleva la entrevista al Director");
        else if (permisoObtenido) ActualizarEstado("Entrevista a Sakamoto-san");
        else if (tieneCamara) ActualizarEstado("Tienes la cámara: busca a Sakamoto-san");
        else ActualizarEstado("");
    }

    // ---------- Pasos de la misión ----------

    public void RecibirCamara()
    {
        tieneCamara = true;
        if (iconoCamara) iconoCamara.SetActive(true);
        ActualizarEstado("Tienes la cámara: busca a Sakamoto-san");
        EventosJuego.Emitir("mision_camara");

        if (panelItemRecibido != null)
            StartCoroutine(MostrarPopupItem("カメラ (kamera) · cámara"));
    }

    public void ObtenerPermiso()
    {
        permisoObtenido = true;
        if (panelTextoGrabar) panelTextoGrabar.SetActive(true);
        if (textoGrabar) textoGrabar.text = TEXTO_GRABAR;
        ActualizarEstado("Entrevista a la Chica en frente");
        EventosJuego.Emitir("mision_permiso");
    }

    public void CompletarEntrevista(int puntaje)
    {
        entrevistaCompletada = true;
        puntajeEntrevista = puntaje;
        if (panelTextoGrabar) panelTextoGrabar.SetActive(false);

        ActualizarEstado(puntaje >= puntajeMinimoExito
            ? "¡Buena entrevista! Llévala al Director"
            : "Entrevista terminada. Llévala al Director");
    }

    public void EntregarEntrevista()
    {
        tieneCamara = false;
        if (iconoCamara) iconoCamara.SetActive(false);
        ActualizarEstado("¡Misión completa! El boletín del festival está listo");
    }

    public void Resetear()
    {
        tieneCamara = false;
        permisoObtenido = false;
        entrevistaCompletada = false;
        puntajeEntrevista = 0;
        if (iconoCamara) iconoCamara.SetActive(false);
        if (panelTextoGrabar) panelTextoGrabar.SetActive(false);
        ActualizarEstado("");
    }

    // ---------- Auxiliares ----------

    void ActualizarEstado(string mensaje)
    {
        if (textoEstado != null) textoEstado.text = mensaje;
    }

    System.Collections.IEnumerator MostrarPopupItem(string nombreItem)
    {
        panelItemRecibido.SetActive(true);
        if (textoItemRecibido != null)
            textoItemRecibido.text = $"¡Objeto recibido!\n{nombreItem}";

        yield return new WaitForSeconds(2.5f);

        panelItemRecibido.SetActive(false);
    }
}