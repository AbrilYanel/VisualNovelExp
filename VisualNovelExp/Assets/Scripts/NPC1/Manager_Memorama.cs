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

    [Header("Preview inicial")]
    [Tooltip("Segundos que se muestran todas las cartas al empezar")]
    public float tiempoPreviewInicial = 2.5f;
    public bool previewInicialActivado = true;
    public TextMeshProUGUI textoCuentaRegresiva; // opcional: "Memorizá... 3...2...1"

    List<CartaMemorama> cartasEnMesa = new List<CartaMemorama>();
    CartaMemorama primera = null;
    CartaMemorama segunda = null;
    bool bloqueInput = false;
    int aciertos = 0;
    int errores = 0;
    int totalParejas = 0;

    public void Iniciar()
    {
        StopAllCoroutines();
        // limpiar
        foreach (Transform t in contenedorCartas) Destroy(t.gameObject);
        cartasEnMesa.Clear();
        primera = segunda = null;
        bloqueInput = true; // bloqueado hasta terminar preview
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

        if (previewInicialActivado && tiempoPreviewInicial > 0f)
        {
            StartCoroutine(RutinaPreviewInicial());
        }
        else
        {
            bloqueInput = false;
            if (textoFeedback) textoFeedback.text = "¡Encontrá las parejas!";
        }
    }

    IEnumerator RutinaPreviewInicial()
    {
        // Mostrar todas boca arriba
        foreach (var c in cartasEnMesa)
        {
            if (c != null) c.Revelar(true);
            if (c.boton) c.boton.interactable = false;
        }

        float restante = tiempoPreviewInicial;
        while (restante > 0f)
        {
            if (textoFeedback)
            {
                textoFeedback.color = new Color(1f, 0.6f, 0.1f);
                textoFeedback.text = $"Memorizá las posiciones... {Mathf.CeilToInt(restante)}";
            }
            if (textoCuentaRegresiva)
                textoCuentaRegresiva.text = Mathf.CeilToInt(restante).ToString();

            yield return new WaitForSeconds(0.1f);
            restante -= 0.1f;
        }

        // Ocultar con pequeño efecto escalonado (más lindo)
        for (int i = 0; i < cartasEnMesa.Count; i++)
        {
            var c = cartasEnMesa[i];
            if (c != null && !c.estaEmparejada)
            {
                c.Revelar(false);
                if (c.boton) c.boton.interactable = true;
            }
            if (i % 4 == 3) yield return new WaitForSeconds(0.04f); // wave effect
        }

        if (textoCuentaRegresiva) textoCuentaRegresiva.text = "";
        if (textoFeedback)
        {
            textoFeedback.color = Color.white;
            textoFeedback.text = "¡A jugar!";
        }

        yield return new WaitForSeconds(0.4f);

        bloqueInput = false;
        if (textoFeedback) textoFeedback.text = "¡Encontrá las parejas!";
        ActualizarUI();
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

