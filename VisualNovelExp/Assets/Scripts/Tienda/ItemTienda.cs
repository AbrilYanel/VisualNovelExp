using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTienda : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoCosto;
    public TextMeshProUGUI textoEstado;
    public Button botonComprar;

    // Debug visual
    [Header("Colores Debug")]
    public Color colorComprado = new Color(0.7f, 0.95f, 0.7f);

    private Image fondo;

    void Awake()
    {
        fondo = GetComponent<Image>();
        // Asegurar que el item sea visible
        if (fondo != null && fondo.color.a < 0.1f)
            fondo.color = Color.white;
    }

    public void Configurar(KanaData fila, bool desbloqueada, Action onComprarClick)
    {
        if (fila == null)
        {
            Debug.LogError("[ItemTienda] fila es NULL", this);
            return;
        }

        gameObject.name = $"ItemTienda_{fila.id}" + (desbloqueada ? "_OWNED" : "");

        if (textoNombre != null)
            textoNombre.text = string.IsNullOrEmpty(fila.nombreFila) ? fila.id : fila.nombreFila;
        else
            Debug.LogWarning("[ItemTienda] textoNombre NO asignado en prefab", this);

        if (textoCosto != null)
            textoCosto.text = desbloqueada ? "" : $"{fila.costo} monedas";

        if (textoEstado != null)
            textoEstado.text = desbloqueada ? "Desbloqueada" : "Disponible";

        if (fondo != null && desbloqueada)
            fondo.color = colorComprado;

        if (botonComprar != null)
        {
            botonComprar.gameObject.SetActive(!desbloqueada);
            botonComprar.interactable = !desbloqueada;
            botonComprar.onClick.RemoveAllListeners();
            if (onComprarClick != null)
                botonComprar.onClick.AddListener(() => onComprarClick.Invoke());
        }
        else if (!desbloqueada)
        {
            Debug.LogError("[ItemTienda] botonComprar NO asignado en prefab", this);
        }

        // Forzar que el objeto esté activo y visible
        gameObject.SetActive(true);
        var rt = GetComponent<RectTransform>();
        if (rt != null && rt.localScale == Vector3.zero)
            rt.localScale = Vector3.one;

        Debug.Log($"[ItemTienda] Configurado: {fila.nombreFila} desbloqueada={desbloqueada}", this);
    }
}
