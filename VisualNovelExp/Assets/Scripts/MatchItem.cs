using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchItem : MonoBehaviour
{
    public enum TipoItem { Imagen, Palabra }
    public TipoItem tipo;
    public string id; // clave para comparar, ej: "neko"

    private bool seleccionado = false;
    private Image imagenFondo;

    [Header("Colores")]
    public Color colorNormal = Color.white;
    public Color colorSeleccionado = Color.yellow;
    public Color colorConectado = Color.green;
    public Color colorError = Color.red;

    void Awake()
    {
        imagenFondo = GetComponent<Image>();
    }

    public void SetSeleccionado(bool value)
    {
        seleccionado = value;
        imagenFondo.color = value ? colorSeleccionado : colorNormal;
    }

    public void SetConectado()
    {
        imagenFondo.color = colorConectado;
        GetComponent<Button>().interactable = false;
    }

    public void SetError()
    {
        StartCoroutine(FlashError());
    }

    System.Collections.IEnumerator FlashError()
    {
        imagenFondo.color = colorError;
        yield return new WaitForSeconds(0.5f);
        imagenFondo.color = colorNormal;
        SetSeleccionado(false);
    }

    public bool EstaSeleccionado() => seleccionado;
}