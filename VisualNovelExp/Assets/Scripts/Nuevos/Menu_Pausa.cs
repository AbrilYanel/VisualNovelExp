using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu_Pausa : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI textoTitulo;
    public Button botonContinuar;
    public Button botonSalir;

    bool abierto;
    bool sesionTerminada;

    void Awake()
    {
        if (panel) panel.SetActive(false);
        if (botonContinuar) botonContinuar.onClick.AddListener(Cerrar);
        if (botonSalir) botonSalir.onClick.AddListener(Salir);
    }

    void OnEnable() { EventosJuego.AlOcurrir += AlEvento; }
    void OnDisable() { EventosJuego.AlOcurrir -= AlEvento; }

    void AlEvento(string evento, string detalle)
    {
        if (evento != "limite_sesion") return;
        sesionTerminada = true;
        Abrir();
    }

    void Update()
    {
        if (sesionTerminada || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (abierto) { Cerrar(); return; }
        if (EstadoUI.Bloqueada) return;                          // ESC de minijuegos
        if (Time.frameCount - EstadoUI.FrameLiberada < 2) return; // el mismo ESC que cerró un minijuego
        Abrir();
    }

    void Abrir()
    {
        abierto = true;
        panel.SetActive(true);
        if (textoTitulo)
            textoTitulo.text = sesionTerminada ? "Terminó el tiempo de la sesión. ¡Gracias por jugar!" : "Pausa";
        if (botonContinuar) botonContinuar.gameObject.SetActive(!sesionTerminada);
        if (Manager_Interaccion.I) Manager_Interaccion.I.BloquearJugador();
        Time.timeScale = 0f;
        EventosJuego.Emitir("pausa_abierta");
    }

    void Cerrar()
    {
        if (sesionTerminada) return;
        abierto = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
        if (Manager_Interaccion.I) Manager_Interaccion.I.LiberarJugador();
        EventosJuego.Emitir("pausa_cerrada");
    }

    void Salir()
    {
        EventosJuego.Emitir(sesionTerminada ? "salir_fin_sesion" : "salir_voluntario");
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}