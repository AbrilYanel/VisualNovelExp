// CharacterSaveSystem.cs - PASO 4
// Pon en: Assets/_CharacterEditor/Scripts/Data/CharacterSaveSystem.cs
// Sistema simple con PlayerPrefs + JSON (suficiente para tu escala)
// Si en el futuro necesitás guardar más cosas, cambiá a Application.persistentDataPath

using UnityEngine;

public static class CharacterSaveSystem
{
    private const string KEY = "CharacterSaveData_v1";

    public static void Save(CharacterSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[SaveSystem] Guardado: {json}");
    }

    public static CharacterSaveData Load()
    {
        if (!PlayerPrefs.HasKey(KEY))
        {
            Debug.Log("[SaveSystem] No hay guardado previo, devolviendo default");
            return new CharacterSaveData();
        }

        string json = PlayerPrefs.GetString(KEY);
        CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);
        Debug.Log($"[SaveSystem] Cargado: {json}");
        return data;
    }

    public static bool HasSavedCharacter()
    {
        return PlayerPrefs.HasKey(KEY);
    }

    public static void DeleteSave()
    {
        if (PlayerPrefs.HasKey(KEY))
        {
            PlayerPrefs.DeleteKey(KEY);
            PlayerPrefs.Save();
            Debug.Log("[SaveSystem] Save borrado");
        }
    }

    // Opcional: Guardado avanzado en archivo (descomenta si lo necesitás)
    /*
    public static void SaveToFile(CharacterSaveData data)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "character.json");
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(path, json);
        Debug.Log($"Guardado en archivo: {path}");
    }
    */
}
