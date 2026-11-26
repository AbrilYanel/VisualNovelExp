using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLookRenderer : MonoBehaviour
{
    [Header("Datos")]
    public CharacterSlotConfig config;
    public CharacterPartLibrary library;

    [Header("Color")]
    [Tooltip("Paleta del editor de alta complejidad (condición A)")]
    public ColorPaletteConfig paletaAlta;
    [Tooltip("Paleta del editor de baja complejidad (condición B)")]
    public ColorPaletteConfig paletaBaja;
    [Tooltip("Versión que se usa si el personaje no indica ninguna (ej.: probar la escena sin pasar por el editor)")]
    public VersionEditor versionPorDefecto = VersionEditor.Alta;
    [Tooltip("Mat_TintableGris: se usa en las capas que tienen grupo de color, salvo que la paleta lo desactive")]
    public Material materialTenible;

    // Paleta en uso (la decide la versión). Solo lectura desde otros scripts.
    public ColorPaletteConfig paletas { get; private set; }

    public ColorPaletteConfig PaletaDe(VersionEditor version) =>
        version == VersionEditor.Alta ? paletaAlta : paletaBaja;

    [Header("Personaje por defecto (solo si no hay guardado)")]
    [Tooltip("Los mismos Default Slot Ids del CharacterEditorController")]
    public List<string> slotsPorDefecto = new List<string>
    {
        "skin_body",
        "hair_front", "hair_upper_back", "hair_lower_back",
        "face_ears", "face_mouth", "face_eye_shape", "face_pupil",
        "clothes_shirt"
    };

    private const string ROOT_NAME = "_Layers";

    private readonly Dictionary<string, Image> layers = new Dictionary<string, Image>();
    private readonly Dictionary<string, GrupoColor> grupoPorSlot = new Dictionary<string, GrupoColor>();
    private ColorPaletteConfig paletaAplicada;   // paleta con la que se calcularon los materiales
    private bool materialesListos;
    private CharacterLook currentLook;

    void Awake()
    {
        paletas = PaletaDe(versionPorDefecto);
        Build();
    }

    // ---------- Construccion de capas ----------

    // Tambien sirve desde el Inspector (click derecho en el componente, en Play mode)
    // para ver el resultado despues de cambiar un sortOrder en el Slot Config.
    [ContextMenu("Reconstruir capas")]
    public void Build()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[LookRenderer] Las capas se construyen en Play mode.", this);
            return;
        }
        if (config == null)
        {
            Debug.LogError("[LookRenderer] Falta asignar el CharacterSlotConfig.", this);
            return;
        }

        // Borrar capas anteriores
        var old = transform.Find(ROOT_NAME);
        if (old != null)
        {
            old.name = ROOT_NAME + "_old";
            old.gameObject.SetActive(false);
            Destroy(old.gameObject);
        }
        layers.Clear();

        // Contenedor
        var root = new GameObject(ROOT_NAME, typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(transform, false);
        Stretch(root);

        // Una Image por slot, de atras hacia adelante (el primer hijo se dibuja detras)
        foreach (var slot in config.AllSlots().OrderBy(s => s.sortOrder))
        {
            var go = new GameObject(slot.id, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            Stretch(rt);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.enabled = false; // sin sprite y enabled = cuadro blanco, asi que arranca apagada
            layers[slot.id] = img;
        }

        materialesListos = false;
        AplicarMateriales();

        // Si ya habia un personaje mostrado, lo volvemos a aplicar
        if (currentLook != null) ApplyLook(currentLook);
    }

    // ---------- Paleta y materiales ----------

    // Elige la paleta según la versión del personaje y, si cambió, recalcula materiales
    private void ElegirPaleta(CharacterLook look)
    {
        VersionEditor version = versionPorDefecto;
        if (look != null && System.Enum.TryParse(look.version, out VersionEditor guardada))
            version = guardada;

        var nueva = PaletaDe(version);
        if (nueva == null)
            Debug.LogWarning($"[LookRenderer] Falta asignar la Paleta {version} en este renderer.", this);

        paletas = nueva;
        if (!materialesListos || paletas != paletaAplicada) AplicarMateriales();
    }

    // Pone en cada capa el material de su grupo de color (o ninguno) y guarda el grupo de cada slot
    private void AplicarMateriales()
    {
        grupoPorSlot.Clear();
        bool usarTenible = paletas == null || paletas.usarMaterialTenible;

        foreach (var kv in layers)
        {
            var grupo = paletas != null ? paletas.GrupoQueTine(kv.Key) : null;
            if (grupo != null && grupo.colores.Count > 0) grupoPorSlot[kv.Key] = grupo;

            Material mat = null;
            if (grupo != null)
                mat = grupo.materialPropio != null ? grupo.materialPropio : (usarTenible ? materialTenible : null);
            kv.Value.material = mat;   // null = material por defecto de la UI (colores originales del PNG)
        }

        paletaAplicada = paletas;
        materialesListos = true;
    }

    // ---------- API publica ----------

    // Carga el personaje guardado (o el predeterminado si no hay guardado) y lo dibuja.
    // La usan los retratos de Casa y Ciudad.
    public void CargarGuardado()
    {
        if (config == null || library == null)
        {
            Debug.LogError("[LookRenderer] Falta Config o Library.", this);
            return;
        }

        CharacterLook look;
        if (CharacterLookSaveSystem.HasSavedLook())
        {
            look = CharacterLookSaveSystem.Load();
            look.RemoveInvalid(library);
        }
        else
        {
            look = CrearPredeterminado();
        }
        ApplyLook(look);
    }

    // Muestra un personaje completo
    public void ApplyLook(CharacterLook look)
    {
        currentLook = look;
        ElegirPaleta(look);
        foreach (var slot in config.AllSlots())
        {
            string itemId = look?.GetItemId(slot.id);
            SetPart(slot.id, library != null ? library.FindById(slot.id, itemId) : null);
        }
    }

    // Cambia una sola pieza (entry null = ninguno)
    public void SetPart(string slotId, PartEntry entry)
    {
        if (!layers.TryGetValue(slotId, out Image img))
        {
            Debug.LogWarning($"[LookRenderer] No existe la capa del slot '{slotId}'", this);
            return;
        }

        if (entry == null || entry.sprite == null)
        {
            img.enabled = false;
            img.sprite = null;
            return;
        }

        if (!materialesListos) AplicarMateriales();
        img.sprite = entry.sprite;
        img.color = ColorPara(slotId, entry);
        img.enabled = true;
    }

    // Color de la paleta si el slot tiene un grupo; si no, el tinte de la pieza
    private Color ColorPara(string slotId, PartEntry entry)
    {
        if (!grupoPorSlot.TryGetValue(slotId, out GrupoColor grupo)) return entry.tint;

        int i = currentLook != null ? currentLook.GetColorIndex(grupo.id) : -1;
        if (i < 0 || i >= grupo.colores.Count)
            i = Mathf.Clamp(grupo.colorPorDefecto, 0, grupo.colores.Count - 1);
        return grupo.colores[i];
    }

    public void ClearSlot(string slotId) => SetPart(slotId, null);

    [ContextMenu("Limpiar todo")]
    public void ClearAll()
    {
        currentLook = null;
        foreach (var img in layers.Values)
        {
            img.enabled = false;
            img.sprite = null;
        }
    }

    // ---------- Auxiliares ----------

    private CharacterLook CrearPredeterminado()
    {
        var look = new CharacterLook();
        foreach (var slot in config.AllSlots())
        {
            if (!slotsPorDefecto.Contains(slot.id) && slot.canBeEmpty) continue;
            var items = library.GetItems(slot.id);
            if (items != null && items.Count > 0) look.SetItemId(slot.id, items[0].id);
        }
        return look;
    }

    // ---------- Prueba rapida ----------

    // Click derecho en el componente (en Play mode): pone la primera pieza de cada slot
    // para comprobar que todo encaja y el orden de apilado es correcto.
    [ContextMenu("TEST - primera pieza de cada slot")]
    public void TestFirstOfEach()
    {
        if (!Application.isPlaying || config == null || library == null)
        {
            Debug.LogWarning("[LookRenderer] Usalo en Play mode con Config y Library asignados.", this);
            return;
        }

        var look = new CharacterLook();
        foreach (var slot in config.AllSlots())
        {
            var items = library.GetItems(slot.id);
            if (items != null && items.Count > 0) look.SetItemId(slot.id, items[0].id);
        }
        ApplyLook(look);
    }

    // ---------- Utilidad ----------

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}