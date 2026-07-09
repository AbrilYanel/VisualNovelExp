using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_KanaRush : MonoBehaviour
{
    [Header("Refs")]
    public Manager_Interaccion interaccionManager;
    public KanaInventario kanaInventario;

    [Header("UI")]
    public TextMeshProUGUI textoKanaGrande;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoPuntaje;
    public TextMeshProUGUI textoFeedback;
    public Transform contenedorOpciones;
    public GameObject botonOpcionPrefab;
    public Slider barraTiempo;

    [Header("Datos")]
    public List<KanaPregunta> bancoPreguntas; // si está vacío, se genera desde KanaData desbloqueadas
    public List<KanaData> filasPermitidas; // opcional: filtrar

    [Header("Reglas NPC5 - fila na")]
    public float tiempoPorPregunta = 4.5f;
    public int rondasTotales = 12;
    public int aciertosNecesarios = 8; // 66%

    int rondaActual = 0;
    int aciertos = 0;
    float tiempoRestante;
    bool rondaActiva = false;
    KanaPregunta preguntaActual;

    [System.Serializable]
    public class KanaPregunta
    {
        public string kana; // な
        public string romajiCorrecto; // na
        public string idFilaKana = "fila_na";
        public string[] distractores; // ni, nu, ne
    }

    public void Iniciar()
    {
        // si no hay banco manual, autogenerar desde filas desbloqueadas
        if ((bancoPreguntas == null || bancoPreguntas.Count == 0) && kanaInventario != null && filasPermitidas != null)
        {
            bancoPreguntas = new List<KanaPregunta>();
            foreach (var fila in filasPermitidas)
            {
                if (fila == null) continue;
                if (!kanaInventario.EstaDesbloqueado(fila.id)) continue;
                foreach (var c in fila.caracteres)
                {
                    bancoPreguntas.Add(new KanaPregunta
                    {
                        kana = c.kana,
                        romajiCorrecto = c.romaji,
                        idFilaKana = fila.id,
                        distractores = GenerarDistractores(fila, c.romaji)
                    });
                }
            }
        }

        if (bancoPreguntas == null || bancoPreguntas.Count == 0)
        {
            Debug.LogError("[KanaRush] Sin preguntas");
            interaccionManager.OnMinigameFinished(false);
            return;
        }

        rondaActual = 0;
        aciertos = 0;
        SiguienteRonda();
    }

    void SiguienteRonda()
    {
        if (rondaActual >= rondasTotales)
        {
            Terminar();
            return;
        }

        // elegir pregunta que el jugador tenga desbloqueada
        List<KanaPregunta> disponibles = bancoPreguntas.FindAll(p =>
            string.IsNullOrEmpty(p.idFilaKana) ||
            kanaInventario == null ||
            kanaInventario.EstaDesbloqueado(p.idFilaKana)
        );
        if (disponibles.Count == 0) disponibles = bancoPreguntas;

        preguntaActual = disponibles[Random.Range(0, disponibles.Count)];
        rondaActual++;

        // UI
        if (textoKanaGrande) textoKanaGrande.text = preguntaActual.kana;
        if (textoPuntaje) textoPuntaje.text = $"Aciertos: {aciertos}/{aciertosNecesarios}  Ronda {rondaActual}/{rondasTotales}";
        if (textoFeedback) textoFeedback.text = "";

        // opciones
        foreach (Transform t in contenedorOpciones) Destroy(t.gameObject);
        List<string> opciones = new List<string> { preguntaActual.romajiCorrecto };
        if (preguntaActual.distractores != null)
            foreach (var d in preguntaActual.distractores) if (!string.IsNullOrEmpty(d) && !opciones.Contains(d)) opciones.Add(d);
        // rellenar hasta 4
        string[] fallback = { "a", "ka", "sa", "ta", "na", "ha", "ma", "ya", "ra", "n" };
        while (opciones.Count < 4)
        {
            var r = fallback[Random.Range(0, fallback.Length)];
            if (!opciones.Contains(r)) opciones.Add(r);
        }
        Shuffle(opciones);
        opciones = opciones.GetRange(0, 4);

        foreach (var op in opciones)
        {
            string opLocal = op;
            GameObject btnObj = Instantiate(botonOpcionPrefab, contenedorOpciones);
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp) tmp.text = opLocal;
            var btn = btnObj.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => Responder(opLocal));
        }

        tiempoRestante = tiempoPorPregunta;
        rondaActiva = true;
        StopAllCoroutines();
        StartCoroutine(TickTiempo());
    }

    IEnumerator TickTiempo()
    {
        while (rondaActiva && tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (textoTiempo) textoTiempo.text = $"{tiempoRestante:F1}s";
            if (barraTiempo) barraTiempo.value = tiempoRestante / tiempoPorPregunta;
            yield return null;
        }
        if (rondaActiva)
        {
            // tiempo agotado = fallo
            Responder(null);
        }
    }

    void Responder(string elegido)
    {
        if (!rondaActiva) return;
        rondaActiva = false;
        StopAllCoroutines();

        bool correcto = elegido != null && elegido == preguntaActual.romajiCorrecto;
        if (correcto) aciertos++;

        if (textoFeedback)
        {
            textoFeedback.color = correcto ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
            textoFeedback.text = correcto ? "¡Bien!" : $"Era {preguntaActual.romajiCorrecto}";
        }

        Invoke(nameof(SiguienteRonda), 0.75f);
    }

    void Terminar()
    {
        bool exito = aciertos >= aciertosNecesarios;
        interaccionManager.OnMinigameFinished(exito);
    }

    string[] GenerarDistractores(KanaData fila, string correcto)
    {
        List<string> pool = new List<string>();
        foreach (var c in fila.caracteres) if (c.romaji != correcto) pool.Add(c.romaji);
        Shuffle(pool);
        return pool.GetRange(0, Mathf.Min(3, pool.Count)).ToArray();
    }

    void Shuffle<T>(List<T> l) { for (int i = l.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (l[i], l[j]) = (l[j], l[i]); } }
}
