using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Panel_Personaje : MonoBehaviour
{
    public static Panel_Personaje I;

    public GameObject panel;
    public KeyCode tecla = KeyCode.C;
    public PlayerProgress playerProgress;
    public Slider barraHambre, barraEnergia, barraDiversion;
    public TextMeshProUGUI textoMonedas, textoComida, textoNivel;

    public bool Abierto => panel != null && panel.activeSelf;

    void Awake()
    {
        I = this;
        if (panel) panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(tecla))
        {
            if (Abierto) Cerrar();
            else if (!EstadoUI.Bloqueada) Abrir();
        }
        if (Abierto) Refrescar();
    }

    public void Abrir()
    {
        if (Abierto) return;
        panel.SetActive(true);
        if (Manager_Interaccion.I) Manager_Interaccion.I.BloquearJugador();
        Refrescar();
        EventosJuego.Emitir("panel_personaje_abierto");
    }

    public void Cerrar()
    {
        if (!Abierto) return;
        panel.SetActive(false);
        if (Manager_Interaccion.I) Manager_Interaccion.I.LiberarJugador();
        EventosJuego.Emitir("panel_personaje_cerrado");
    }

    void Refrescar()
    {
        if (Manager_Necesidades.I) Manager_Necesidades.I.ActualizarBarras(barraHambre, barraEnergia, barraDiversion);
        if (!playerProgress) return;
        if (textoMonedas) textoMonedas.text = $"Monedas: {playerProgress.monedas}";
        if (textoComida) textoComida.text = $"Comida en casa: {playerProgress.comida}";
        if (textoNivel) textoNivel.text = $"Nivel {playerProgress.nivelActual}" +
         (Manager_Dia.I ? $" · Día {Manager_Dia.I.Dia} · {Manager_Dia.I.Franja}" : "");
    }
}