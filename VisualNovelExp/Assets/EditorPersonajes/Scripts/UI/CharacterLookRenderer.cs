// CharacterLookRenderer.cs - PASO 5
// Pon en: Assets/_CharacterEditor/Scripts/UI/CharacterLookRenderer.cs
//
// Reemplaza (mas adelante) a CharacterRendererUI.
// Lee el CharacterSlotConfig y crea SOLO una Image por slot, ordenadas por sortOrder.
// No hay que armar ni asignar capas a mano en el Inspector.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLookRenderer : MonoBehaviour
{
    [Header("Datos")]
    public CharacterSlotConfig config;
    public CharacterPartLibrary library;

    private const string ROOT_NAME = "_Layers";

    private readonly Dictionary<string, Image> layers = new Dictionary<string, Image>();
    private CharacterLook currentLook;

    void Awake()
    {
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

        // Si ya habia un personaje mostrado, lo volvemos a aplicar
        if (currentLook != null) ApplyLook(currentLook);
    }

    // ---------- API publica ----------

    // Muestra un personaje completo
    public void ApplyLook(CharacterLook look)
    {
        currentLook = look;
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

        img.sprite = entry.sprite;
        img.color = entry.tint;
        img.enabled = true;
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