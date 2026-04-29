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
    public Transform contenedorOriginal;
    private Vector2 posicionOriginal;
    private Image imagenFondo;
    private SlotPalabra slotActual = null;

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

        // Si estaba en un slot, liberarlo
        if (slotActual != null)
        {
            slotActual.LiberarBloque();
            slotActual = null;
        }

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        imagenFondo.color = colorNormal;

        imagenFondo.color = colorNormal;

        SlotPalabra slotCercano = EncontrarSlotCercano();

        if (slotCercano != null && slotCercano.EstaVacio())
        {
            slotActual = slotCercano;
            slotCercano.ColocarBloque(this);
        }
        else
        {
            // Volver al contenedor de bloques disponibles
            transform.SetParent(contenedorOriginal);
            rt.anchoredPosition = Vector2.zero;
            slotActual = null;
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