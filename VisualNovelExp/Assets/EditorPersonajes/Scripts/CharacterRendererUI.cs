// CharacterRendererUI_Fixed.cs - FIX imagen blanca + capas vacías
// Reemplaza tu CharacterRendererUI.cs con esta versión

using UnityEngine;
using UnityEngine.UI;

public class CharacterRendererUI : MonoBehaviour
{
    [Header("Capas Base")]
    public Image skinRenderer;
    public Image shoesRenderer;

    [Header("Ropa")]
    public Image shirtRenderer;
    public Image bottomRenderer;
    public Image dressRenderer;

    [Header("Pelo y Cara")]
    public Image hairBackRenderer;
    public Image faceRenderer;
    public Image hairFrontRenderer;

    [Header("Extras")]
    public Image accessoryRenderer;

    public void SetSkinColor(CosmeticItem item) => Apply(skinRenderer, item);
    public void SetHairColor(CosmeticItem item)
    {
        Apply(hairFrontRenderer, item);
        // Si tenés pelo atrás con sprite distinto, usá otro CosmeticItem
        // Por ahora lo dejamos sincronizado o lo podés dejar vacío
        // Apply(hairBackRenderer, item); 
    }
    public void SetShoes(CosmeticItem item) => Apply(shoesRenderer, item);
    public void SetAccessory(CosmeticItem item) => Apply(accessoryRenderer, item);


    void Awake()
    {
        // Arrancar todo apagado hasta que el CharacterCustomizer asigne algo
        Image[] all = GetComponentsInChildren<Image>(true);
        foreach (var img in all) Disable(img);
    }


    public void SetShirt(CosmeticItem item)
    {
        Apply(shirtRenderer, item);
        if (item != null && item.sprite != null)
        {
            // Al poner remera, apaga vestido
            Disable(dressRenderer);
        }
    }

    public void SetBottom(CosmeticItem item)
    {
        Apply(bottomRenderer, item);
        if (item != null && item.sprite != null)
        {
            Disable(dressRenderer);
        }
    }

    public void SetDress(CosmeticItem item)
    {
        Apply(dressRenderer, item);
        if (item != null && item.sprite != null)
        {
            Disable(shirtRenderer);
            Disable(bottomRenderer);
        }
    }

    // Aplicar con FIX de imagen blanca
    private void Apply(Image img, CosmeticItem item)
    {
        if (img == null) return;

        if (item == null || item.sprite == null)
        {
            Disable(img);
            return;
        }

        img.sprite = item.sprite;
        img.color = item.tint;
        img.enabled = true;
        // Si el Image estaba desactivado por SetActive, actívalo
        if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
        img.preserveAspect = true;
    }

    private void Disable(Image img)
    {
        if (img == null) return;
        // ESTE ES EL FIX CLAVE: No dejar Image.enabled = true sin sprite
        // Porque Unity dibuja un cuadro blanco cuando Image tiene sprite = null y enabled = true
        img.enabled = false;
        img.sprite = null;
        // Alternativa más agresiva si querés ahorrar draw calls:
        // img.gameObject.SetActive(false);
    }

    [ContextMenu("Limpiar Todo")]
    public void ClearAll()
    {
        Image[] all = GetComponentsInChildren<Image>(true);
        foreach (var r in all) Disable(r);
    }
}
