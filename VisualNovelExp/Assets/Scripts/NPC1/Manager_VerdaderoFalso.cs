using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_VerdaderoFalso : MonoBehaviour
{
    [Header("Ref")]
    public Manager_Interaccion interaccionManager;
    public KanaInventario kanaInventario; // opcional para bloqueo 🔒

    [Header("UI")]
    public TextMeshProUGUI textoEnunciadoJapones;
    public TextMeshProUGUI textoEnunciadoEspanol;
    public TextMeshProUGUI textoProgreso;
    public TextMeshProUGUI textoFeedback;
    public Button botonVerdadero;
    public Button botonFalso;
    public GameObject panelResultado; // opcional

    [Header("Datos")]
    public List<PreguntaVoF> preguntas; // se puede inyectar también
    [Tooltip("Si fallás 1 sola, perdés – es V/F de una oportunidad")]
    public bool falloInstantaneo = true;

    int indice = 0;
    int correctas = 0;

    [System.Serializable]
    public class PreguntaVoF
    {
        [Header("Contenido")]
        public string enunciadoJapones; // ej: "ねこ は いぬ です"
        public string enunciadoEspanol; // "(El gato es un perro)"
        public bool esVerdadero; // false en el ejemplo
        [TextArea] public string explicacion; // "ねこ = gato, いぬ = perro → Falso"

        [Header("Progresión")]
        public string idFilaKana = "fila_a"; // si no tenés esta fila, se bloquea

        [Header("Journal (opcional)")]
        public List<PalabraAprendida> palabrasQueEnsena;
    }

    public void SetPreguntas(List<PreguntaVoF> lista)
    {
        if (lista != null && lista.Count > 0) preguntas = lista;
    }

    public void Iniciar()
    {
        if (preguntas == null || preguntas.Count == 0)
        {
            Debug.LogError("[VoF] No hay preguntas");
            interaccionManager.OnMinigameFinished(false);
            return;
        }
        indice = 0;
        correctas = 0;
        if (panelResultado) panelResultado.SetActive(false);
        MostrarPregunta();
    }

    void MostrarPregunta()
    {
        if (indice >= preguntas.Count) { Terminar(); return; }
        var p = preguntas[indice];

        bool bloqueada = kanaInventario != null && !string.IsNullOrEmpty(p.idFilaKana) && !kanaInventario.EstaDesbloqueado(p.idFilaKana);

        if (textoProgreso) textoProgreso.text = $"{indice + 1} / {preguntas.Count}";
        if (textoEnunciadoJapones) textoEnunciadoJapones.text = bloqueada ? "🔒🔒🔒" : p.enunciadoJapones;
        if (textoEnunciadoEspanol) textoEnunciadoEspanol.text = bloqueada ? "(kana bloqueado – compralo en la tienda)" : p.enunciadoEspanol;
        if (textoFeedback) textoFeedback.text = bloqueada ? "¡Necesitás la fila " + p.idFilaKana + "!" : "";

        botonVerdadero.interactable = true;
        botonFalso.interactable = true;
        botonVerdadero.onClick.RemoveAllListeners();
        botonFalso.onClick.RemoveAllListeners();
        // si está bloqueada, cualquier respuesta cuenta como fallo
        botonVerdadero.onClick.AddListener(() => Responder(true, bloqueada));
        botonFalso.onClick.AddListener(() => Responder(false, bloqueada));
    }

    void Responder(bool elijoVerdadero, bool bloqueada)
    {
        botonVerdadero.interactable = false;
        botonFalso.interactable = false;

        var p = preguntas[indice];
        bool acierto = !bloqueada && (elijoVerdadero == p.esVerdadero);

        if (textoFeedback)
        {
            textoFeedback.color = acierto ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
            textoFeedback.text = acierto ? "¡Correcto!" : $"Incorrecto. {p.explicacion}";
        }

        if (acierto)
        {
            correctas++;
            // journal
            if (p.palabrasQueEnsena != null && p.palabrasQueEnsena.Count > 0)
            {
                var journal = FindObjectOfType<Manager_Journal>();
                if (journal) journal.RegistrarPalabras(p.palabrasQueEnsena);
            }
            Invoke(nameof(Siguiente), 1.4f);
        }
        else
        {
            if (falloInstantaneo)
            {
                Invoke(nameof(Fallar), 1.4f);
            }
            else
            {
                Invoke(nameof(Siguiente), 1.4f);
            }
        }
    }

    void Siguiente()
    {
        indice++;
        if (indice >= preguntas.Count) Terminar();
        else MostrarPregunta();
    }

    void Terminar()
    {
        // éxito si acertaste al menos 60%, o todas si falloInstantaneo=false lo decide el contador
        bool exito = correctas >= Mathf.CeilToInt(preguntas.Count * 0.6f);
        // si es modo 1 sola pregunta con fallo instantáneo:
        if (falloInstantaneo && preguntas.Count == 1) exito = correctas >= 1;

        interaccionManager.OnMinigameFinished(exito);
    }

    void Fallar()
    {
        interaccionManager.OnMinigameFinished(false);
    }
}
