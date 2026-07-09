using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Minijuego "rasca y gana" simple para carteles wa/wo/n
public class Manager_LimpiezaCartel : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelLimpieza;
    public Image imagenKanaFondo; // muestra el kana limpio debajo
    public RawImage capaSucia;    // imagen gris semitransparente encima, con RaycastTarget true
    public Slider sliderProgreso;
    public TextMeshProUGUI textoKana;
    public TextMeshProUGUI textoProgreso;
    public Button botonCerrar;

    [Header("Ajustes")]
    [Range(0.1f, 2f)] public float velocidadLimpieza = 0.8f; // % por segundo manteniendo click
    public float objetivo = 85f; // % para completar

    private Cartel_Kana cartelActual;
    private float progreso = 0f;
    private bool limpiando = false;
    private MonoBehaviour playerMovement;
    private MonoBehaviour cameraController;

    void Start()
    {
        if (panelLimpieza) panelLimpieza.SetActive(false);
        if (botonCerrar) botonCerrar.onClick.AddListener(Cancelar);
        // buscar referencias de movimiento para congelar
        var inter = FindObjectOfType<Manager_Interaccion>();
        if (inter != null)
        {
            playerMovement = inter.Player_Movement;
            cameraController = inter.cameraController;
        }
    }

    public void IniciarLimpieza(Cartel_Kana cartel)
    {
        cartelActual = cartel;
        progreso = 0f;
        limpiando = true;

        if (textoKana) textoKana.text = cartel.nombreMostrar;
        ActualizarUI();

        if (panelLimpieza) panelLimpieza.SetActive(true);

        // congelar jugador
        if (playerMovement) playerMovement.enabled = false;
        if (cameraController) cameraController.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // reset visual suciedad
        if (capaSucia)
        {
            var c = capaSucia.color;
            c.a = 0.92f;
            capaSucia.color = c;
            capaSucia.raycastTarget = true;
        }
    }

    void Update()
    {
        if (!limpiando || panelLimpieza == null || !panelLimpieza.activeSelf) return;

        // limpiar manteniendo click izquierdo sobre la capa sucia
        bool mouseSobre = false;
        if (capaSucia && RectTransformUtility.RectangleContainsScreenPoint(
            capaSucia.rectTransform, Input.mousePosition, null))
        {
            mouseSobre = true;
        }

        if (mouseSobre && Input.GetMouseButton(0))
        {
            progreso += velocidadLimpieza * 60f * Time.deltaTime; // ~ % por segundo
            progreso = Mathf.Clamp(progreso, 0f, 100f);

            // fade visual de la capa sucia
            if (capaSucia)
            {
                var col = capaSucia.color;
                col.a = Mathf.Lerp(0.92f, 0.05f, progreso / 100f);
                capaSucia.color = col;
            }

            ActualizarUI();

            if (progreso >= objetivo)
            {
                Completar();
            }
        }

        // tecla ESC cancela
        if (Input.GetKeyDown(KeyCode.Escape))
            Cancelar();
    }

    void ActualizarUI()
    {
        if (sliderProgreso) sliderProgreso.value = progreso / 100f;
        if (textoProgreso) textoProgreso.text = $"Limpiando... {Mathf.RoundToInt(progreso)}%";
    }

    void Completar()
    {
        limpiando = false;
        if (textoProgreso) textoProgreso.text = "¡Kana revelado!";
        if (cartelActual != null)
            cartelActual.Desbloquear();

        Invoke(nameof(Cerrar), 1.0f);
    }

    public void Cancelar()
    {
        limpiando = false;
        Cerrar();
    }

    void Cerrar()
    {
        if (panelLimpieza) panelLimpieza.SetActive(false);
        if (playerMovement) playerMovement.enabled = true;
        if (cameraController) cameraController.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cartelActual = null;
    }
}
