// CharacterLookLoader.cs - PASO 6
// Pon en: Assets/_CharacterEditor/Scripts/UI/CharacterLookLoader.cs
//
// Reemplaza a CharacterLoaderUI. Va en la escena del JUEGO, junto a un CharacterLookRenderer.

using UnityEngine;

public class CharacterLookLoader : MonoBehaviour
{
    public CharacterLookRenderer lookRenderer;

    void Start()
    {
        LoadCharacter();

        // RESEARCH: Log cuando aparece la pantalla de juego
        ResearchAnalytics.LogGameplayScreenOn();
    }

    public void LoadCharacter()
    {
        if (lookRenderer == null || lookRenderer.library == null)
        {
            Debug.LogError("[LookLoader] Falta el CharacterLookRenderer (con su Library asignada).", this);
            return;
        }

        CharacterLook look = CharacterLookSaveSystem.Load();
        look.RemoveInvalid(lookRenderer.library);
        lookRenderer.ApplyLook(look);
    }
}