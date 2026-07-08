// Manager_Minijuego2 - versión inyectable (Paso 4)
// Reemplaza tu Manager_Minijuego2.cs actual
// Cambios clave:
// - SetPreguntas(List<PreguntaData>) para inyectar desde Manager_Interaccion
// - Soporte bloqueo 🔒 con KanaInventario
// - Mantiene FillBlank y WordOrder

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Minijuego2 : MonoBehaviour
{
    [Header("Referencia principal")]
    public Manager_Interaccion interaccionManager;
    public Manager_Journal managerJournal;

    [Header("Preguntas - se inyectan por código (Paso 4)")]
    public List<PreguntaData> preguntas; // fallback inspector

    [Header("Progresión Kanas")]
    public KanaInventario kanaInventario;

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
    public Transform contenedorBloques;
    public Transform contenedorSlots;
    public GameObject bloquePrefab;
    public GameObject slotPrefab;

    [Header("Colores")]
    public Color colorCorrecto = new Color(0.2f, 0.8f, 0.2f);
    public Color colorIncorrecto = new Color(0.9f, 0.2f, 0.2f);
    public Color colorNeutro = Color.white;

    int indiceActual = 0;
    int respuestasCorrectas = 0;
    int opcionSeleccionada = -1;
    bool esperandoSiguiente = false;

    // --- INYECCIÓN PASO 4 ---
    public void SetPreguntas(List<PreguntaData> nuevasPreguntas)
    {
        if (nuevasPreguntas != null && nuevasPreguntas.Count > 0)
        {
            preguntas = nuevasPreguntas;
            Debug.Log($"[Minijuego2] Preguntas inyectadas: {preguntas.Count}");
        }
    }

    public void Iniciar()
    {
        if (preguntas == null || preguntas.Count == 0)
        { Debug.LogError("[Minijuego2] No hay preguntas. ¿Olvidaste SetPreguntas()?"); return; }

        indiceActual = 0;
        respuestasCorrectas = 0;
        esperandoSiguiente = false;
        if (botonSiguiente) botonSiguiente.gameObject.SetActive(false);
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(true);
        MostrarPregunta(preguntas[indiceActual]);
    }

    void MostrarPregunta(PreguntaData pregunta)
    {
        textoFeedback.text = "";
        textoFeedback.color = colorNeutro;
        opcionSeleccionada = -1;
        esperandoSiguiente = false;
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(true);
        if (botonSiguiente) botonSiguiente.gameObject.SetActive(false);
        if (textoProgreso) textoProgreso.text = $"{indiceActual + 1} / {preguntas.Count}";
        if (textoInstruccion) textoInstruccion.text = pregunta.instruccion;

        // bloqueo visual de la pregunta entera si su kana no está desbloqueado
        bool bloqueada = false;
        if (kanaInventario != null && !string.IsNullOrEmpty(pregunta.idFilaKana))
            bloqueada = !kanaInventario.EstaDesbloqueado(pregunta.idFilaKana);

        if (pregunta.tipo == TipoPregunta.FillBlank)
            MostrarFillBlank(pregunta, bloqueada);
        else
            MostrarWordOrder(pregunta, bloqueada);
    }

    void MostrarFillBlank(PreguntaData pregunta, bool bloqueada)
    {
        panelFillBlank.SetActive(true);
        panelWordOrder.SetActive(false);

        if (textoOracion)
            textoOracion.text = bloqueada ? "🔒 🔒 🔒" : pregunta.oracionConBlanco;

        foreach (Transform t in contenedorOpciones) Destroy(t.gameObject);

        List<int> indices = new List<int>();
        for (int i = 0; i < pregunta.opciones.Length; i++) indices.Add(i);
        Shuffle(indices);

        foreach (int i in indices)
        {
            int idxLocal = i;
            GameObject obj = Instantiate(botonOpcionPrefab, contenedorOpciones);
            var tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp) { tmp.text = bloqueada ? "🔒" : pregunta.opciones[i]; tmp.color = Color.black; }
            var btn = obj.GetComponent<Button>();
            if (btn)
            {
                btn.interactable = !bloqueada; // si está bloqueado, no deja elegir bien -> fuerza fallo
                btn.onClick.AddListener(() => SeleccionarOpcion(idxLocal, obj));
            }
        }

        if (bloqueada && textoFeedback)
        {
            textoFeedback.color = colorIncorrecto;
            textoFeedback.text = "¡Necesitás comprar el kana en la tienda!";
        }
    }

    void SeleccionarOpcion(int indice, GameObject botonObj)
    {
        if (esperandoSiguiente) return;
        opcionSeleccionada = indice;
        foreach (Transform t in contenedorOpciones)
            t.GetComponent<Image>().color = colorNeutro;
        botonObj.GetComponent<Image>().color = new Color(0.7f, 0.9f, 1f);
    }

    void MostrarWordOrder(PreguntaData pregunta, bool bloqueada)
    {
        panelFillBlank.SetActive(false);
        panelWordOrder.SetActive(true);
        if (textoEspanol) textoEspanol.text = pregunta.oracionEspanol;

        foreach (Transform t in contenedorBloques) Destroy(t.gameObject);
        foreach (Transform t in contenedorSlots) Destroy(t.gameObject);

        List<string> mezcladas = new List<string>(pregunta.palabrasDesordenadas);
        Shuffle(mezcladas);

        foreach (string palabra in mezcladas)
        {
            GameObject obj = Instantiate(bloquePrefab, contenedorBloques);
            var bloque = obj.GetComponent<BloquePalabra>();
            string mostrar = bloqueada ? "🔒" : palabra;
            if (bloque) bloque.Configurar(mostrar, contenedorBloques);
            // guardar valor real igual, para que falle si está bloqueado
            if (bloque) bloque.valor = palabra;
        }
        for (int i = 0; i < pregunta.ordenCorrecto.Length; i++)
            Instantiate(slotPrefab, contenedorSlots);

        if (bloqueada && textoFeedback)
        {
            textoFeedback.color = colorIncorrecto;
            textoFeedback.text = "Kana bloqueado – compralo en la tienda";
        }
    }

    public void ConfirmarRespuesta()
    {
        if (esperandoSiguiente) return;
        var pregunta = preguntas[indiceActual];
        bool correcto = pregunta.tipo == TipoPregunta.FillBlank ? VerificarFillBlank(pregunta) : VerificarWordOrder(pregunta);
        if (!esperandoSiguiente && !correcto && (pregunta.tipo == TipoPregunta.FillBlank && opcionSeleccionada == -1)) return; // ya mostró mensaje "seleccioná opción"

        esperandoSiguiente = true;
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(false);
        if (botonSiguiente) botonSiguiente.gameObject.SetActive(true);
        bool esUltima = indiceActual >= preguntas.Count - 1;
        if (textoBotonSiguiente) textoBotonSiguiente.text = esUltima ? "Finalizar" : "Siguiente";

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
            textoFeedback.color = colorIncorrecto;
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
                textoFeedback.text = "Completá todos los espacios";
                textoFeedback.color = colorIncorrecto;
                esperandoSiguiente = false;
                if (botonConfirmar) botonConfirmar.gameObject.SetActive(true);
                if (botonSiguiente) botonSiguiente.gameObject.SetActive(false);
                return false;
            }
            if (slots[i].OcupadoPor.valor != pregunta.ordenCorrecto[i])
                return false;
        }
        return true;
    }

    public void SiguientePregunta()
    {
        indiceActual++;
        if (indiceActual >= preguntas.Count) { TerminarMinijuego(); return; }
        MostrarPregunta(preguntas[indiceActual]);
    }

    void TerminarMinijuego()
    {
        bool exito = respuestasCorrectas >= Mathf.CeilToInt(preguntas.Count * 0.6f);
        if (minijuego2Panel) minijuego2Panel.SetActive(false);
        interaccionManager.OnMinigameFinished(exito);
    }

    void Shuffle<T>(List<T> lista) { for (int i = lista.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (lista[i], lista[j]) = (lista[j], lista[i]); } }
}
