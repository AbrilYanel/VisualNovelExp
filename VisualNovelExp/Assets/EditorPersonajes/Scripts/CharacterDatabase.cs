// CharacterDatabase.cs - PASO 2
// Pon este archivo en: Assets/_CharacterEditor/Scripts/Data/CharacterDatabase.cs
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Character/Database")]
public class CharacterDatabase : ScriptableObject
{
    [Header("Base - Colores")]
    public CosmeticItem[] skinColors;
    public CosmeticItem[] hairColors;

    [Header("Ropa - Parte Superior")]
    public CosmeticItem[] shirts;

    [Header("Ropa - Parte Inferior")]
    public CosmeticItem[] skirts;
    public CosmeticItem[] jeans;

    [Header("Ropa - Cuerpo Completo")]
    [Tooltip("Los vestidos reemplazan camisa + parte inferior")]
    public CosmeticItem[] dresses;

    [Header("Calzado")]
    public CosmeticItem[] shoes;

    [Header("Extras")]
    public CosmeticItem[] accessories;

    // Helper para obtener un array por nombre de categoría (útil para la UI)
    public CosmeticItem[] GetCategory(string categoryName)
    {
        switch (categoryName.ToLower())
        {
            case "skin": return skinColors;
            case "hair": return hairColors;
            case "shirts": return shirts;
            case "skirts": return skirts;
            case "jeans": return jeans;
            case "dresses": return dresses;
            case "shoes": return shoes;
            case "accessories": return accessories;
            default: return null;
        }
    }

    // Para validar que no haya huecos vacíos en la DB
    private void OnValidate()
    {
        DebugCheckArray(skinColors, "skinColors");
        DebugCheckArray(hairColors, "hairColors");
    }

    private void DebugCheckArray(CosmeticItem[] array, string name)
    {
        if (array == null) return;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == null)
                Debug.LogWarning($"[CharacterDatabase] Hay un slot vacío en {name} en el índice {i}", this);
        }
    }
}
