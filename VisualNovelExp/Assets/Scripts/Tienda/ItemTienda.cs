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

    [Header("Colores Debug")]
    public Color colorComprado = new Color(0.7f, 0.95f, 0.7f);

    private Image fondo;

    void Awake()
    {
        fondo = GetComponent<Image>();
        if (fondo != null && fondo.color.a < 0.1f)
            fondo.color = Color.white;
    }

    // NUEVO: artículo de la konbini (se puede comprar varias veces)
    public void ConfigurarArticulo(string nombre, string costo, string descripcion, Action onComprarClick)
    {
        gameObject.name = "ItemTienda_" + nombre;

        if (textoNombre != null) textoNombre.text = nombre;
        if (textoCosto != null) textoCosto.text = costo;
        if (textoEstado != null) textoEstado.text = descripcion;

        if (botonComprar != null)
        {
            botonComprar.gameObject.SetActive(true);
            botonComprar.interactable = true;
            botonComprar.onClick.RemoveAllListeners();
            if (onComprarClick != null)
                botonComprar.onClick.AddListener(() => onComprarClick.Invoke());
        }
        else
        {
            Debug.LogError("[ItemTienda] botonComprar NO asignado en prefab", this);
        }

        gameObject.SetActive(true);
        var rt = GetComponent<RectTransform>();
        if (rt != null && rt.localScale == Vector3.zero)
            rt.localScale = Vector3.one;
    }

    // ANTERIOR: fila de kana. Se conserva para que ningún otro script deje de compilar.
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

        gameObject.SetActive(true);
        var rt = GetComponent<RectTransform>();
        if (rt != null && rt.localScale == Vector3.zero)
            rt.localScale = Vector3.one;
    }
}
