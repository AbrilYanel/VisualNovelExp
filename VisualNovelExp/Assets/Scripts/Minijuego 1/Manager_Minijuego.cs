using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Manager_Minijuego : MonoBehaviour
{
    [Header("Referencia al manager principal")]
    public Manager_Interaccion interaccionManager;

    [Header("Contenedores UI")]
    public Transform columnaImagenes;
    public Transform columnaPalabras;

    [Header("Prefabs")]
    public GameObject itemImagenPrefab;
    public GameObject itemPalabraPrefab;

    [Header("Línea de conexión")]
    public GameObject lineaPrefab;
    private List<GameObject> lineasActivas = new List<GameObject>();

    [Header("Datos del minijuego")]
    public List<ParejaDatos> parejas;

    [Header("UI")]
    public TextMeshProUGUI textoFeedback;
    public Button botonConfirmar;

    // --- PASO 3: vidas / intentos ---
    [Header("Paso 3 - Vidas")]
    public int erroresMaximos = 3;
    public TextMeshProUGUI textoVidas; // asignar en inspector: "Vidas: ❤❤❤"
    private int erroresActuales = 0;

    // --- PASO 5: bloqueo por kanas ---
    [Header("Progresión Kanas")]
    public KanaInventario kanaInventario; // arrastrar asset

    private MatchItem itemSeleccionado = null;
    private int parejasCorrectas = 0;

    public void Iniciar()
    {
        if (textoFeedback == null || botonConfirmar == null || columnaImagenes == null || columnaPalabras == null)
        { Debug.LogError("[Minijuego] Faltan referencias UI"); return; }
        if (parejas == null || parejas.Count == 0)
        { Debug.LogError("[Minijuego] Lista parejas vacía"); return; }

        IniciarMinijuego();
    }

    void IniciarMinijuego()
    {
        parejasCorrectas = 0;
        erroresActuales = 0;
        itemSeleccionado = null;
        textoFeedback.text = "";
        ActualizarVidasUI();

        foreach (Transform t in columnaImagenes) Destroy(t.gameObject);
        foreach (Transform t in columnaPalabras) Destroy(t.gameObject);
        foreach (var l in lineasActivas) Destroy(l);
        lineasActivas.Clear();

        List<ParejaDatos> mezcladasIzq = new List<ParejaDatos>(parejas);
        Shuffle(mezcladasIzq);
        List<ParejaDatos> mezcladasDer = new List<ParejaDatos>(parejas);
        Shuffle(mezcladasDer);

        // Izquierda - imágenes
        foreach (var pareja in mezcladasIzq)
        {
            GameObject obj = Instantiate(itemImagenPrefab, columnaImagenes);
            MatchItem item = obj.GetComponent<MatchItem>();
            item.id = pareja.id;
            item.tipo = MatchItem.TipoItem.Imagen;

            Image img = obj.GetComponentInChildren<Image>();
            if (img != null) img.sprite = pareja.imagen;

            AddClick(obj, item);
        }

        // Derecha - palabras (con bloqueo 🔒)
        foreach (var pareja in mezcladasDer)
        {
            GameObject obj = Instantiate(itemPalabraPrefab, columnaPalabras);
            MatchItem item = obj.GetComponent<MatchItem>();
            item.id = pareja.id;
            item.tipo = MatchItem.TipoItem.Palabra;

            bool bloqueado = false;
            if (kanaInventario != null && !string.IsNullOrEmpty(pareja.idFilaKana))
            {
                bloqueado = !kanaInventario.EstaDesbloqueado(pareja.idFilaKana);
            }

            TextMeshProUGUI[] textos = obj.GetComponentsInChildren<TextMeshProUGUI>();
            if (textos.Length >= 2)
            {
                if (bloqueado)
                {
                    textos[0].text = "🔒";
                    textos[1].text = "???";
                }
                else
                {
                    textos[0].text = pareja.palabraJaponesa;
                    textos[1].text = pareja.romaji;
                }
            }

            AddClick(obj, item);
        }

        botonConfirmar.gameObject.SetActive(false);
    }

    void AddClick(GameObject obj, MatchItem item)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        MatchItem captured = item;
        entry.callback.AddListener((data) => OnClickItem(captured));
        trigger.triggers.Add(entry);
    }

    void OnClickItem(MatchItem itemClickeado)
    {
        if (itemSeleccionado == null)
        {
            itemSeleccionado = itemClickeado;
            itemClickeado.SetSeleccionado(true);
            return;
        }
        if (itemSeleccionado == itemClickeado)
        {
            itemSeleccionado.SetSeleccionado(false);
            itemSeleccionado = null;
            return;
        }
        if (itemSeleccionado.tipo == itemClickeado.tipo)
        {
            itemSeleccionado.SetSeleccionado(false);
            itemSeleccionado = itemClickeado;
            itemClickeado.SetSeleccionado(true);
            return;
        }

        MatchItem itemImagen = itemSeleccionado.tipo == MatchItem.TipoItem.Imagen ? itemSeleccionado : itemClickeado;
        MatchItem itemPalabra = itemSeleccionado.tipo == MatchItem.TipoItem.Palabra ? itemSeleccionado : itemClickeado;

        if (itemImagen.id == itemPalabra.id)
        {
            // correcto
            itemImagen.SetConectado();
            itemPalabra.SetConectado();
            parejasCorrectas++;
            textoFeedback.text = "¡Bien! " + itemImagen.id.ToUpper();
            textoFeedback.color = Color.green;

           

            if (parejasCorrectas >= parejas.Count)
            {
                textoFeedback.text = "¡Completaste todo!";
                botonConfirmar.gameObject.SetActive(true);
                // auto confirmar a los 0.8s
                Invoke(nameof(ConfirmarResultado), 0.8f);
            }
        }
        else
        {
           
            itemImagen.SetError();
            itemPalabra.SetError();
            erroresActuales++;
            ActualizarVidasUI();
            textoFeedback.text = $"Incorrecto... Vidas: {erroresMaximos - erroresActuales}";
            textoFeedback.color = Color.red;

            if (erroresActuales >= erroresMaximos)
            {
                textoFeedback.text = "¡Sin intentos!";
                Invoke(nameof(FallarMinijuego), 1.0f);
                return;
            }
        }

        itemSeleccionado?.SetSeleccionado(false);
        itemSeleccionado = null;
    }

    void ActualizarVidasUI()
    {
        if (textoVidas == null) return;
        int vidasRestantes = Mathf.Max(0, erroresMaximos - erroresActuales);
        string corazones = "";
        for (int i = 0; i < vidasRestantes; i++) corazones += "❤";
        for (int i = vidasRestantes; i < erroresMaximos; i++) corazones += "♡";
        textoVidas.text = $"{corazones}  ({vidasRestantes}/{erroresMaximos})";
    }

    void FallarMinijuego()
    {
        interaccionManager.OnMinigameFinished(false);
    }

    public void ConfirmarResultado()
    {
        bool exito = parejasCorrectas >= parejas.Count;
        if (exito)
        {
            // registrar en journal
            var journal = FindObjectOfType<Manager_Journal>();
            if (journal != null)
            {
                List<PalabraAprendida> aprendidas = new List<PalabraAprendida>();
                foreach (var p in parejas)
                {
                    aprendidas.Add(new PalabraAprendida
                    {
                        hiragana = p.palabraJaponesa,
                        romaji = p.romaji,
                        traduccion = p.traduccion,
                        idFuente = p.id
                    });
                }
                //journal.RegistrarPalabras(aprendidas);
            }
        }
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

[System.Serializable]
public class ParejaDatos
{
    public string id;
    public Sprite imagen;
    public string palabraJaponesa;
    public string romaji;
    public string traduccion;

    [Header("Progresión Kanas - Paso 5")]
    public string idFilaKana = "fila_a"; 
}
