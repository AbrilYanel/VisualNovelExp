// CharacterSlotConfig.cs - PASO 2
// Pon en: Assets/_CharacterEditor/Scripts/Data/CharacterSlotConfig.cs
//
// Define TODAS las categorias, subcategorias (slots) y el orden de apilado.
// Agregar un slot nuevo en el futuro = agregar una fila en este asset.

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDefinition
{
    [Tooltip("ID unico, ej: hair_front. Se usa en el guardado: no lo cambies despues.")]
    public string id;

    [Tooltip("Texto del boton de subcategoria")]
    public string displayName;

    [Tooltip("Carpeta dentro de Art/, ej: Hair/Front")]
    public string folder;

    [Tooltip("Orden de apilado: menor = mas atras, mayor = mas adelante")]
    public int sortOrder;

    [Tooltip("Si esta activo, aparece el boton 'ninguno' en este slot")]
    public bool canBeEmpty = true;
}

[System.Serializable]
public class CategoryDefinition
{
    public string id;
    public string displayName;
    public List<SlotDefinition> slots = new List<SlotDefinition>();
}

[CreateAssetMenu(fileName = "CharacterSlotConfig", menuName = "Character/Slot Config")]
public class CharacterSlotConfig : ScriptableObject
{
    public List<CategoryDefinition> categories = new List<CategoryDefinition>();

    public IEnumerable<SlotDefinition> AllSlots()
    {
        foreach (var cat in categories)
            foreach (var slot in cat.slots)
                yield return slot;
    }

    public SlotDefinition GetSlot(string slotId)
    {
        foreach (var slot in AllSlots())
            if (slot.id == slotId) return slot;
        return null;
    }

    // ---------- Configuracion inicial ----------
    // Click derecho en el titulo del componente en el Inspector > "Cargar configuracion por defecto"
    [ContextMenu("Cargar configuracion por defecto")]
    public void LoadDefaults()
    {
        categories = new List<CategoryDefinition>
        {
            Cat("skin", "Skin",
                Slot("skin_body", "Body", "Skin/Body", 100, false)),

            Cat("hair", "Hair",
                Slot("hair_front",           "Front",             "Hair/Front",          310),
                Slot("hair_right_front",     "Right front side",  "Hair/RightFrontSide", 300),
                Slot("hair_left_front",      "Left front side",   "Hair/LeftFrontSide",  300),
                Slot("hair_upper_back",      "Upper back",        "Hair/UpperBack",       10),
                Slot("hair_lower_back",      "Lower back",        "Hair/LowerBack",        0),
                Slot("hair_left_lower",      "Left lower",        "Hair/LeftLower",       20),
                Slot("hair_right_lower",     "Right lower",       "Hair/RightLower",      20)),

            Cat("face", "Face",
                Slot("face_ears",      "Ears",       "Face/Ears",      110),
                Slot("face_mouth",     "Mouth",      "Face/Mouth",     130),
                Slot("face_eye_shape", "Eye shape",  "Face/EyeShape",  140),
                Slot("face_pupil",     "Pupil",      "Face/Pupil",     150),
                Slot("face_eye_spark", "Eye spark",  "Face/EyeSpark",  160),
                Slot("face_blush",     "Blush",      "Face/Blush",     120),
                Slot("face_moles",     "Moles",      "Face/Moles",     135)),

            Cat("clothes", "Clothes",
                Slot("clothes_shirt",  "Shirt",   "Clothes/Shirt",  200),
                Slot("clothes_shirt2", "Shirt 2", "Clothes/Shirt2", 210),
                Slot("clothes_jacket", "Jacket",  "Clothes/Jacket", 220)),

            Cat("accessories", "Accessories",
                Slot("acc_head",    "Head",    "Accessories/Head",    330),
                Slot("acc_collar",  "Collar",  "Accessories/Collar",  230),
                Slot("acc_glasses", "Glasses", "Accessories/Glasses", 320)),
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private static CategoryDefinition Cat(string id, string name, params SlotDefinition[] slots)
    {
        return new CategoryDefinition { id = id, displayName = name, slots = new List<SlotDefinition>(slots) };
    }

    private static SlotDefinition Slot(string id, string name, string folder, int order, bool canBeEmpty = true)
    {
        return new SlotDefinition { id = id, displayName = name, folder = folder, sortOrder = order, canBeEmpty = canBeEmpty };
    }

    // Avisa si hay IDs repetidos (rompen el guardado)
    private void OnValidate()
    {
        var seen = new HashSet<string>();
        foreach (var slot in AllSlots())
        {
            if (string.IsNullOrEmpty(slot.id)) continue;
            if (!seen.Add(slot.id))
                Debug.LogWarning($"[CharacterSlotConfig] ID de slot repetido: {slot.id}", this);
        }
    }
}