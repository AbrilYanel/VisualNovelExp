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
    public GameObject panelFeedback;         // Panel que muestra si fue correcto/incorrecto
    public TextMeshProUGUI textoFeedback;    // "¡Correcto!" / "Incorrecto"
    public Image imagenFeedbackBG;           // Fondo del feedback

    [Header("Colores")]
    public Color colorCorrecta = new Color(0.2f, 0.8f, 0.2f);
    public Color colorIncorrecta = new Color(0.9f, 0.2f, 0.2f);
    public Color colorNormal = Color.white;

    // Estado interno
    private int indiceActual = 0;
    private int puntajeTotal = 0;
    private bool esperandoSiguiente = false;

    // ─────────────────────────────────────────
    void Start()
    {
        panelEntrevista.SetActive(false);

        if (panelResultado != null)
            panelResultado.SetActive(false);

        if (panelFeedback != null)
            panelFeedback.SetActive(false);

        if (botonCerrarResultado != null)
            botonCerrarResultado.onClick.AddListener(CerrarPanelResultado);
    }

    // ─────────────────────────────────────────
    public void IniciarEntrevista()
    {
        indiceActual = 0;
        puntajeTotal = 0;
        esperandoSiguiente = false;

        panelEntrevista.SetActive(true);
        textoNombreEntrevistado.text = entrevistaData.nombreEntrevistado;

        // Liberar cursor para hacer clic en opciones
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MostrarPregunta(entrevistaData.preguntas[indiceActual]);
    }

    // ─────────────────────────────────────────
    void MostrarPregunta(PreguntaEntrevista pregunta)
    {
        esperandoSiguiente = false;
        textoRespuestaNPC.text = "";
        textoProgreso.text = $"質問 {indiceActual + 1}/{entrevistaData.preguntas.Count}";
        textoPuntaje.text = $"関係: {puntajeTotal}pts";

        textoPreguntaJapones.text = pregunta.preguntaJapones;
        textoPreguntaEspanol.text = $"({pregunta.preguntaEspanol})";

        // Ocultar feedback anterior
        if (panelFeedback != null)
            panelFeedback.SetActive(false);

        // Limpiar botones anteriores
        foreach (Transform t in contenedorOpciones)
            Destroy(t.gameObject);

        // Crear botones de opciones
        foreach (var opcion in pregunta.opciones)
        {
            OpcionEntrevista opcionLocal = opcion;
            GameObject obj = Instantiate(botonOpcionPrefab, contenedorOpciones);

            // Asignar texto
            TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
                tmp.text = opcion.textoOpcion;

            // Asignar click
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(() => SeleccionarOpcion(opcionLocal, obj));

            // Estado inicial normal
            Image imgBtn = obj.GetComponent<Image>();
            if (imgBtn != null)
                imgBtn.color = colorNormal;
        }
    }

    // ─────────────────────────────────────────
    void SeleccionarOpcion(OpcionEntrevista opcion, GameObject botonObj)
    {
        if (esperandoSiguiente) return;
        esperandoSiguiente = true;

        // Colorear el botón seleccionado
        Image imgBoton = botonObj.GetComponent<Image>();
        if (imgBoton != null)
            imgBoton.color = opcion.esCorrecta ? colorCorrecta : colorIncorrecta;

        // Desactivar todos los botones
        foreach (Transform t in contenedorOpciones)
        {
            Button btn = t.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }

        // Actualizar puntaje
        puntajeTotal += opcion.puntaje;
        textoPuntaje.text = $"関係: {puntajeTotal}pts";

        // Mostrar respuesta y reacción del NPC
        textoRespuestaNPC.text = opcion.respuestaDelNPC;
        ActualizarReaccionNPC(opcion.reaccion);

        // Mostrar feedback de correcto/incorrecto
        MostrarFeedback(opcion.esCorrecta);

        StartCoroutine(EsperarSiguiente());
    }

    // ─────────────────────────────────────────
    void MostrarFeedback(bool correcto)
    {
        if (panelFeedback == null) return;

        panelFeedback.SetActive(true);

        if (textoFeedback != null)
            textoFeedback.text = correcto ? "✓ 正解！" : "✗ 不正解";
        // "¡Correcto!" / "Incorrecto"

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
        yield return new WaitForSeconds(2.5f);

        indiceActual++;

        if (indiceActual >= entrevistaData.preguntas.Count)
        {
            TerminarEntrevista();
            yield break;
        }

        MostrarPregunta(entrevistaData.preguntas[indiceActual]);
    }

    // ─────────────────────────────────────────
    void TerminarEntrevista()
    {
        // Ocultar panel de preguntas
        panelEntrevista.SetActive(false);

        // Mostrar resultado final
        MostrarResultadoFinal();

        // Notificar al manager
        managerCamara.CompletarEntrevista(puntajeTotal);
    }

    // ─────────────────────────────────────────
    void MostrarResultadoFinal()
    {
        if (panelResultado == null) return;

        panelResultado.SetActive(true);

        bool exito = puntajeTotal >= entrevistaData.puntajeMinimoExito;

        if (textoResultadoTitulo != null)
            textoResultadoTitulo.text = exito
                ? "インタビュー成功！🎉"   // ¡Entrevista exitosa!
                : "インタビュー終了";       // Entrevista terminada

        if (textoResultadoPuntaje != null)
            textoResultadoPuntaje.text =
                $"スコア: {puntajeTotal} / {entrevistaData.puntajeMaximo}";

        if (textoResultadoMensaje != null)
        {
            textoResultadoMensaje.text = ObtenerMensajeFinal(puntajeTotal);
        }
    }

    string ObtenerMensajeFinal(int puntaje)
    {
        // Ajustá los rangos según tu puntajeMaximo
        float porcentaje = (float)puntaje / entrevistaData.puntajeMaximo;

        if (porcentaje >= 0.8f)
            return "素晴らしい！\n¡Excelente entrevista! Sakamoto-san quedó muy contento.";
        else if (porcentaje >= 0.5f)
            return "まあまあ。\nEntrevista aceptable. Podría haber sido mejor.";
        else
            return "がんばれ！\nNecesitás practicar más el vocabulario.";
    }

    // ─────────────────────────────────────────
    void CerrarPanelResultado()
    {
        if (panelResultado != null)
            panelResultado.SetActive(false);

        // Restaurar cursor según tu sistema
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}