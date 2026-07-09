using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_EscrituraInversa : MonoBehaviour
{
    [Header("Refs")]
    public Manager_Interaccion interaccionManager;
    public KanaInventario kanaInventario;

    [Header("UI")]
    public TextMeshProUGUI textoRomaji; // ej: "ha"
    public TMP_InputField inputKana; // el jugador escribe "は"
    public TextMeshProUGUI textoPista;
    public TextMeshProUGUI textoProgreso;
    public TextMeshProUGUI textoFeedback;
    public Button botonConfirmar;
    public Button botonPista;

    [Header("Datos")]
    public List<ItemEscritura> items; // se puede autogenerar desde filas

    public List<KanaData> filasPermitidas; // ej: fila_ha

    [Header("Reglas")]
    public int rondas = 8;
    public int aciertosNecesarios = 5;
    public bool aceptarRomajiComoFallback = true; // si no tienen IME japonés, acepta "ha"

    int indice = 0;
    int aciertos = 0;
    List<ItemEscritura> mazo;
    ItemEscritura actual;
    int pistasUsadas = 0;

    [System.Serializable]
    public class ItemEscritura
    {
        public string kanaCorrecto; // は
        public string romaji;       // ha
        public string idFilaKana = "fila_ha";
        public string pista = "ha – fila HA";
    }

    void Awake()
    {
        if (botonConfirmar) botonConfirmar.onClick.AddListener(Confirmar);
        if (botonPista) botonPista.onClick.AddListener(DarPista);
        if (inputKana) inputKana.onSubmit.AddListener((s) => Confirmar());
    }

    public void Iniciar()
    {
        // autogenerar si no hay items manuales
        if ((items == null || items.Count == 0) && filasPermitidas != null && kanaInventario != null)
        {
            items = new List<ItemEscritura>();
            foreach (var fila in filasPermitidas)
            {
                if (fila == null) continue;
                // no exigimos que esté desbloqueada para poder practicar, pero marcamos idFilaKana
                foreach (var c in fila.caracteres)
                {
                    items.Add(new ItemEscritura
                    {
                        kanaCorrecto = c.kana,
                        romaji = c.romaji,
                        idFilaKana = fila.id,
                        pista = $"{c.romaji} – {fila.nombreFila}"
                    });
                }
            }
        }

        if (items == null || items.Count == 0)
        {
            Debug.LogError("[EscrituraInversa] sin items");
            interaccionManager.OnMinigameFinished(false);
            return;
        }

        // filtrar solo desbloqueados si hay inventario
        mazo = new List<ItemEscritura>(items);
        if (kanaInventario != null)
            mazo.RemoveAll(it => !string.IsNullOrEmpty(it.idFilaKana) && !kanaInventario.EstaDesbloqueado(it.idFilaKana));

        if (mazo.Count == 0) mazo = new List<ItemEscritura>(items); // fallback para testear

        Shuffle(mazo);
        if (mazo.Count > rondas) mazo = mazo.GetRange(0, rondas);

        indice = 0;
        aciertos = 0;
        pistasUsadas = 0;
        Siguiente();
    }

    void Siguiente()
    {
        if (indice >= mazo.Count || indice >= rondas)
        {
            Terminar();
            return;
        }
        actual = mazo[indice];
        if (textoRomaji) textoRomaji.text = actual.romaji;
        if (textoProgreso) textoProgreso.text = $"{indice + 1} / {Mathf.Min(rondas, mazo.Count)}   ✓ {aciertos}";
        if (textoPista) textoPista.text = "";
        if (textoFeedback) textoFeedback.text = "";
        if (inputKana)
        {
            inputKana.text = "";
            inputKana.ActivateInputField();
            inputKana.Select();
        }
    }

    void DarPista()
    {
        pistasUsadas++;
        if (textoPista) textoPista.text = "💡 " + actual.pista + (actual.kanaCorrecto != null ? $" → {actual.kanaCorrecto}" : "");
    }

    public void Confirmar()
    {
        if (actual == null) return;
        string respuesta = inputKana ? inputKana.text.Trim().ToLower() : "";
        if (string.IsNullOrEmpty(respuesta))
        {
            if (textoFeedback) { textoFeedback.color = Color.yellow; textoFeedback.text = "Escribí el kana (o romaji)"; }
            return;
        }

        bool ok = respuesta == actual.kanaCorrecto;
        if (!ok && aceptarRomajiComoFallback)
            ok = respuesta == actual.romaji.ToLower();

        if (textoFeedback)
        {
            textoFeedback.color = ok ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
            textoFeedback.text = ok ? "¡Correcto!" : $"Era: {actual.kanaCorrecto} ({actual.romaji})";
        }

        if (ok) aciertos++;
        indice++;
        Invoke(nameof(Siguiente), 1.0f);
    }

    void Terminar()
    {
        // si usó más de 3 pistas, exige un acierto extra
        int requeridos = aciertosNecesarios + (pistasUsadas > 3 ? 1 : 0);
        bool exito = aciertos >= requeridos;
        interaccionManager.OnMinigameFinished(exito);
    }

    void Shuffle<T>(List<T> l) { for (int i = l.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (l[i], l[j]) = (l[j], l[i]); } }
}
