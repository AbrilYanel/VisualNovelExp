using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BloquePalabra : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TextMeshProUGUI texto;
    public string valor;

    private RectTransform rt;
    private Canvas canvas;
    private Transform contenedorOriginal;
    private Vector2 posicionOriginal;
    private Image imagenFondo;

    [Header("Colores")]
    public Color colorNormal = Color.white;
    public Color colorArrastrando = new Color(1f, 1f, 0.6f);

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        imagenFondo = GetComponent<Image>();
    }

    public void Configurar(string _valor, Transform _contenedorOriginal)
    {
        valor = _valor;
        texto.text = _valor;
        contenedorOriginal = _contenedorOriginal;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        posicionOriginal = rt.anchoredPosition;
        imagenFondo.color = colorArrastrando;
        transform.SetParent(canvas.transform); // sale del layout al arrastrar
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        imagenFondo.color = colorNormal;

        // Buscar slot cercano
        SlotPalabra slotCercano = EncontrarSlotCercano();

        if (slotCercano != null && slotCercano.EstaVacio())
        {
            slotCercano.ColocarBloque(this);
        }
        else
        {
            // Volver al contenedor original
            transform.SetParent(contenedorOriginal);
            rt.anchoredPosition = posicionOriginal;
        }
    }

    SlotPalabra EncontrarSlotCercano()
    {
        SlotPalabra[] slots = FindObjectsOfType<SlotPalabra>();
        SlotPalabra cercano = null;
        float distanciaMin = 80f; // distancia máxima para "soltar"

        foreach (var slot in slots)
        {
            float dist = Vector2.Distance(
                rt.position,
                slot.GetComponent<RectTransform>().position
            );
            if (dist < distanciaMin)
            {
                distanciaMin = dist;
                cercano = slot;
            }
        }
        return cercano;
    }
}