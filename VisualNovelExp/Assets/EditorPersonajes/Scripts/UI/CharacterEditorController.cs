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

    [Header("Color")]
    public ColorPaletteConfig paletas;
    public ColorGridPanel colorGrid;

    [Header("Nombre")]
    public TMPro.TMP_InputField inputNombre;
    public string nombrePorDefecto = "Kai";
    public int largoMaximo = 16;

    [Header("Confirmar reset")]
    public GameObject panelConfirmarReset;


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


    [Header("Editor simple: peinados armados")]
    [Tooltip("Slot cuyas piezas son íconos de peinados completos. Solo existe en la config de B")]
    public string slotPeinado = "hair_style";
    [Tooltip("Slots que se reemplazan al elegir un peinado")]
    public List<string> slotsDePelo = new List<string>
    {
        "hair_front", "hair_right_front", "hair_left_front", "hair_upper_back",
        "hair_lower_back", "hair_left_lower", "hair_right_lower"
    };
    [Tooltip("Qué piezas pone cada ícono. Los slots de pelo que no estén quedan en 'ninguno'")]
    public List<Peinado> peinados = new List<Peinado>();

    [System.Serializable]
    public class Peinado
    {
        [Tooltip("ID del ícono en el slot de peinado (ej: 001)")]
        public string itemId;
        public List<SlotSelection> piezas = new List<SlotSelection>();
    }

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
        if (panelConfirmarReset) panelConfirmarReset.SetActive(false);
        if (inputNombre)
        {
            inputNombre.characterLimit = largoMaximo;
            inputNombre.text = current.nombre ?? "";
            inputNombre.onEndEdit.AddListener(GuardarNombre);
        }

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

        if (EsSlotPeinado(slot.id))
            grid.Populate(items, false, PeinadoActual(), SelectPeinado);   // sin "ninguno"
        else
            grid.Populate(items, slot.canBeEmpty, current.GetItemId(slot.id), entry => SelectPart(slot, entry));

        MostrarPaleta(slot);
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

    // ---------- Peinados armados (editor simple) ----------

    private bool EsSlotPeinado(string slotId)
    {
        return !string.IsNullOrEmpty(slotPeinado) && peinados.Count > 0 && slotId == slotPeinado;
    }

    // Un clic en el ícono pone todas las piezas del peinado; el ícono no se guarda ni se dibuja
    private void SelectPeinado(PartEntry icono)
    {
        if (icono == null) return;
        var p = peinados.Find(x => x.itemId == icono.id);
        if (p == null)
        {
            Debug.LogWarning($"[EditorController] El ícono {icono.id} no tiene peinado cargado en la lista.", this);
            return;
        }

        foreach (string slotId in slotsDePelo)
        {
            var sel = p.piezas.Find(s => s.slotId == slotId);
            string itemId = sel != null ? sel.itemId : null;
            PartEntry pieza = string.IsNullOrEmpty(itemId) ? null : library.FindById(slotId, itemId);
            if (!string.IsNullOrEmpty(itemId) && pieza == null)
                Debug.LogWarning($"[EditorController] El peinado {p.itemId} usa {slotId}={itemId}, que no existe.", this);

            current.SetItemId(slotId, pieza?.id);
            lookRenderer.SetPart(slotId, pieza);
        }

        // Un solo registro por clic: es una decisión, no siete
        ResearchAnalytics.LogEditorInteraction(slotPeinado, icono.id);
        EventosJuego.Emitir("editor_seleccion", slotPeinado + ":" + icono.id);
    }

    // ID del ícono cuyo peinado coincide con el personaje actual (null si ninguno)
    private string PeinadoActual()
    {
        foreach (var p in peinados)
        {
            bool igual = true;
            foreach (string slotId in slotsDePelo)
            {
                var sel = p.piezas.Find(s => s.slotId == slotId);
                string esperado = sel != null ? sel.itemId ?? "" : "";
                string actual = current.GetItemId(slotId) ?? "";
                if (esperado != actual) { igual = false; break; }
            }
            if (igual) return p.itemId;
        }
        return null;
    }

    // Muestra la grilla de color si esta subcategoría tiene paleta; si no, la oculta
    private void MostrarPaleta(SlotDefinition slot)
    {
        if (colorGrid == null) return;
        var grupo = paletas != null ? paletas.GrupoQueAparece(slot.id) : null;
        bool hay = grupo != null && grupo.colores.Count > 0;
        colorGrid.gameObject.SetActive(hay);
        if (!hay) return;

        int actual = current.GetColorIndex(grupo.id);
        if (actual < 0) actual = grupo.colorPorDefecto;
        colorGrid.Populate(grupo, actual, i => SelectColor(grupo, i));
    }

    private void SelectColor(GrupoColor grupo, int indice)
    {
        current.SetColorIndex(grupo.id, indice);
        lookRenderer.ApplyLook(current);
        ResearchAnalytics.LogEditorInteraction("color_" + grupo.id, indice.ToString());
        EventosJuego.Emitir("editor_color", grupo.id + ":" + indice);
    }

    // ---------- Botones de la escena ----------

    public void ResetCharacter()
    {
        string nombre = current.nombre;
        current = CreateDefaultLook();
        current.nombre = nombre;               // el reset no borra el nombre
        lookRenderer.ApplyLook(current);
        if (currentSlot != null) OpenSlot(currentSlot);
        ResearchAnalytics.LogEditorInteraction("reset", "reset_character");
        EventosJuego.Emitir("editor_reset");
    }

    // Botón Reset: primero pregunta
    public void PedirReset()
    {
        if (panelConfirmarReset) panelConfirmarReset.SetActive(true);
        else ResetCharacter();
    }

    public void ConfirmarReset()
    {
        if (panelConfirmarReset) panelConfirmarReset.SetActive(false);
        ResetCharacter();
    }

    public void CancelarReset()
    {
        if (panelConfirmarReset) panelConfirmarReset.SetActive(false);
        ResearchAnalytics.LogEditorInteraction("reset", "cancelado");
        EventosJuego.Emitir("editor_reset_cancelado");
    }

    private void GuardarNombre(string texto)
    {
        current.nombre = texto.Trim();
        // Se registra solo el largo, no el nombre: puede ser el nombre real del participante
        EventosJuego.Emitir("editor_nombre", current.nombre.Length.ToString());
    }

    public void ConfirmAndSave()
    {
        ResearchAnalytics.LogEditorClose();
        EventosJuego.Emitir("editor_fin", $"piezas{current.selections.Count}:colores{current.colores.Count}");

        if (inputNombre) current.nombre = inputNombre.text.Trim();
        if (string.IsNullOrEmpty(current.nombre)) current.nombre = nombrePorDefecto;
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
            if (EsSlotPeinado(slot.id)) continue;
            if (slot.canBeEmpty || look.HasItem(slot.id)) continue;

            var items = library.GetItems(slot.id);
            if (items != null && items.Count > 0) look.SetItemId(slot.id, items[0].id);
        }
    }
}