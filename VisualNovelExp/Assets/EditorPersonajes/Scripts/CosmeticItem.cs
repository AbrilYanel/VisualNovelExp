// CosmeticItem.cs - PASO 2
// Pon este archivo en: Assets/_CharacterEditor/Scripts/Data/CosmeticItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewCosmeticItem", menuName = "Character/Cosmetic Item")]
public class CosmeticItem : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("ID único, ej: hair_01, shirt_03, skin_02")]
    public string id;

    [Header("Visuales")]
    [Tooltip("Miniatura para el botón de la UI - 128x128 ideal")]
    public Sprite icon;

    [Tooltip("Sprite real que se pone sobre el personaje")]
    public Sprite sprite;

    [Tooltip("Color para tinte. Blanco = sin tinte. Útil para piel/pelo con un solo sprite base")]
    public Color tint = Color.white;

    // Valida que el ID se genere solo si está vacío
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = name.ToLower().Replace(" ", "_");
        }
        // Si no tiene icono, usa el sprite real como fallback
        if (icon == null && sprite != null)
        {
            icon = sprite;
        }
    }
}
