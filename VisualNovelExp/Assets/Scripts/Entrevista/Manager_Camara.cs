using UnityEngine;
using TMPro;

public class Manager_Camara : MonoBehaviour
{
    public static Manager_Camara Instance;

    [Header("Estado misión")]
    public bool tieneCamara = false;
    public bool permisoObtenido = false;
    public bool entrevistaCompletada = false;
    public int puntajeEntrevista = 0;
    public int puntajeMinimoExito = 3; // Movido aquí desde EntrevistaData

    [Header("UI - HUD")]
    public GameObject iconoCamara;
    public GameObject panelTextoGrabar;    // Panel que contiene el texto de grabar
    public TextMeshProUGUI textoGrabar;    // "Presioná R para grabar"
    public TextMeshProUGUI textoEstado;    // Estado actual de la misión

    [Header("UI - Panel Item Cámara")]
    public GameObject panelItemRecibido;         // Aparece brevemente al recibir la cámara
    public TextMeshProUGUI textoItemRecibido;     // "¡Recibiste: Cámara!"

    [Header("NPC Entrevistado")]
    public Interaccion_NPC npcEntrevistado;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Estado inicial: todo oculto
        iconoCamara.SetActive(false);
        panelTextoGrabar.SetActive(false);

        if (panelItemRecibido != null)
            panelItemRecibido.SetActive(false);
    }

    public void RecibirCamara()
    {
        Debug.Log("RecibirCamara llamado correctamente");

        tieneCamara = true;
        iconoCamara.SetActive(true);
        ActualizarEstado("Obtuviste la camara, ve con Sakamoto-San");
        // "Tenés la cámara - ¡Encontrá a Sakamoto-san!"

        // Mostrar popup de item recibido
        if (panelItemRecibido != null)
            StartCoroutine(MostrarPopupItem("カメラ (Cámara)"));
    }

    public void ObtenerPermiso()
    {
        Debug.Log(" ObtenerPermiso() llamado correctamente");
        permisoObtenido = true;
        panelTextoGrabar.SetActive(true);

        if (textoGrabar != null)
            textoGrabar.text = "【R】で撮影開始  /  Presioná R para grabar";

        ActualizarEstado("インタビューを始めよう！");
        // "¡Empecemos la entrevista!"
    }

    public void CompletarEntrevista(int puntaje)
    {
        entrevistaCompletada = true;
        puntajeEntrevista = puntaje;
        panelTextoGrabar.SetActive(false);

        // Feedback según puntaje
        if (puntaje >= puntajeMinimoExito)
            ActualizarEstado("✓ 良いインタビューだった！監督に戻ろう");
        // "¡Fue una buena entrevista! Volvé al Director"
        else
            ActualizarEstado("△ インタビュー終了。監督に戻ろう");
        // "Entrevista terminada. Volvé al Director"
    }

    public void EntregarEntrevista()
    {
        tieneCamara = false;
        iconoCamara.SetActive(false);
        ActualizarEstado("ミッション完了！");
        // "¡Misión completa!"
    }

    public void Resetear()
    {
        tieneCamara = false;
        permisoObtenido = false;
        entrevistaCompletada = false;
        puntajeEntrevista = 0;
        iconoCamara.SetActive(false);
        panelTextoGrabar.SetActive(false);
        ActualizarEstado("");
    }

    void ActualizarEstado(string mensaje)
    {
        if (textoEstado != null)
            textoEstado.text = mensaje;
    }

    System.Collections.IEnumerator MostrarPopupItem(string nombreItem)
    {
        panelItemRecibido.SetActive(true);
        if (textoItemRecibido != null)
            textoItemRecibido.text = $"Item Recibido！\n{nombreItem}";

        yield return new WaitForSeconds(2.5f);

        panelItemRecibido.SetActive(false);
    }
}