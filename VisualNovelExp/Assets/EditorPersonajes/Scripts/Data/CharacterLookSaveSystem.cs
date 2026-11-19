// CharacterLookSaveSystem.cs - PASO 4
// Pon en: Assets/_CharacterEditor/Scripts/Data/CharacterLookSaveSystem.cs
//
// Misma idea que CharacterSaveSystem (PlayerPrefs + JSON) pero con otra clave,
// asi no se mezcla con el guardado viejo.

using UnityEngine;

public static class CharacterLookSaveSystem
{
    private const string KEY = "CharacterLook_v2";
    private const string LEGACY_KEY = "CharacterSaveData_v1"; // guardado del sistema viejo

    public static void Save(CharacterLook look)
    {
        string json = JsonUtility.ToJson(look);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[LookSave] Guardado: {json}");
    }

    public static CharacterLook Load()
    {
        if (!PlayerPrefs.HasKey(KEY))
        {
            Debug.Log("[LookSave] No hay guardado previo, devolviendo personaje vacio");
            return new CharacterLook();
        }

        try
        {
            string json = PlayerPrefs.GetString(KEY);
            var look = JsonUtility.FromJson<CharacterLook>(json);
            if (look == null) return new CharacterLook();
            if (look.selections == null) look.selections = new System.Collections.Generic.List<SlotSelection>();
            if (look.colores == null) look.colores = new System.Collections.Generic.List<ColorSelection>();
            Debug.Log($"[LookSave] Cargado: {json}");
            return look;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LookSave] Guardado ilegible, se usa uno vacio. {e.Message}");
            return new CharacterLook();
        }
    }

    public static bool HasSavedLook() => PlayerPrefs.HasKey(KEY);

    public static void DeleteSave()
    {
        if (!PlayerPrefs.HasKey(KEY)) return;
        PlayerPrefs.DeleteKey(KEY);
        PlayerPrefs.Save();
        Debug.Log("[LookSave] Save borrado");
    }

    // Borra el guardado del sistema viejo (indices que ya no significan nada).
    // Lo vamos a llamar una vez cuando terminemos la migracion.
    public static void DeleteLegacySave()
    {
        if (!PlayerPrefs.HasKey(LEGACY_KEY)) return;
        PlayerPrefs.DeleteKey(LEGACY_KEY);
        PlayerPrefs.Save();
    }
}