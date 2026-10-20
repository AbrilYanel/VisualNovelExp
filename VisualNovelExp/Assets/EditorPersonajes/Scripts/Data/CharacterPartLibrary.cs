// CharacterPartLibrary.cs - PASO 3
// Pon en: Assets/_CharacterEditor/Scripts/Data/CharacterPartLibrary.cs
//
// Reemplaza a CharacterDatabase + todos los CosmeticItem sueltos.
// Guarda las piezas de cada slot y se llena SOLO leyendo las carpetas de Art/.

using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

// Una pieza (ya no es un ScriptableObject, es un dato simple dentro de la biblioteca)
[System.Serializable]
public class PartEntry
{
    [Tooltip("Nombre del archivo sin extension, ej: 001. Es lo que se guarda en el save.")]
    public string id;
    public Sprite icon;   // miniatura del boton
    public Sprite sprite; // pieza real sobre el personaje
    public Color tint = Color.white;
}

[System.Serializable]
public class SlotItems
{
    public string slotId;
    public List<PartEntry> items = new List<PartEntry>();
}

[CreateAssetMenu(fileName = "CharacterPartLibrary", menuName = "Character/Part Library")]
public class CharacterPartLibrary : ScriptableObject
{
    public CharacterSlotConfig config;
    public List<SlotItems> slots = new List<SlotItems>();

    // ---------- Consultas (las usan el renderer y la UI) ----------

    public List<PartEntry> GetItems(string slotId)
    {
        foreach (var s in slots)
            if (s.slotId == slotId) return s.items;
        return null;
    }

    public PartEntry FindById(string slotId, string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        var list = GetItems(slotId);
        return list?.Find(e => e.id == itemId);
    }

#if UNITY_EDITOR
    // ---------- Importador ----------

    private const string ART_ROOT = "Assets/EditorPersonajes/Art/";

    // Click derecho en el titulo del componente > "Importar desde carpetas"
    [ContextMenu("Importar desde carpetas")]
    public void ImportFromFolders()
    {
        if (config == null)
        {
            Debug.LogError("[PartLibrary] Asigna primero el CharacterSlotConfig en el campo Config.", this);
            return;
        }

        // Guardo los tintes que ya hayas editado a mano para no perderlos al reimportar
        var oldTints = new Dictionary<string, Color>();
        foreach (var s in slots)
            foreach (var e in s.items)
                oldTints[s.slotId + "/" + e.id] = e.tint;

        var newSlots = new List<SlotItems>();
        var sizeMismatches = new List<string>();
        Vector2? canvasSize = null;
        int totalItems = 0;
        int missingIcons = 0;

        foreach (var slot in config.AllSlots())
        {
            var result = new SlotItems { slotId = slot.id };
            newSlots.Add(result);

            string itemsFolder = ART_ROOT + slot.folder + "/Items";
            string iconsFolder = ART_ROOT + slot.folder + "/Icons";

            if (!AssetDatabase.IsValidFolder(itemsFolder))
            {
                Debug.LogWarning($"[PartLibrary] No existe la carpeta: {itemsFolder}", this);
                continue;
            }

            var sprites = LoadSpritesByName(itemsFolder);
            var icons = LoadSpritesByName(iconsFolder);

            foreach (var kv in sprites)
            {
                string id = kv.Key;
                Sprite sprite = kv.Value;

                icons.TryGetValue(id, out Sprite icon);
                if (icon == null) { icon = sprite; missingIcons++; }

                Color tint = oldTints.TryGetValue(slot.id + "/" + id, out Color t) ? t : Color.white;

                result.items.Add(new PartEntry { id = id, icon = icon, sprite = sprite, tint = tint });
                totalItems++;

                // Control de tamano de lienzo: todas las piezas deberian medir lo mismo
                Vector2 size = sprite.rect.size;
                if (canvasSize == null) canvasSize = size;
                else if (size != canvasSize.Value)
                    sizeMismatches.Add($"{slot.folder}/{id} ({size.x}x{size.y})");
            }
        }

        slots = newSlots;
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        // ---------- Resumen ----------
        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"[PartLibrary] Importadas {totalItems} piezas en {slots.Count} slots.");
        foreach (var s in slots)
            summary.AppendLine($"   {s.slotId}: {s.items.Count}");
        Debug.Log(summary.ToString(), this);

        if (missingIcons > 0)
            Debug.LogWarning($"[PartLibrary] {missingIcons} piezas no tienen miniatura en Icons/ (se usa la pieza completa como icono).", this);

        if (sizeMismatches.Count > 0)
        {
            int show = Mathf.Min(10, sizeMismatches.Count);
            Debug.LogWarning($"[PartLibrary] {sizeMismatches.Count} piezas NO miden {canvasSize.Value.x}x{canvasSize.Value.y} " +
                             $"y no van a encajar. Primeras {show}: {string.Join(", ", sizeMismatches.GetRange(0, show))}", this);
        }
    }

    // Devuelve nombre-de-archivo -> sprite, ordenado alfabeticamente (001, 002, 003...)
    private static SortedDictionary<string, Sprite> LoadSpritesByName(string folder)
    {
        var dict = new SortedDictionary<string, Sprite>(System.StringComparer.Ordinal);
        if (!AssetDatabase.IsValidFolder(folder)) return dict;

        foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;
            dict[Path.GetFileNameWithoutExtension(path)] = sprite;
        }
        return dict;
    }
#endif
}