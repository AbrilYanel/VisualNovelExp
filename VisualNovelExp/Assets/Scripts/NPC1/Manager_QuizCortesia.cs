using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_QuizCortesia : MonoBehaviour
{
    [Header("Refs")]
    public Manager_Interaccion interaccionManager;
    public KanaInventario kanaInventario;

    [Header("UI")]
    public TextMeshProUGUI textoSituacion;
    public TextMeshProUGUI textoPregunta;
    public Transform contenedorOpciones;
    public GameObject botonOpcionPrefab;
    public TextMeshProUGUI textoProgreso;
    public TextMeshProUGUI textoFeedback;

    [Header("Datos")]
    public List<PreguntaCortesia> preguntas;

    int indice = 0;
    int correctas = 0;
    bool esperando = false;

    [System.Serializable]
    public class PreguntaCortesia
    {
        [TextArea] public string situacion; // "Entrás a una tienda en Japón..."
        public string pregunta; // "¿Qué decís al entrar?"
        public string[] opciones; // {"Irasshaimase","Konnichiwa","Sayonara","Arigato"}
        public int indiceCorrecta;
        [TextArea] public string explicacion;
        public string idFilaKana = "fila_ya"; // opcional para bloqueo
    }

    public void SetPreguntas(List<PreguntaCortesia> lista)
    {
        if (lista != null && lista.Count > 0) preguntas = lista;
    }

    public void Iniciar()
    {
        if (preguntas == null || preguntas.Count == 0)
        {
            Debug.LogError("[QuizCortesia] sin preguntas");
            interaccionManager.OnMinigameFinished(false);
            return;
        }
        indice = 0;
        correctas = 0;
        Mostrar();
    }

    void Mostrar()
    {
        esperando = false;
        var p = preguntas[indice];
        bool bloqueada = kanaInventario != null && !string.IsNullOrEmpty(p.idFilaKana) && !kanaInventario.EstaDesbloqueado(p.idFilaKana);

        if (textoSituacion) textoSituacion.text = bloqueada ? "🔒 Contenido bloqueado" : p.situacion;
        if (textoPregunta) textoPregunta.text = bloqueada ? "Comprá la fila " + p.idFilaKana + " en la tienda" : p.pregunta;
        if (textoProgreso) textoProgreso.text = $"{indice + 1}/{preguntas.Count}";
        if (textoFeedback) textoFeedback.text = "";

        foreach (Transform t in contenedorOpciones) Destroy(t.gameObject);

        for (int i = 0; i < p.opciones.Length; i++)
        {
            int idx = i;
            var obj = Instantiate(botonOpcionPrefab, contenedorOpciones);
            var tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp) tmp.text = bloqueada ? "🔒" : p.opciones[i];
            var btn = obj.GetComponent<Button>();
            if (btn)
            {
                btn.interactable = !bloqueada;
                btn.onClick.AddListener(() => Responder(idx, p));
            }
        }
    }

    void Responder(int elegido, PreguntaCortesia p)
    {
        if (esperando) return;
        esperando = true;

        bool ok = elegido == p.indiceCorrecta;
        if (ok) correctas++;

        // pintar botones
        int idx = 0;
        foreach (Transform t in contenedorOpciones)
        {
            var btn = t.GetComponent<Button>();
            var img = t.GetComponent<Image>();
            if (img)
            {
                if (idx == p.indiceCorrecta) img.color = new Color(0.4f, 0.9f, 0.4f);
                else if (idx == elegido) img.color = new Color(0.9f, 0.4f, 0.4f);
            }
            if (btn) btn.interactable = false;
            idx++;
        }

        if (textoFeedback)
        {
            textoFeedback.color = ok ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
            textoFeedback.text = ok ? "¡Correcto!" : p.explicacion;
        }

        Invoke(nameof(Avanzar), 1.8f);
    }

    void Avanzar()
    {
        indice++;
        if (indice >= preguntas.Count)
        {
            bool exito = correctas >= Mathf.CeilToInt(preguntas.Count * 0.6f);
            interaccionManager.OnMinigameFinished(exito);
        }
        else
        {
            Mostrar();
        }
    }
}
