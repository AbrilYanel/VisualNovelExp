using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Minijuego2 : MonoBehaviour
{
    [Header("Ref principal")]
    public Manager_Interaccion interaccionManager;
    public Manager_Journal managerJournal;

    [Header("Data - inyectable Paso 4")]
    public List<PreguntaData> preguntas;

    [Header("Progresión Kanas")]
    public KanaInventario kanaInventario;

    [Header("UI Común (siempre visible en Minijuego2)")]
    public GameObject minijuego2Panel; // panel raíz completo
    public TextMeshProUGUI textoInstruccion;
    public TextMeshProUGUI textoFeedback;
    public TextMeshProUGUI textoProgreso;
    public Button botonConfirmar;
    public Button botonSiguiente;
    public TextMeshProUGUI textoBotonSiguiente;

   
    [Header("UI FillBlank – PANEL INDEPENDIENTE")]
    public GameObject panelFillBlankRoot; 
    public TextMeshProUGUI textoOracion;
    public Transform contenedorOpciones;
    public GameObject botonOpcionPrefab;

    
    [Header("UI WordOrder – PANEL INDEPENDIENTE")]
    public GameObject panelWordOrderRoot; 
    public TextMeshProUGUI textoEspanol;
    public Transform contenedorBloques;   // layout horizontal abajo
    public Transform contenedorSlots;     // layout horizontal arriba
    public GameObject bloquePrefab;       // debe tener BloquePalabra + Image + TMP
    public GameObject slotPrefab;         // debe tener SlotPalabra + Image

    [Header("Colores")]
    public Color colorCorrecto = new Color(0.2f, 0.8f, 0.2f);
    public Color colorIncorrecto = new Color(0.9f, 0.2f, 0.2f);
    public Color colorNeutro = Color.white;

    // estado
    int indiceActual = 0;
    int respuestasCorrectas = 0;
    int opcionSeleccionada = -1;
    bool esperandoSiguiente = false;

    
    public void SetPreguntas(List<PreguntaData> nuevas)
    {
        if (nuevas != null && nuevas.Count > 0)
        {
            preguntas = nuevas;
            Debug.Log($"[Minijuego2] Preguntas inyectadas: {preguntas.Count}", this);
        }
    }

    public void Iniciar()
    {
        Debug.Log($"[Minijuego2] Iniciar() – preguntas: {(preguntas != null ? preguntas.Count : 0)}", this);
        if (!ValidarReferenciasBasicas()) return;

        if (preguntas == null || preguntas.Count == 0)
        {
            Debug.LogError("[Minijuego2] No hay preguntas. ¿Olvidaste SetPreguntas() en Manager_Interaccion?", this);
            interaccionManager?.OnMinigameFinished(false);
            return;
        }

        // asegurar panel raíz encendido
        if (minijuego2Panel != null) minijuego2Panel.SetActive(true);

        indiceActual = 0;
        respuestasCorrectas = 0;
        esperandoSiguiente = false;

        if (botonSiguiente) botonSiguiente.gameObject.SetActive(false);
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(true);

        MostrarPregunta(preguntas[indiceActual]);
    }

    bool ValidarReferenciasBasicas()
    {
        bool ok = true;
        if (minijuego2Panel == null) { Debug.LogError("[Minijuego2] minijuego2Panel NULL", this); ok = false; }
        if (panelFillBlankRoot == null) Debug.LogWarning("[Minijuego2] panelFillBlankRoot NULL – FillBlank no funcionará", this);
        if (panelWordOrderRoot == null) Debug.LogError("[Minijuego2] panelWordOrderRoot NULL – ¡WordOrder NO se verá! Asignalo en el Inspector.", this);
        if (textoInstruccion == null) Debug.LogWarning("[Minijuego2] textoInstruccion NULL");
        if (botonConfirmar == null) Debug.LogError("[Minijuego2] botonConfirmar NULL");
        return ok;
    }

    bool ValidarWordOrderRefs()
    {
        bool ok = true;
        if (panelWordOrderRoot == null) { Debug.LogError("[WordOrder] panelWordOrderRoot es NULL – asignalo en Inspector", this); ok = false; }
        if (textoEspanol == null) { Debug.LogError("[WordOrder] textoEspanol NULL", this); ok = false; }
        if (contenedorBloques == null) { Debug.LogError("[WordOrder] contenedorBloques NULL", this); ok = false; }
        if (contenedorSlots == null) { Debug.LogError("[WordOrder] contenedorSlots NULL", this); ok = false; }
        if (bloquePrefab == null) { Debug.LogError("[WordOrder] bloquePrefab NULL", this); ok = false; }
        if (slotPrefab == null) { Debug.LogError("[WordOrder] slotPrefab NULL", this); ok = false; }
        return ok;
    }

   
    void MostrarPregunta(PreguntaData pregunta)
    {
        Debug.Log($"[Minijuego2] MostrarPregunta {indiceActual + 1}/{preguntas.Count} tipo={pregunta.tipo}", this);

        // reset UI común
        if (textoFeedback) { textoFeedback.text = ""; textoFeedback.color = colorNeutro; }
        opcionSeleccionada = -1;
        esperandoSiguiente = false;
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(true);
        if (botonSiguiente) botonSiguiente.gameObject.SetActive(false);
        if (textoProgreso) textoProgreso.text = $"{indiceActual + 1} / {preguntas.Count}";
        if (textoInstruccion) textoInstruccion.text = pregunta.instruccion;

        // apagar AMBOS paneles primero (evita solapamiento)
        if (panelFillBlankRoot) panelFillBlankRoot.SetActive(false);
        if (panelWordOrderRoot) panelWordOrderRoot.SetActive(false);

        bool bloqueada = false;
        if (kanaInventario != null && !string.IsNullOrEmpty(pregunta.idFilaKana))
            bloqueada = !kanaInventario.EstaDesbloqueado(pregunta.idFilaKana);

        if (pregunta.tipo == TipoPregunta.FillBlank)
        {
            MostrarFillBlank(pregunta, bloqueada);
        }
        else // WordOrder
        {
            MostrarWordOrder(pregunta, bloqueada);
        }
    }

    void MostrarFillBlank(PreguntaData pregunta, bool bloqueada)
    {
        if (panelFillBlankRoot == null)
        {
            Debug.LogError("[FillBlank] panelFillBlankRoot NULL – no puedo mostrar UI", this);
            return;
        }
        // ENCENDER solo FillBlank
        panelFillBlankRoot.SetActive(true);
        if (panelWordOrderRoot) panelWordOrderRoot.SetActive(false);

      
       // panelFillBlankRoot.transform.SetAsLastSibling();

        if (textoOracion) textoOracion.text = bloqueada ? "🔒 🔒 🔒" : pregunta.oracionConBlanco;

        // limpiar opciones
        if (contenedorOpciones != null)
            foreach (Transform t in contenedorOpciones) Destroy(t.gameObject);
        else
            Debug.LogError("[FillBlank] contenedorOpciones NULL");

        // crear botones
        if (pregunta.opciones != null)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < pregunta.opciones.Length; i++) indices.Add(i);
            Shuffle(indices);

            foreach (int i in indices)
            {
                if (botonOpcionPrefab == null) { Debug.LogError("[FillBlank] botonOpcionPrefab NULL"); break; }
                int idxLocal = i;
                GameObject obj = Instantiate(botonOpcionPrefab, contenedorOpciones);
                var tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp) { tmp.text = bloqueada ? "🔒" : pregunta.opciones[i]; tmp.color = Color.black; }
                var btn = obj.GetComponent<Button>();
                if (btn)
                {
                    btn.interactable = !bloqueada;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SeleccionarOpcion(idxLocal, obj));
                }
            }
        }

        Debug.Log("[FillBlank] UI activada OK", panelFillBlankRoot);
    }

    void SeleccionarOpcion(int indice, GameObject botonObj)
    {
        if (esperandoSiguiente) return;
        opcionSeleccionada = indice;
        if (contenedorOpciones != null)
        {
            foreach (Transform t in contenedorOpciones)
            {
                var img = t.GetComponent<Image>();
                if (img) img.color = colorNeutro;
            }
        }
        var imgSel = botonObj.GetComponent<Image>();
        if (imgSel) imgSel.color = new Color(0.7f, 0.9f, 1f);
    }

    void MostrarWordOrder(PreguntaData pregunta, bool bloqueada)
    {
        Debug.Log("[WordOrder] Entrando a MostrarWordOrder()", this);
        if (!ValidarWordOrderRefs())
        {
            Debug.LogError("[WordOrder] Faltan referencias – abortando, marco como fallo", this);
            if (textoFeedback) { textoFeedback.color = colorIncorrecto; textoFeedback.text = "Error de configuración WordOrder – revisá Inspector"; }
            // fallar en 1.5s para no colgar el juego
            Invoke(nameof(FallarPorConfig), 1.5f);
            return;
        }

        // ENCENDER solo WordOrder
        panelWordOrderRoot.SetActive(true);
        if (panelFillBlankRoot) panelFillBlankRoot.SetActive(false);

        // traer al frente
        //panelWordOrderRoot.transform.SetAsLastSibling();

        // asegurar que TODOS los padres estén activos
        Transform p = panelWordOrderRoot.transform;
        while (p != null)
        {
            if (!p.gameObject.activeSelf)
            {
                Debug.LogWarning($"[WordOrder] Activando padre inactivo: {p.name}", p.gameObject);
                p.gameObject.SetActive(true);
            }
            p = p.parent;
        }

        if (textoEspanol) textoEspanol.text = bloqueada ? "🔒 (kana bloqueado)" : pregunta.oracionEspanol;

        // limpiar
        foreach (Transform t in contenedorBloques) Destroy(t.gameObject);
        foreach (Transform t in contenedorSlots) Destroy(t.gameObject);

        // crear bloques mezclados
        List<string> mezcladas = new List<string>(pregunta.palabrasDesordenadas);
        Shuffle(mezcladas);

        Debug.Log($"[WordOrder] Creando {mezcladas.Count} bloques, {pregunta.ordenCorrecto.Length} slots", this);

        foreach (string palabra in mezcladas)
        {
            if (bloquePrefab == null) break;
            GameObject obj = Instantiate(bloquePrefab, contenedorBloques);
            obj.SetActive(true);
            var bloque = obj.GetComponent<BloquePalabra>();
            if (bloque == null) Debug.LogError("bloquePrefab necesita BloquePalabra", obj);
            else
            {
                string mostrar = bloqueada ? "🔒" : palabra;
                bloque.Configurar(mostrar, contenedorBloques);
                // guardar valor real aunque mostremos 🔒
                bloque.valor = palabra;
            }
            // asegurar escala / layout
            var rt = obj.GetComponent<RectTransform>();
            if (rt && rt.localScale == Vector3.zero) rt.localScale = Vector3.one;
        }

        // crear slots
        int nSlots = pregunta.ordenCorrecto != null ? pregunta.ordenCorrecto.Length : 0;
        for (int i = 0; i < nSlots; i++)
        {
            if (slotPrefab == null) break;
            GameObject s = Instantiate(slotPrefab, contenedorSlots);
            s.SetActive(true);
        }

        // forzar rebuild layout para que se vean inmediatamente
        Canvas.ForceUpdateCanvases();
        if (contenedorBloques is RectTransform rtb)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rtb);
        if (contenedorSlots is RectTransform rts)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rts);

        if (bloqueada && textoFeedback)
        {
            textoFeedback.color = colorIncorrecto;
            textoFeedback.text = "Kana bloqueado – compralo en la tienda";
        }

        Debug.Log("[WordOrder] UI activada OK – bloques: " + contenedorBloques.childCount + " slots: " + contenedorSlots.childCount, panelWordOrderRoot);
    }

    void FallarPorConfig()
    {
        interaccionManager?.OnMinigameFinished(false);
    }

   
    public void ConfirmarRespuesta()
    {
        if (esperandoSiguiente) return;
        var pregunta = preguntas[indiceActual];
        bool correcto = pregunta.tipo == TipoPregunta.FillBlank ? VerificarFillBlank(pregunta) : VerificarWordOrder(pregunta);
        
        if (!esperandoSiguiente && !correcto) return; // salió temprano por input incompleto

        esperandoSiguiente = true;
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(false);
        if (botonSiguiente) botonSiguiente.gameObject.SetActive(true);
        bool esUltima = indiceActual >= preguntas.Count - 1;
        if (textoBotonSiguiente) textoBotonSiguiente.text = esUltima ? "Finalizar" : "Siguiente";

        if (correcto)
        {
            respuestasCorrectas++;
            if (textoFeedback) { textoFeedback.color = colorCorrecto; textoFeedback.text = "¡Correcto!"; }
            if (pregunta.palabrasQueEnsena != null && managerJournal != null)
                managerJournal.RegistrarPalabras(pregunta.palabrasQueEnsena);
        }
        else
        {
            if (textoFeedback)
            {
                textoFeedback.color = colorIncorrecto;
                if (pregunta.tipo == TipoPregunta.FillBlank)
                    textoFeedback.text = $"Incorrecto. Era: {pregunta.opciones[pregunta.indiceRespuestaCorrecta]}";
                else
                    textoFeedback.text = $"Incorrecto. Era: {string.Join(" ", pregunta.ordenCorrecto)}";
            }
        }
    }

    bool VerificarFillBlank(PreguntaData pregunta)
    {
        if (opcionSeleccionada == -1)
        {
            if (textoFeedback) { textoFeedback.text = "Seleccioná una opción primero"; textoFeedback.color = colorIncorrecto; }
            return false;
        }
        return opcionSeleccionada == pregunta.indiceRespuestaCorrecta;
    }

    bool VerificarWordOrder(PreguntaData pregunta)
    {
        if (contenedorSlots == null) return false;
        SlotPalabra[] slots = contenedorSlots.GetComponentsInChildren<SlotPalabra>(true);
        if (slots.Length != pregunta.ordenCorrecto.Length)
        {
            Debug.LogWarning($"[WordOrder] slots encontrados {slots.Length} != esperados {pregunta.ordenCorrecto.Length}");
            // igual intentamos
        }
        for (int i = 0; i < pregunta.ordenCorrecto.Length; i++)
        {
            if (i >= slots.Length || slots[i] == null || slots[i].EstaVacio())
            {
                if (textoFeedback) { textoFeedback.text = "Completá todos los espacios primero"; textoFeedback.color = colorIncorrecto; }
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
        // apagar subpaneles también
        if (panelFillBlankRoot) panelFillBlankRoot.SetActive(false);
        if (panelWordOrderRoot) panelWordOrderRoot.SetActive(false);
        interaccionManager?.OnMinigameFinished(exito);
    }

    void Shuffle<T>(List<T> lista) { for (int i = lista.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (lista[i], lista[j]) = (lista[j], lista[i]); } }

   
#if UNITY_EDITOR
    [ContextMenu("Test Mostrar WordOrder")]
    void TestWordOrder()
    {
        if (preguntas == null || preguntas.Count == 0)
        {
            // crear pregunta dummy
            preguntas = new List<PreguntaData>{ new PreguntaData{
                tipo = TipoPregunta.WordOrder,
                instruccion = "Ordená la oración",
                oracionEspanol = "El gato come pescado",
                palabrasDesordenadas = new string[]{ "taberu","neko","sakana","wa","wo" },
                ordenCorrecto = new string[]{ "neko","wa","sakana","wo","taberu" },
                idFilaKana = ""
            }};
        }
        if (minijuego2Panel) minijuego2Panel.SetActive(true);
        indiceActual = 0;
        respuestasCorrectas = 0;
        var p = preguntas[0];
        p.tipo = TipoPregunta.WordOrder; // forzar
        MostrarPregunta(p);
    }

    [ContextMenu("Test Mostrar FillBlank")]
    void TestFillBlank()
    {
        if (minijuego2Panel) minijuego2Panel.SetActive(true);
        var p = new PreguntaData
        {
            tipo = TipoPregunta.FillBlank,
            instruccion = "Test FillBlank",
            oracionConBlanco = "Watashi ___ gakusei desu",
            opciones = new string[] { "wa", "wo", "ni", "ga" },
            indiceRespuestaCorrecta = 0,
            idFilaKana = ""
        };
        preguntas = new List<PreguntaData> { p };
        indiceActual = 0;
        MostrarPregunta(p);
    }
#endif
}
