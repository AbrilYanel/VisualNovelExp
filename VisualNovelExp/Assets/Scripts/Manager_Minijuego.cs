using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Minijuego : MonoBehaviour
{
    [Header("Referencia al manager principal")]
    public Manager_Interaccion interaccionManager;

    [Header("Contenedores UI")]
    public Transform columnaImagenes;   // Panel izquierdo
    public Transform columnaPalabras;   // Panel derecho

    [Header("Prefabs")]
    public GameObject itemImagenPrefab;
    public GameObject itemPalabraPrefab;

    [Header("Línea de conexión")]
    public RectTransform lineaVisual;   // Image estirada entre dos puntos
    private List<GameObject> lineasActivas = new List<GameObject>();
    public GameObject lineaPrefab;

    [Header("Datos del minijuego")]
    public List<ParejaDatos> parejas;   // Asignás desde el Inspector

    [Header("UI")]
    public TextMeshProUGUI textoFeedback;
    public Button botonConfirmar;

    private MatchItem itemSeleccionado = null;
    private Dictionary<string, string> conexiones = new Dictionary<string, string>();
    // key: id imagen, value: id palabra conectada

    private int parejasCorrectas = 0;

    public void Iniciar()
    {
        // Validaciones primero
        if (textoFeedback == null) { Debug.LogError("textoFeedback no asignado"); return; }
        if (botonConfirmar == null) { Debug.LogError("botonConfirmar no asignado"); return; }
        if (columnaImagenes == null) { Debug.LogError("columnaImagenes no asignado"); return; }
        if (columnaPalabras == null) { Debug.LogError("columnaPalabras no asignado"); return; }
        if (itemImagenPrefab == null) { Debug.LogError("itemImagenPrefab no asignado"); return; }
        if (itemPalabraPrefab == null) { Debug.LogError("itemPalabraPrefab no asignado"); return; }
        if (parejas == null || parejas.Count == 0) { Debug.LogError("Lista parejas vacía"); return; }

        IniciarMinijuego();
    }

    void IniciarMinijuego()
    {
        // Limpiar estado anterior
        conexiones.Clear();
        parejasCorrectas = 0;
        itemSeleccionado = null;
        textoFeedback.text = "";

        // Limpiar columnas
        foreach (Transform t in columnaImagenes) Destroy(t.gameObject);
        foreach (Transform t in columnaPalabras) Destroy(t.gameObject);
        foreach (var l in lineasActivas) Destroy(l);
        lineasActivas.Clear();

        // Mezclar orden
        List<ParejaDatos> mezcladas = new List<ParejaDatos>(parejas);
        Shuffle(mezcladas);

        List<ParejaDatos> palabrasMezcladas = new List<ParejaDatos>(parejas);
        Shuffle(palabrasMezcladas);

        // Instanciar imágenes (izquierda)
        foreach (var pareja in mezcladas)
        {
            GameObject obj = Instantiate(itemImagenPrefab, columnaImagenes);
            MatchItem item = obj.GetComponent<MatchItem>();
            item.id = pareja.id;
            item.tipo = MatchItem.TipoItem.Imagen;

            Image img = obj.GetComponentInChildren<Image>();
            if (img != null) img.sprite = pareja.imagen;
            else Debug.LogError("No se encontró Image en ItemImagen");

            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnClickItem(item));
        }

        // Instanciar palabras (derecha)
        foreach (var pareja in palabrasMezcladas)
        {
            GameObject obj = Instantiate(itemPalabraPrefab, columnaPalabras);
            MatchItem item = obj.GetComponent<MatchItem>();
            item.id = pareja.id;
            item.tipo = MatchItem.TipoItem.Palabra;

            TextMeshProUGUI[] textos = obj.GetComponentsInChildren<TextMeshProUGUI>();
            if (textos.Length >= 2)
            {
                textos[0].text = pareja.palabraJaponesa;
                textos[1].text = pareja.romaji;
            }
            else Debug.LogError("ItemPalabra necesita al menos 2 TMP, tiene: " + textos.Length);

            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnClickItem(item));
        }

        botonConfirmar.gameObject.SetActive(false);
    }

    void OnClickItem(MatchItem itemClickeado)
    {
        // Si no hay nada seleccionado, seleccionar este
        if (itemSeleccionado == null)
        {
            itemSeleccionado = itemClickeado;
            itemClickeado.SetSeleccionado(true);
            return;
        }

        // Si clickeás el mismo, deseleccionar
        if (itemSeleccionado == itemClickeado)
        {
            itemSeleccionado.SetSeleccionado(false);
            itemSeleccionado = null;
            return;
        }

        // Si clickeás dos del mismo tipo, cambiar selección
        if (itemSeleccionado.tipo == itemClickeado.tipo)
        {
            itemSeleccionado.SetSeleccionado(false);
            itemSeleccionado = itemClickeado;
            itemClickeado.SetSeleccionado(true);
            return;
        }

        // Son de distinto tipo → intentar conectar
        MatchItem itemImagen = itemSeleccionado.tipo == MatchItem.TipoItem.Imagen
            ? itemSeleccionado : itemClickeado;
        MatchItem itemPalabra = itemSeleccionado.tipo == MatchItem.TipoItem.Palabra
            ? itemSeleccionado : itemClickeado;

        if (itemImagen.id == itemPalabra.id)
        {
            // ✅ Correcto
            itemImagen.SetConectado();
            itemPalabra.SetConectado();
            DibujarLinea(itemImagen.GetComponent<RectTransform>(),
                         itemPalabra.GetComponent<RectTransform>());

            conexiones[itemImagen.id] = itemPalabra.id;
            parejasCorrectas++;

            textoFeedback.text = "✓ " + itemImagen.id.ToUpper() + "!";

            if (parejasCorrectas >= parejas.Count)
            {
                textoFeedback.text = "¡Completaste todo!";
                botonConfirmar.gameObject.SetActive(true);
            }
        }
        else
        {
            // ❌ Incorrecto
            itemImagen.SetError();
            itemPalabra.SetError();
            textoFeedback.text = "Intentá de nuevo...";
        }

        itemSeleccionado = null;
    }

    void DibujarLinea(RectTransform desde, RectTransform hasta)
    {
        GameObject linea = Instantiate(lineaPrefab, transform);
        lineasActivas.Add(linea);

        RectTransform rt = linea.GetComponent<RectTransform>();

        Vector2 posDesde = desde.anchoredPosition + new Vector2(desde.rect.width / 2, 0);
        Vector2 posHasta = hasta.anchoredPosition - new Vector2(hasta.rect.width / 2, 0);

        Vector2 centro = (posDesde + posHasta) / 2;
        float distancia = Vector2.Distance(posDesde, posHasta);
        float angulo = Mathf.Atan2(posHasta.y - posDesde.y, posHasta.x - posDesde.x)
                       * Mathf.Rad2Deg;

        rt.anchoredPosition = centro;
        rt.sizeDelta = new Vector2(distancia, 4f); // 4px de grosor
        rt.rotation = Quaternion.Euler(0, 0, angulo);
    }

    public void ConfirmarResultado()
    {
        bool exito = parejasCorrectas >= parejas.Count;
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

// Estructura de datos para cada pareja
[System.Serializable]
public class ParejaDatos
{
    public string id;               // "neko", "sakana", etc.
    public Sprite imagen;
    public string palabraJaponesa;  // "猫"
    public string romaji;           // "Neko"
}