using System.Collections.Generic;
using UnityEngine;

public class CharacterLookLoader : MonoBehaviour
{
    public CharacterLookRenderer lookRenderer;

    [Tooltip("Paletas de todas las condiciones (ej: ColorPaletteConfig y ColorPaletteConfig_B). " +
             "Se elige la que tenga el mismo nombre que la guardada. Si no hay coincidencia, " +
             "el renderer conserva la paleta que tiene asignada.")]
    public List<ColorPaletteConfig> paletasPorCondicion = new List<ColorPaletteConfig>();

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
        AsignarPaleta(look);
        lookRenderer.ApplyLook(look);
    }

    private void AsignarPaleta(CharacterLook look)
    {
        if (string.IsNullOrEmpty(look.paleta)) return;   // guardado viejo o sin editor: no se toca

        var paleta = paletasPorCondicion.Find(p => p != null && p.name == look.paleta);
        if (paleta != null)
            lookRenderer.paletas = paleta;
        else
            Debug.LogWarning($"[LookLoader] La paleta '{look.paleta}' no está en Paletas Por Condicion.", this);
    }
}