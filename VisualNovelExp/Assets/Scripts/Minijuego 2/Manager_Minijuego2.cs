using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Minijuego2 : MonoBehaviour
{
    [Header("Referencia al manager principal")]
    public Manager_Interaccion interaccionManager;
    public Manager_Journal managerJournal;

    [Header("Preguntas")]
    public List<PreguntaData> preguntas;

    [Header("UI General")]
    public TextMeshProUGUI textoInstruccion;
    public TextMeshProUGUI textoFeedback;
    public TextMeshProUGUI textoProgreso;
    public Button botonConfirmar;
    public Button botonSiguiente;
    public TextMeshProUGUI textoBotonSiguiente;
    public GameObject minijuego2Panel;


    [Header("UI Fill in the Blank")]
    public GameObject panelFillBlank;
    public TextMeshProUGUI textoOracion;
    public Transform contenedorOpciones;
    public GameObject botonOpcionPrefab;

    [Header("UI Word Order")]
    public GameObject panelWordOrder;
    public TextMeshProUGUI textoEspanol;
    public Transform contenedorBloques;   // bloques disponibles (abajo)
    public Transform contenedorSlots;     // slots de respuesta (arriba)
    public GameObject bloquePrefab;
    public GameObject slotPrefab;

    [Header("Colores feedback")]
    public Color colorCorrecto = new Color(0.2f, 0.8f, 0.2f);
    public Color colorIncorrecto = new Color(0.9f, 0.2f, 0.2f);
    public Color colorNeutro = Color.white;

    private int indiceActual = 0;
    private int respuestasCorrectas = 0;
    private int opcionSeleccionada = -1;
    private bool esperandoSiguiente = false;

    // Entrada 
    public void Iniciar()
    {
        if (!ValidarReferencias()) return;

        indiceActual = 0;
        respuestasCorrectas = 0;
        esperandoSiguiente = false;

        botonSiguiente.gameObject.SetActive(false);
        botonConfirmar.gameObject.SetActive(true);

        MostrarPregunta(preguntas[indiceActual]);
    }

    bool ValidarReferencias()
    {
        if (preguntas == null || preguntas.Count == 0)
        { Debug.LogError("No hay preguntas cargadas"); return false; }
        if (textoFeedback == null)
        { Debug.LogError("Falta textoFeedback"); return false; }
        return true;
    }

    //  Mostrar pregunta
    void MostrarPregunta(PreguntaData pregunta)
    {
        textoFeedback.text = "";
        textoFeedback.color = colorNeutro;
        opcionSeleccionada = -1;
        esperandoSiguiente = false;

        botonConfirmar.gameObject.SetActive(true);
        botonSiguiente.gameObject.SetActive(false);

        textoProgreso.text = $"{indiceActual + 1} / {preguntas.Count}";
        textoInstruccion.text = pregunta.instruccion;

        if (pregunta.tipo == TipoPregunta.FillBlank)
            MostrarFillBlank(pregunta);
        else
            MostrarWordOrder(pregunta);
    }

    // FASE 1: Fill in the Blank 
    void MostrarFillBlank(PreguntaData pregunta)
    {
        panelFillBlank.SetActive(true);
        panelWordOrder.SetActive(false);

        textoOracion.text = pregunta.oracionConBlanco;

        foreach (Transform t in contenedorOpciones) Destroy(t.gameObject);

        // Mezclar opciones manteniendo track del índice correcto
        List<int> indices = new List<int>();
        for (int i = 0; i < pregunta.opciones.Length; i++)
            indices.Add(i);
        Shuffle(indices);

        foreach (int i in indices)
        {
            int indiceLocal = i;
            GameObject obj = Instantiate(botonOpcionPrefab, contenedorOpciones);

            TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = pregunta.opciones[i];
                tmp.color = Color.black;
            }

            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(() => SeleccionarOpcion(indiceLocal, obj));
        }
    }

    void SeleccionarOpcion(int indice, GameObject botonObj)
    {
        if (esperandoSiguiente) return;

        opcionSeleccionada = indice;

        // Resetear color de todos los botones
        foreach (Transform t in contenedorOpciones)
            t.GetComponent<Image>().color = colorNeutro;

        // Resaltar el seleccionado
        botonObj.GetComponent<Image>().color = new Color(0.7f, 0.9f, 1f);
    }

    // Word Order 
    void MostrarWordOrder(PreguntaData pregunta)
    {
        panelFillBlank.SetActive(false);
        panelWordOrder.SetActive(true);

        textoEspanol.text = pregunta.oracionEspanol;

        // Limpiar anterior
        foreach (Transform t in contenedorBloques) Destroy(t.gameObject);
        foreach (Transform t in contenedorSlots) Destroy(t.gameObject);

        // Mezclar palabras
        List<string> mezcladas = new List<string>(pregunta.palabrasDesordenadas);
        Shuffle(mezcladas);

        // Crear bloques
        foreach (string palabra in mezcladas)
        {
            GameObject obj = Instantiate(bloquePrefab, contenedorBloques);
            BloquePalabra bloque = obj.GetComponent<BloquePalabra>();
            bloque.Configurar(palabra, contenedorBloques);
        }

        // Crear slots vacíos
        for (int i = 0; i < pregunta.ordenCorrecto.Length; i++)
        {
            Instantiate(slotPrefab, contenedorSlots);
        }
    }

    //  Confirmar respuesta
    public void ConfirmarRespuesta()
    {
        if (esperandoSiguiente) return;

        PreguntaData pregunta = preguntas[indiceActual];
        bool correcto = false;

        if (pregunta.tipo == TipoPregunta.FillBlank)
            correcto = VerificarFillBlank(pregunta);
        else
            correcto = VerificarWordOrder(pregunta);

        esperandoSiguiente = true;
        botonConfirmar.gameObject.SetActive(false);
        botonSiguiente.gameObject.SetActive(true);

        // Cambiar texto según si es la última pregunta
        bool esUltima = (indiceActual >= preguntas.Count - 1);
        if (textoBotonSiguiente != null)
            textoBotonSiguiente.text = esUltima ? "Finalizar" : "Siguiente";

        if (correcto)
        {
            respuestasCorrectas++;
            textoFeedback.color = colorCorrecto;
            textoFeedback.text = "¡Correcto!";

            if (pregunta.palabrasQueEnsena != null && managerJournal != null)
                managerJournal.RegistrarPalabras(pregunta.palabrasQueEnsena);
        }
        else
        {
            textoFeedback.color = colorIncorrecto;

            if (pregunta.tipo == TipoPregunta.FillBlank)
                textoFeedback.text = $"Incorrecto. Era: {pregunta.opciones[pregunta.indiceRespuestaCorrecta]}";
            else
                textoFeedback.text = $"Incorrecto. Era: {string.Join(" ", pregunta.ordenCorrecto)}";
        }
    }

    bool VerificarFillBlank(PreguntaData pregunta)
    {
        if (opcionSeleccionada == -1)
        {
            textoFeedback.text = "Seleccioná una opción primero";
            esperandoSiguiente = false;
            botonConfirmar.gameObject.SetActive(true);
            botonSiguiente.gameObject.SetActive(false);
            return false;
        }
        return opcionSeleccionada == pregunta.indiceRespuestaCorrecta;
    }

    bool VerificarWordOrder(PreguntaData pregunta)
    {
        SlotPalabra[] slots = contenedorSlots.GetComponentsInChildren<SlotPalabra>();

        if (slots.Length != pregunta.ordenCorrecto.Length) return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].EstaVacio())
            {
                textoFeedback.text = "Completá todos los espacios primero";
                esperandoSiguiente = false;
                botonConfirmar.gameObject.SetActive(true);
                botonSiguiente.gameObject.SetActive(false);
                return false;
            }
            if (slots[i].OcupadoPor.valor != pregunta.ordenCorrecto[i])
                return false;
        }
        return true;
    }

    //  Siguiente pregunta
    public void SiguientePregunta()
    {
        indiceActual++;

        if (indiceActual >= preguntas.Count)
        {
            TerminarMinijuego();
            return;
        }

        MostrarPregunta(preguntas[indiceActual]);
    }

    void TerminarMinijuego()
    {
        bool exito = respuestasCorrectas >= Mathf.CeilToInt(preguntas.Count * 0.6f);

        // Cerrar el panel antes de continuar el diálogo
        if (minijuego2Panel != null)
            minijuego2Panel.SetActive(false);

        interaccionManager.OnMinigameFinished(exito);
    }

    void Shuffle<T>(List<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }
}