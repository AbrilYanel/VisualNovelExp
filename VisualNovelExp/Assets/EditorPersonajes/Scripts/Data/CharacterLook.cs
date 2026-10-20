// CharacterLook.cs - PASO 4
// Pon en: Assets/_CharacterEditor/Scripts/Data/CharacterLook.cs
//
// Reemplaza (mas adelante) a CharacterSaveData.
// En vez de campos fijos (skinIndex, hairIndex...) guarda una lista "slot -> pieza elegida".
// Si un slot NO aparece en la lista = ese slot esta en "ninguno".

using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class SlotSelection
{
    public string slotId; // ej: hair_front
    public string itemId; // ej: 003 (nombre del archivo)
}

[System.Serializable]
public class CharacterLook
{
    public List<SlotSelection> selections = new List<SlotSelection>();

    public string GetItemId(string slotId)
    {
        foreach (var s in selections)
            if (s.slotId == slotId) return s.itemId;
        return null;
    }

    public bool HasItem(string slotId) => !string.IsNullOrEmpty(GetItemId(slotId));

    // itemId vacio o null = "ninguno" (se quita la seleccion de ese slot)
    public void SetItemId(string slotId, string itemId)
    {
        selections.RemoveAll(s => s.slotId == slotId);
        if (!string.IsNullOrEmpty(itemId))
            selections.Add(new SlotSelection { slotId = slotId, itemId = itemId });
    }

    // Quita selecciones de piezas que ya no existen en la biblioteca
    // (por ejemplo si borraste un PNG despues de que alguien guardo su personaje).
    // Devuelve cuantas se quitaron.
    public int RemoveInvalid(CharacterPartLibrary library)
    {
        return selections.RemoveAll(s => library.FindById(s.slotId, s.itemId) == null);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var s in selections) sb.Append($"{s.slotId}={s.itemId} ");
        return sb.ToString().Trim();
    }
}