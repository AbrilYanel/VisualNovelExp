using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class Manager_EscrituraInversa : MonoBehaviour
{
    [Header("Refs")]
    public Manager_Interaccion interaccionManager;
    public KanaInventario kanaInventario;

    [Header("UI")]
    [Tooltip("IMPORTANTE: aunque el campo se llama textoRomaji por compatibilidad, ahora muestra el HIRAGANA")]
    public TextMeshProUGUI textoRomaji; // REUTILIZADO: ahora muestra el kana grande (ひらがな)

    [Tooltip("Input donde el jugador escribe – ahora espera ROMAJI")]
    public TMP_InputField inputKana; // REUTILIZADO: ahora se escribe romaji aquí

    public TextMeshProUGUI textoPista;
    public TextMeshProUGUI textoProgreso;
    public TextMeshProUGUI textoFeedback;
    public Button botonConfirmar;
    public Button botonPista;

    [Header("UI extra opcional")]
    public TextMeshProUGUI textoSubtitulo; // ej: "Escribí el romaji"
    public TextMeshProUGUI textoPlaceholderInput; // para cambiar placeholder del InputField en runtime

    [Header("Datos")]
    public List<ItemEscritura> items;

    public List<KanaData> filasPermitidas; // ej: fila_ha

    [Header("Reglas")]
    public int rondas = 8;
    public int aciertosNecesarios = 5;

    [FormerlySerializedAs("aceptarRomajiComoFallback")]
    [Tooltip("Si el jugador pega el hiragana igual se acepta – útil si tienen IME")]
    public bool aceptarKanaComoFallback = true;

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
        if ((items == null || items.Count == 0) && filasPermitidas != null)
        {
            items = new List<ItemEscritura>();
            foreach (var fila in filasPermitidas)
            {
                if (fila == null) continue;
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

        if (mazo.Count == 0) mazo = new List<ItemEscritura>(items); // fallback test

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

        // >>> CAMBIO CLAVE: mostrar HIRAGANA grande <<<
        if (textoRomaji)
        {
            textoRomaji.text = actual.kanaCorrecto; // ej: "は"
            textoRomaji.fontSize = 96; // grande, ajustá en inspector si querés
        }

        // subtítulo ayuda
        if (textoSubtitulo) textoSubtitulo.text = "Escribí el romaji y presioná Enter";

        if (textoProgreso) textoProgreso.text = $"{indice + 1} / {Mathf.Min(rondas, mazo.Count)}   ✓ {aciertos}";
        if (textoPista) textoPista.text = "";
        if (textoFeedback) textoFeedback.text = "";

        if (inputKana)
        {
            inputKana.text = "";
            // cambiar placeholder a "ha / hi / fu..."
            var ph = inputKana.placeholder as TextMeshProUGUI;
            if (ph) ph.text = "escribí en romaji...";
            if (textoPlaceholderInput) textoPlaceholderInput.text = "romaji...";
            inputKana.ActivateInputField();
            inputKana.Select();
        }
    }

    void DarPista()
    {
        pistasUsadas++;
        if (textoPista)
        {
            // Pista progresiva: 1ª vez primera letra, 2ª vez 2 letras, etc.
            string rom = actual.romaji;
            string pistaMostrar = pistasUsadas == 1 ? $"{rom[0]}___" :
                                  pistasUsadas == 2 && rom.Length > 1 ? $"{rom.Substring(0, Mathf.Min(2, rom.Length))}___" :
                                  rom; // tercera pista revela todo
            textoPista.text = $"💡 {pistaMostrar}  ·  {actual.idFilaKana}";
        }
    }

    public void Confirmar()
    {
        if (actual == null) return;
        string respuesta = inputKana ? inputKana.text.Trim().ToLower() : "";
        // normalizar: quitar espacios, pasar a minúsculas
        respuesta = respuesta.Replace(" ", "").Replace("-", "");

        if (string.IsNullOrEmpty(respuesta))
        {
            if (textoFeedback) { textoFeedback.color = Color.yellow; textoFeedback.text = "Escribí el romaji y presioná Enter"; }
            if (inputKana) { inputKana.ActivateInputField(); }
            return;
        }

        // >>> CAMBIO CLAVE: ahora la respuesta correcta es ROMAJI <<<
        bool ok = respuesta == actual.romaji.ToLower();

        // fallback: si escribe el kana y tiene IME, también aceptamos
        if (!ok && aceptarKanaComoFallback)
            ok = respuesta == actual.kanaCorrecto;

        // aceptar variantes comunes: hu = fu, si = shi, chi = ti, etc.
        if (!ok)
            ok = EsVarianteAceptable(respuesta, actual.romaji.ToLower());

        if (textoFeedback)
        {
            textoFeedback.color = ok ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
            textoFeedback.text = ok ? "¡Correcto!" : $"Era: {actual.romaji}  ({actual.kanaCorrecto})";
        }

        if (ok) aciertos++;
        indice++;
        Invoke(nameof(Siguiente), ok ? 0.9f : 1.4f);
    }

    // acepta variantes de romanización hepburn / kunrei
    bool EsVarianteAceptable(string input, string correcto)
    {
        var mapa = new Dictionary<string, string[]>
        {
            {"shi", new[]{"si","shi","ci"}},
            {"chi", new[]{"ti","chi"}},
            {"tsu", new[]{"tu","tsu"}},
            {"fu", new[]{"hu","fu"}},
            {"ji", new[]{"zi","ji","dji"}},
            {"sha", new[]{"sya","sha"}},
            {"shu", new[]{"syu","shu"}},
            {"sho", new[]{"syo","sho"}},
            {"cha", new[]{"tya","cha"}},
            {"chu", new[]{"tyu","chu"}},
            {"cho", new[]{"tyo","cho"}},
            {"ja", new[]{"zya","ja"}},
            {"ju", new[]{"zyu","ju"}},
            {"jo", new[]{"zyo","jo"}},
        };
        if (mapa.ContainsKey(correcto))
            return System.Array.Exists(mapa[correcto], v => v == input);
        // búsqueda inversa
        foreach (var kv in mapa)
            if (System.Array.Exists(kv.Value, v => v == correcto) && System.Array.Exists(kv.Value, v => v == input))
                return true;
        return false;
    }

    void Terminar()
    {
        int requeridos = aciertosNecesarios + (pistasUsadas > 3 ? 1 : 0);
        bool exito = aciertos >= requeridos;
        Debug.Log($"[EscrituraInversa] Fin: {aciertos}/{rondas}  pistas:{pistasUsadas}  exito:{exito}");
        interaccionManager.OnMinigameFinished(exito);
    }

    void Shuffle<T>(List<T> l) { for (int i = l.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (l[i], l[j]) = (l[j], l[i]); } }

    // test rápido desde inspector
    [ContextMenu("Test Iniciar")]
    void TestIniciar() => Iniciar();
}
