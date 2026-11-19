using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Entrevista : MonoBehaviour
{
    [Header("Data")]
    public EntrevistaData entrevistaData;

    [Header("Referencias")]
    public Manager_Interaccion managerInteraccion;
    public Manager_Camara managerCamara;

    [Header("UI - Panel Principal")]
    public GameObject panelEntrevista;
    public TextMeshProUGUI textoNombreEntrevistado;
    public TextMeshProUGUI textoPreguntaJapones;
    public TextMeshProUGUI textoPreguntaEspanol;
    public TextMeshProUGUI textoRespuestaNPC;
    public TextMeshProUGUI textoProgreso;
    public TextMeshProUGUI textoPuntaje;

    [Header("UI - Reacción NPC")]
    public Image imagenReaccionNPC;
    public Sprite spritefeliz;
    public Sprite spriteNeutral;
    public Sprite spriteIncomodo;
    public Sprite spriteEnojado;

    [Header("UI - Opciones")]
    public Transform contenedorOpciones;
    public GameObject botonOpcionPrefab;

    [Header("UI - Resultado Final")]
    public GameObject panelResultado;
    public TextMeshProUGUI textoResultadoTitulo;
    public TextMeshProUGUI textoResultadoPuntaje;
    public TextMeshProUGUI textoResultadoMensaje;
    public Button botonCerrarResultado;

    [Header("UI - Feedback de opción")]
    public GameObject panelFeedback;
    public TextMeshProUGUI textoFeedback;
    public Image imagenFeedbackBG;

    [Header("Colores")]
    public Color colorCorrecta = new Color(0.2f, 0.8f, 0.2f);
    public Color colorIncorrecta = new Color(0.9f, 0.2f, 0.2f);
    public Color colorNormal = Color.white;

    [Header("Tiempos")]
    public float segundosEntrePreguntas = 3f;

    // Estado interno
    private int indiceActual = 0;
    private int puntajeTotal = 0;
    private bool esperandoSiguiente = false;
    private bool activa = false;

    Manager_Interaccion Interaccion => Manager_Interaccion.I != null ? Manager_Interaccion.I : managerInteraccion;

    int PuntajeMinimo => managerCamara != null ? managerCamara.puntajeMinimoExito
                       : (entrevistaData != null ? entrevistaData.puntajeMinimoExito : 0);

    // ─────────────────────────────────────────
    void Start()
    {
        if (entrevistaData == null)
            entrevistaData = Resources.Load<EntrevistaData>("Sakamoto");

        if (panelEntrevista != null) panelEntrevista.SetActive(false);
        if (panelResultado != null) panelResultado.SetActive(false);
        if (panelFeedback != null) panelFeedback.SetActive(false);
        if (botonCerrarResultado != null)
            botonCerrarResultado.onClick.AddListener(CerrarPanelResultado);
    }

    // ─────────────────────────────────────────
    public void IniciarEntrevista()
    {
        if (activa) return;

        if (entrevistaData == null)
            entrevistaData = Resources.Load<EntrevistaData>("Sakamoto");

        if (entrevistaData == null || entrevistaData.preguntas == null || entrevistaData.preguntas.Count == 0)
        {
            Debug.LogError("[Manager_Entrevista] No hay EntrevistaData o no tiene preguntas", this);
            return;
        }
        if (panelEntrevista == null)
        {
            Debug.LogError("[Manager_Entrevista] Falta asignar panelEntrevista", this);
            return;
        }

        activa = true;
        indiceActual = 0;
        puntajeTotal = 0;
        esperandoSiguiente = false;

        var mi = Interaccion;
        if (mi != null)
        {
            if (mi.dialoguePanel) mi.dialoguePanel.SetActive(false);
            if (mi.choicePanel) mi.choicePanel.SetActive(false);
            mi.BloquearJugador(); // congela al jugador, pausa las necesidades y muestra el cursor
        }

        panelEntrevista.SetActive(true);
        if (panelResultado) panelResultado.SetActive(false);
        if (textoNombreEntrevistado) textoNombreEntrevistado.text = entrevistaData.nombreEntrevistado;

        EventosJuego.Emitir("entrevista_inicio");
        MostrarPregunta(entrevistaData.preguntas[indiceActual]);
    }

    // ─────────────────────────────────────────
    void MostrarPregunta(PreguntaEntrevista pregunta)
    {
        esperandoSiguiente = false;
        if (textoRespuestaNPC) textoRespuestaNPC.text = "";
        if (textoProgreso) textoProgreso.text = $"Pregunta {indiceActual + 1}/{entrevistaData.preguntas.Count}";
        if (textoPuntaje) textoPuntaje.text = $"Confianza: {puntajeTotal} pts";

        if (textoPreguntaJapones) textoPreguntaJapones.text = pregunta.preguntaJapones;
        if (textoPreguntaEspanol) textoPreguntaEspanol.text = $"({pregunta.preguntaEspanol})";

        if (panelFeedback != null) panelFeedback.SetActive(false);
        ActualizarReaccionNPC(ReaccionNPC.Neutral);

        foreach (Transform t in contenedorOpciones)
            Destroy(t.gameObject);

        foreach (var opcion in pregunta.opciones)
        {
            OpcionEntrevista opcionLocal = opcion;
            GameObject obj = Instantiate(botonOpcionPrefab, contenedorOpciones);

            TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = opcion.textoOpcion;

            Button btn = obj.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => SeleccionarOpcion(opcionLocal, obj));

            Image imgBtn = obj.GetComponent<Image>();
            if (imgBtn != null) imgBtn.color = colorNormal;
        }
    }

    // ─────────────────────────────────────────
    void SeleccionarOpcion(OpcionEntrevista opcion, GameObject botonObj)
    {
        if (esperandoSiguiente) return;
        esperandoSiguiente = true;

        Image imgBoton = botonObj.GetComponent<Image>();
        if (imgBoton != null)
            imgBoton.color = opcion.esCorrecta ? colorCorrecta : colorIncorrecta;

        foreach (Transform t in contenedorOpciones)
        {
            Button btn = t.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }

        puntajeTotal += opcion.puntaje;
        if (textoPuntaje) textoPuntaje.text = $"Confianza: {puntajeTotal} pts";

        if (textoRespuestaNPC) textoRespuestaNPC.text = opcion.respuestaDelNPC;
        ActualizarReaccionNPC(opcion.reaccion);
        MostrarFeedback(opcion.esCorrecta);

        EventosJuego.Emitir("entrevista_respuesta", $"{indiceActual + 1}:{opcion.puntaje}");
        StartCoroutine(EsperarSiguiente());
    }

    // ─────────────────────────────────────────
    void MostrarFeedback(bool correcto)
    {
        if (panelFeedback == null) return;
        panelFeedback.SetActive(true);

        if (textoFeedback != null)
            textoFeedback.text = correcto ? "¡Bien dicho!" : "No era esa";

        if (imagenFeedbackBG != null)
            imagenFeedbackBG.color = correcto
                ? new Color(0.2f, 0.8f, 0.2f, 0.8f)
                : new Color(0.9f, 0.2f, 0.2f, 0.8f);
    }

    // ─────────────────────────────────────────
    void ActualizarReaccionNPC(ReaccionNPC reaccion)
    {
        if (imagenReaccionNPC == null) return;

        imagenReaccionNPC.sprite = reaccion switch
        {
            ReaccionNPC.Feliz => spritefeliz,
            ReaccionNPC.Neutral => spriteNeutral,
            ReaccionNPC.Incomodo => spriteIncomodo,
            ReaccionNPC.Enojado => spriteEnojado,
            _ => spriteNeutral
        };
    }

    // ─────────────────────────────────────────
    IEnumerator EsperarSiguiente()
    {
        yield return new WaitForSeconds(segundosEntrePreguntas);

        indiceActual++;
        if (indiceActual >= entrevistaData.preguntas.Count)
        {
            TerminarEntrevista();
            yield break;
        }

        MostrarPregunta(entrevistaData.preguntas[indiceActual]);
    }

    // ─────────────────────────────────────────
    public void TerminarEntrevista()
    {
        if (panelEntrevista) panelEntrevista.SetActive(false);

        if (managerCamara) managerCamara.CompletarEntrevista(puntajeTotal);
        EventosJuego.Emitir("entrevista_fin", puntajeTotal.ToString());

        // La entrevista ocupa una franja del día, como cualquier actividad
        if (Manager_Dia.I) Manager_Dia.I.UsarFranja("entrevista");

        MostrarResultadoFinal();
    }

    // ─────────────────────────────────────────
    void MostrarResultadoFinal()
    {
        if (panelResultado == null)
        {
            CerrarPanelResultado(); // sin panel de resultado, se libera al jugador enseguida
            return;
        }

        panelResultado.SetActive(true);
        bool exito = puntajeTotal >= PuntajeMinimo;

        if (textoResultadoTitulo != null)
            textoResultadoTitulo.text = exito ? "¡Entrevista exitosa!" : "Entrevista terminada";

        if (textoResultadoPuntaje != null)
            textoResultadoPuntaje.text = $"Puntaje: {puntajeTotal} / {entrevistaData.puntajeMaximo}";

        if (textoResultadoMensaje != null)
            textoResultadoMensaje.text = ObtenerMensajeFinal(puntajeTotal);
    }

    string ObtenerMensajeFinal(int puntaje)
    {
        float porcentaje = (float)puntaje / Mathf.Max(1, entrevistaData.puntajeMaximo);

        if (porcentaje >= 0.8f)
            return "¡Excelente! Futaba quedó muy contenta. Lleva la entrevista al Director.";
        if (porcentaje >= 0.5f)
            return "Bien. La entrevista sirve para el boletín, aunque podría haber salido mejor. Llévala al Director.";
        return "Ánimo. Repasa las palabras de tu journal (TAB). Igual puedes llevar la entrevista al Director.";
    }

    // ─────────────────────────────────────────
    void CerrarPanelResultado()
    {
        if (panelResultado != null) panelResultado.SetActive(false);
        activa = false;

        var mi = Interaccion;
        if (mi != null) mi.LiberarJugador();
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}