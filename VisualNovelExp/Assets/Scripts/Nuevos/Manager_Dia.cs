using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Dia : MonoBehaviour
{
    public static Manager_Dia I;

    public PlayerProgress playerProgress;
    public JournalData journalData;

    [Header("Ritmo (se calibra en el piloto)")]
    public int franjasPorDia = 3;
    public int diaDelFestival = 4;
    public string[] nombresFranjas = { "mañana", "mediodía", "tarde" };

    [Header("Lo que baja cada franja")]
    public float hambrePorFranja = 20f;
    public float energiaPorFranja = 15f;
    public float diversionPorFranja = 15f;

    [Header("Resumen al dormir")]
    public GameObject panelResumen;
    public TextMeshProUGUI textoResumen;
    public Button botonContinuar;

    public int Dia => playerProgress ? playerProgress.dia : 1;
    public bool EsDeNoche => playerProgress != null && playerProgress.franjasUsadas >= franjasPorDia;
    public string Franja
    {
        get
        {
            if (EsDeNoche) return "noche";
            if (playerProgress == null || nombresFranjas.Length == 0) return "";
            return nombresFranjas[Mathf.Clamp(playerProgress.franjasUsadas, 0, nombresFranjas.Length - 1)];
        }
    }

    void Awake()
    {
        I = this;
        if (panelResumen) panelResumen.SetActive(false);
        if (botonContinuar) botonContinuar.onClick.AddListener(CerrarResumen);
    }

    void OnDestroy() { if (I == this) I = null; }

    // Lo llama Manager_Interaccion al terminar cada minijuego
    public void UsarFranja(string motivo)
    {
        if (playerProgress == null || EsDeNoche) return;
        playerProgress.franjasUsadas++;

        var nec = Manager_Necesidades.I;
        if (nec)
        {
            nec.Sumar(Necesidad.Hambre, -hambrePorFranja);
            nec.Sumar(Necesidad.Energia, -energiaPorFranja);
            nec.Sumar(Necesidad.Diversion, -diversionPorFranja);
        }

        EventosJuego.Emitir("franja_usada", $"dia{Dia}:{playerProgress.franjasUsadas}:{motivo}");
        if (Manager_Interaccion.I) Manager_Interaccion.I.ActualizarUIProgreso(); // apaga los ! de noche

        if (Mensaje_UI.I)
        {
            if (EsDeNoche)
                Mensaje_UI.I.Mostrar("Se hizo de noche. Los vecinos ya no atienden: vuelve a casa a dormir. (La konbini abre las 24 horas.)", 5f);
            else
                Mensaje_UI.I.Mostrar($"Día {Dia} · {Franja}", 3f);
        }
    }

    // Lo llama la cama al terminar el fundido
    public void Dormir()
    {
        if (playerProgress == null) return;

        int palabras = journalData ? journalData.palabrasAprendidas.Count : 0;
        int palabrasNuevas = Mathf.Max(0, palabras - playerProgress.palabrasInicioDia);
        int diferencia = playerProgress.monedas - playerProgress.monedasInicioDia;
        string signo = diferencia >= 0 ? "+" : "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>Fin del día {Dia}</b>");
        sb.AppendLine();
        sb.AppendLine($"Palabras nuevas: {palabrasNuevas}  (total: {palabras})");
        sb.AppendLine($"Monedas: {playerProgress.monedas}  ({signo}{diferencia})");
        sb.AppendLine($"Comida en casa: {playerProgress.comida}");
        var nec = Manager_Necesidades.I;
        if (nec) sb.AppendLine($"Hambre {nec.hambre:0} · Energía {nec.energia:0} · Diversión {nec.diversion:0}");
        sb.AppendLine();
        sb.AppendLine(TextoFestival(Dia + 1));

        EventosJuego.Emitir("dia_fin", $"dia{Dia}:palabras{palabrasNuevas}:monedas{diferencia}");

        // Empieza el día siguiente
        playerProgress.dia++;
        playerProgress.franjasUsadas = 0;
        playerProgress.monedasInicioDia = playerProgress.monedas;
        playerProgress.palabrasInicioDia = palabras;

        textoResumen.text = sb.ToString();
        panelResumen.SetActive(true);
        if (Manager_Interaccion.I) Manager_Interaccion.I.BloquearJugador();
    }

    string TextoFestival(int diaSiguiente)
    {
        if (playerProgress.festivalCompletado)
            return "El festival ya pasó. Puedes seguir explorando el barrio todo lo que quieras.";
        int faltan = diaDelFestival - diaSiguiente;
        if (faltan > 1) return $"Faltan {faltan} días para el festival.";
        if (faltan == 1) return "Mañana es el festival: prepara la entrevista para el boletín.";
        if (faltan == 0) return "¡Hoy es el festival! Lleva la entrevista al Director.";
        return "El festival ya empezó: todavía puedes entregar la entrevista al Director.";
    }

    void CerrarResumen()
    {
        panelResumen.SetActive(false);
        if (Manager_Interaccion.I)
        {
            Manager_Interaccion.I.LiberarJugador();
            Manager_Interaccion.I.ActualizarUIProgreso();
        }
        EventosJuego.Emitir("dia_inicio", Dia.ToString());
        if (Mensaje_UI.I) Mensaje_UI.I.Mostrar($"Día {Dia} · {Franja}", 3f);
    }
}