// CharacterEditorController.cs - PASO 6
// Pon en: Assets/_CharacterEditor/Scripts/UI/CharacterEditorController.cs
//
// Reemplaza a CharacterCustomizer.
// Categorias (arriba) > subcategorias (abajo) > grilla de piezas.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterEditorController : MonoBehaviour
{
    [Header("Datos")]
    public CharacterSlotConfig config;
    public CharacterPartLibrary library;

    [Header("Escena")]
    public CharacterLookRenderer lookRenderer;
    public TabBar categoryBar;   // Skin, Hair, Face...
    public TabBar slotBar;       // Front, Upper back, Ears...
    public PartGridPanel grid;   // grilla de piezas
    public string nextSceneName = "MainScene";

    [Header("Personaje inicial (al empezar sin guardado y al resetear)")]
    [Tooltip("Slots que arrancan con su primera pieza. El resto arranca en 'ninguno'.")]
    public List<string> defaultSlotIds = new List<string>
    {
        "skin_body",
        "hair_front", "hair_upper_back", "hair_lower_back",
        "face_ears", "face_mouth", "face_eye_shape", "face_pupil",
        "clothes_shirt"
    };

    private CharacterLook current = new CharacterLook();
    private SlotDefinition currentSlot;

    // ---------- Arranque ----------

    void Start()
    {
        if (config == null || library == null || lookRenderer == null ||
            categoryBar == null || slotBar == null || grid == null)
        {
            Debug.LogError("[EditorController] Faltan referencias en el Inspector.", this);
            return;
        }

        ResearchAnalytics.LogEditorOpen();
        EventosJuego.Emitir("editor_inicio");

        if (CharacterLookSaveSystem.HasSavedLook())
        {
            current = CharacterLookSaveSystem.Load();
            current.RemoveInvalid(library);
            EnsureRequiredSlots(current);
        }
        else
        {
            current = CreateDefaultLook();
        }

        lookRenderer.ApplyLook(current);

        categoryBar.Populate(
            config.categories.Select(c => c.displayName).ToList(),
            OpenCategory,
            0);
        OpenCategory(0);
    }

    // ---------- Navegacion ----------

    public void OpenCategory(int index)
    {
        if (index < 0 || index >= config.categories.Count) return;

        var slots = config.categories[index].slots;
        if (slots.Count == 0) return;

        // Primero activar la barra, despues llenarla, y recien ahi ocultarla si no hace falta
        slotBar.gameObject.SetActive(true);
        slotBar.Populate(
            slots.Select(s => s.displayName).ToList(),
            i => OpenSlot(slots[i]),
            0);

        OpenSlot(slots[0]);

        // Una categoria con un solo slot (ej: Skin) no necesita barra de subcategorias
        if (slots.Count <= 1) slotBar.gameObject.SetActive(false);
    }

    private void OpenSlot(SlotDefinition slot)
    {
        currentSlot = slot;
        var items = library.GetItems(slot.id);
        grid.Populate(items, slot.canBeEmpty, current.GetItemId(slot.id), entry => SelectPart(slot, entry));
    }

    // ---------- Seleccion ----------

    // entry == null significa "ninguno"
    private void SelectPart(SlotDefinition slot, PartEntry entry)
    {
        current.SetItemId(slot.id, entry?.id);
        lookRenderer.SetPart(slot.id, entry);
        ResearchAnalytics.LogEditorInteraction(slot.id, entry != null ? entry.id : "none");
        EventosJuego.Emitir("editor_seleccion", slot.id + ":" + (entry != null ? entry.id : "none"));
    }

    // ---------- Botones de la escena (conectalos en el OnClick) ----------

    public void ResetCharacter()
    {
        current = CreateDefaultLook();
        lookRenderer.ApplyLook(current);
        if (currentSlot != null) OpenSlot(currentSlot); // refresca el resaltado
        ResearchAnalytics.LogEditorInteraction("reset", "reset_character");
    }

    public void ConfirmAndSave()
    {
        ResearchAnalytics.LogEditorClose();
        EventosJuego.Emitir("editor_fin", current.selections.Count.ToString());

        CharacterLookSaveSystem.Save(current);
        CharacterLookSaveSystem.DeleteLegacySave();
        Debug.Log($"Guardado: {current}");

        if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public CharacterLook GetCurrentLook() => current;

    // ---------- Auxiliares ----------

    private CharacterLook CreateDefaultLook()
    {
        var look = new CharacterLook();
        foreach (string slotId in defaultSlotIds)
        {
            var items = library.GetItems(slotId);
            if (items != null && items.Count > 0) look.SetItemId(slotId, items[0].id);
        }
        EnsureRequiredSlots(look);
        return look;
    }

    // Los slots que no admiten "ninguno" (como skin) siempre tienen que tener una pieza
    private void EnsureRequiredSlots(CharacterLook look)
    {
        foreach (var slot in config.AllSlots())
        {
            if (slot.canBeEmpty || look.HasItem(slot.id)) continue;

            var items = library.GetItems(slot.id);
            if (items != null && items.Count > 0) look.SetItemId(slot.id, items[0].id);
        }
    }
}