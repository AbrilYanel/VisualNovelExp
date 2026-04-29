using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotPalabra : MonoBehaviour
{
    public BloquePalabra OcupadoPor { get; private set; }
    private Image imagenFondo;

    public Color colorVacio = new Color(0.9f, 0.9f, 0.9f);
    public Color colorOcupado = new Color(0.8f, 0.95f, 0.8f);

    void Awake()
    {
        imagenFondo = GetComponent<Image>();
        imagenFondo.color = colorVacio;
    }

    public void ColocarBloque(BloquePalabra bloque)
    {
        if (OcupadoPor != null)
            LiberarBloque();

        OcupadoPor = bloque;
        RectTransform bloqueRT = bloque.GetComponent<RectTransform>();
        RectTransform slotRT = GetComponent<RectTransform>();

        bloque.transform.SetParent(transform, false); // false = no mantener posición mundial

        // Forzar que ocupe todo el slot
        bloqueRT.anchorMin = Vector2.zero;
        bloqueRT.anchorMax = Vector2.one;
        bloqueRT.offsetMin = Vector2.zero;
        bloqueRT.offsetMax = Vector2.zero;

        imagenFondo.color = colorOcupado;
    }

    public void LiberarBloque()
    {
        if (OcupadoPor == null) return;

        OcupadoPor.transform.SetParent(OcupadoPor.contenedorOriginal);
        OcupadoPor.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        OcupadoPor = null;
        imagenFondo.color = colorVacio;
    }

    public bool EstaVacio() => OcupadoPor == null;
}