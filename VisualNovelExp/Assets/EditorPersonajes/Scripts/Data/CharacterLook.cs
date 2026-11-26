using System.Collections.Generic;
using System.Text;

// Versión del editor de personajes (condición experimental)
public enum VersionEditor { Alta, Baja }

[System.Serializable]
public class SlotSelection
{
    public string slotId; // ej: hair_front
    public string itemId; // ej: 003 (nombre del archivo)
}

[System.Serializable]
public class ColorSelection
{
    public string grupoId; // ej: hair
    public int indice;     // posición en la paleta
}

[System.Serializable]
public class CharacterLook
{
    public List<SlotSelection> selections = new List<SlotSelection>();
    public List<ColorSelection> colores = new List<ColorSelection>();
    public string nombre = "";
    // Versión del editor que creó al personaje ("Alta" o "Baja").
    // CharacterLookRenderer la usa para elegir la paleta en el juego.
    public string version = "";

    public int GetColorIndex(string grupoId)
    {
        foreach (var c in colores)
            if (c.grupoId == grupoId) return c.indice;
        return -1; // -1 = usar el color por defecto
    }

    public void SetColorIndex(string grupoId, int indice)
    {
        colores.RemoveAll(c => c.grupoId == grupoId);
        if (indice >= 0) colores.Add(new ColorSelection { grupoId = grupoId, indice = indice });
    }

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