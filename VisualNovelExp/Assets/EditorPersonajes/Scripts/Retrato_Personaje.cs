using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterLookRenderer))]
public class Retrato_Personaje : MonoBehaviour
{
    [Tooltip("Los mismos Default Slot Ids del CharacterEditorController")]
    public List<string> slotsPorDefecto = new List<string>
    {
        "skin_body",
        "hair_front", "hair_upper_back", "hair_lower_back",
        "face_ears", "face_mouth", "face_eye_shape", "face_pupil",
        "clothes_shirt"
    };

    void Start()
    {
        var r = GetComponent<CharacterLookRenderer>();
        if (r.config == null || r.library == null)
        {
            Debug.LogError("[Retrato] Falta Config o Library en el CharacterLookRenderer.", this);
            return;
        }

        CharacterLook look;
        if (CharacterLookSaveSystem.HasSavedLook())
        {
            look = CharacterLookSaveSystem.Load();
            look.RemoveInvalid(r.library);
        }
        else
        {
            look = CrearPredeterminado(r);
        }
        r.ApplyLook(look);
    }

    CharacterLook CrearPredeterminado(CharacterLookRenderer r)
    {
        var look = new CharacterLook();
        foreach (var slot in r.config.AllSlots())
        {
            if (!slotsPorDefecto.Contains(slot.id) && slot.canBeEmpty) continue;
            var items = r.library.GetItems(slot.id);
            if (items != null && items.Count > 0) look.SetItemId(slot.id, items[0].id);
        }
        return look;
    }
}