using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Memorama : MonoBehaviour
{
    [Header("Ref")]
    public Manager_Interaccion interaccionManager;
    public KanaInventario kanaInventario;

    [Header("UI")]
    public Transform contenedorCartas; // GridLayoutGroup
    public GameObject cartaPrefab; // necesita componente CartaMemorama
    public TextMeshProUGUI textoFeedback;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoProgreso;

    [Header("Datos")]
    public List<ParejaDatos> parejas; // reutiliza ParejaDatos (id, imagen, palabraJaponesa, romaji, idFilaKana)

    [Header("Reglas")]
    public int erroresMaximos = 3;
    public float tiempoMostrarCarta = 0.9f;

    List<CartaMemorama> cartasEnMesa = new List<CartaMemorama>();
    CartaMemorama primera = null;
    CartaMemorama segunda = null;
    bool bloqueInput = false;
    int aciertos = 0;
    int errores = 0;
    int totalParejas = 0;

    public void Iniciar()
    {
        // limpiar
        foreach (Transform t in contenedorCartas) Destroy(t.gameObject);
        cartasEnMesa.Clear();
        primera = segunda = null;
        bloqueInput = false;
        aciertos = 0;
        errores = 0;

        // armar mazo: 2 cartas por pareja
        List<ParejaDatos> mazo = new List<ParejaDatos>();
        foreach (var p in parejas) { mazo.Add(p); mazo.Add(p); }
        Shuffle(mazo);
        totalParejas = parejas.Count;

        foreach (var dato in mazo)
        {
            GameObject go = Instantiate(cartaPrefab, contenedorCartas);
            var carta = go.GetComponent<CartaMemorama>();
            if (carta == null) { Debug.LogError("cartaPrefab necesita CartaMemorama"); continue; }

            bool bloqueada = kanaInventario != null && !string.IsNullOrEmpty(dato.idFilaKana) && !kanaInventario.EstaDesbloqueado(dato.idFilaKana);
            carta.Configurar(dato, bloqueada, OnCartaClick);
            cartasEnMesa.Add(carta);
        }

        ActualizarUI();
        if (textoFeedback) textoFeedback.text = "¡Encontrá las parejas!";
    }

    void OnCartaClick(CartaMemorama carta)
    {
        if (bloqueInput) return;
        if (carta.estaRevelada || carta.estaEmparejada) return;

        carta.Revelar(true);

        if (primera == null)
        {
            primera = carta;
            return;
        }
        if (segunda == null && carta != primera)
        {
            segunda = carta;
            StartCoroutine(EvaluarPareja());
        }
    }

    IEnumerator EvaluarPareja()
    {
        bloqueInput = true;
        yield return new WaitForSeconds(tiempoMostrarCarta);

        if (primera != null && segunda != null && primera.datoPareja.id == segunda.datoPareja.id)
        {
            // acierto
            primera.MarcarEmparejada();
            segunda.MarcarEmparejada();
            aciertos++;
            if (textoFeedback) { textoFeedback.color = Color.green; textoFeedback.text = "¡Par encontrado!"; }
            if (aciertos >= totalParejas)
            {
                yield return new WaitForSeconds(0.6f);
                interaccionManager.OnMinigameFinished(true);
                yield break;
            }
        }
        else
        {
            // fallo
            if (primera) primera.Revelar(false);
            if (segunda) segunda.Revelar(false);
            errores++;
            if (textoFeedback) { textoFeedback.color = Color.red; textoFeedback.text = "No coinciden..."; }
            ActualizarUI();
            if (errores >= erroresMaximos)
            {
                yield return new WaitForSeconds(0.7f);
                interaccionManager.OnMinigameFinished(false);
                yield break;
            }
        }

        primera = null;
        segunda = null;
        bloqueInput = false;
        ActualizarUI();
    }

    void ActualizarUI()
    {
        if (textoVidas)
        {
            int vidas = Mathf.Max(0, erroresMaximos - errores);
            string s = "";
            for (int i = 0; i < vidas; i++) s += "❤";
            for (int i = vidas; i < erroresMaximos; i++) s += "♡";
            textoVidas.text = $"{s}  {vidas}/{erroresMaximos}";
        }
        if (textoProgreso)
            textoProgreso.text = $"Pares: {aciertos}/{totalParejas}";
    }

    void Shuffle<T>(List<T> l) { for (int i = l.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (l[i], l[j]) = (l[j], l[i]); } }
}

// -------------------------------------------------
// Componente para cada carta
// Poner este script en el prefab cartaPrefab
// -------------------------------------------------
public class CartaMemorama : MonoBehaviour
{
    [Header("UI refs")]
    public Image imagenFondo;
    public Image imagenContenido; // para sprite (opcional)
    public TextMeshProUGUI textoKana;
    public TextMeshProUGUI textoRomaji;
    public Button boton;

    [HideInInspector] public ParejaDatos datoPareja;
    [HideInInspector] public bool estaRevelada = false;
    [HideInInspector] public bool estaEmparejada = false;

    System.Action<CartaMemorama> onClick;
    bool bloqueada = false;

    void Awake()
    {
        if (boton == null) boton = GetComponent<Button>();
        if (boton) boton.onClick.AddListener(() => onClick?.Invoke(this));
    }

    public void Configurar(ParejaDatos dato, bool esBloqueada, System.Action<CartaMemorama> callback)
    {
        datoPareja = dato;
        bloqueada = esBloqueada;
        onClick = callback;
        estaRevelada = false;
        estaEmparejada = false;
        ActualizarVisual(false);
    }

    public void Revelar(bool mostrar)
    {
        estaRevelada = mostrar;
        ActualizarVisual(mostrar);
    }

    public void MarcarEmparejada()
    {
        estaEmparejada = true;
        estaRevelada = true;
        if (imagenFondo) imagenFondo.color = new Color(0.6f, 1f, 0.6f);
        if (boton) boton.interactable = false;
        ActualizarVisual(true);
    }

    void ActualizarVisual(bool revelada)
    {
        if (!revelada)
        {
            // dorso
            if (textoKana) textoKana.text = "?";
            if (textoRomaji) textoRomaji.text = "";
            if (imagenContenido) imagenContenido.enabled = false;
            if (imagenFondo) imagenFondo.color = Color.white;
        }
        else
        {
            if (bloqueada)
            {
                if (textoKana) textoKana.text = "🔒";
                if (textoRomaji) textoRomaji.text = "???";
                if (imagenContenido) imagenContenido.enabled = false;
            }
            else
            {
                if (textoKana) textoKana.text = datoPareja.palabraJaponesa;
                if (textoRomaji) textoRomaji.text = datoPareja.romaji;
                if (imagenContenido && datoPareja.imagen != null)
                {
                    imagenContenido.enabled = true;
                    imagenContenido.sprite = datoPareja.imagen;
                }
                else if (imagenContenido) imagenContenido.enabled = false;
            }
            if (imagenFondo && !estaEmparejada) imagenFondo.color = new Color(1f, 0.95f, 0.7f);
        }
    }
}
