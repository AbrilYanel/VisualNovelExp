using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrupoColor
{
    [Tooltip("ID único, se usa en el guardado: no lo cambies después")]
    public string id;
    [Tooltip("Título que se muestra sobre la grilla de colores")]
    public string nombre;
    [Tooltip("Slots que se tiñen con esta paleta")]
    public List<string> slotsQueTine = new List<string>();
    [Tooltip("Subcategorías en las que aparece la grilla de esta paleta")]
    public List<string> slotsDondeAparece = new List<string>();
    public List<Color> colores = new List<Color>();
    [Tooltip("Índice del color inicial (antes de que el jugador elija)")]
    public int colorPorDefecto = 0;
    [Tooltip("Opcional: material propio (por ejemplo, con más brillo para los ojos)")]
    public Material materialPropio;
}

[CreateAssetMenu(fileName = "ColorPaletteConfig", menuName = "Character/Color Palette Config")]
public class ColorPaletteConfig : ScriptableObject
{
    public List<GrupoColor> grupos = new List<GrupoColor>();

    public GrupoColor GrupoQueTine(string slotId) => grupos.Find(g => g.slotsQueTine.Contains(slotId));
    public GrupoColor GrupoQueAparece(string slotId) => grupos.Find(g => g.slotsDondeAparece.Contains(slotId));

    // Clic derecho en el título del componente > "Cargar paletas por defecto (condición alta)"
    [ContextMenu("Cargar paletas por defecto (condición alta)")]
    public void CargarPorDefecto()
    {
        string[] general =
        {
            "#FFFFFF", "#D9D9D9", "#8C8C8C", "#3A3A3A", "#1A1A1A",           // 0-4 blanco a negro
            "#F2E1B3", "#D9A441", "#A0622D", "#6B3A1E", "#3B2314",           // 5-9 rubios y castaños
            "#E53935", "#FB8C00", "#FDD835", "#7CB342", "#2E7D32", "#00ACC1", // 10-15
            "#1E88E5", "#283593", "#8E24AA", "#D81B60", "#F48FB1", "#B39DDB", // 16-21
            "#80DEEA", "#A5D6A7"                                              // 22-23 celeste, menta
        };
        string[] piel =
        {
            "#FFF1E6", "#FDE3D0", "#F9D5BE", "#F3C6A5", "#EBB894", "#E2A982",
            "#D69A73", "#C88B63", "#B97B56", "#A86B4A", "#965D40", "#845036",
            "#72432D", "#603825", "#4E2D1E", "#3F2418",                      // 0-15 naturales
            "#FFE0EC", "#E6E0FF", "#D5F5E3", "#CDE7FF", "#B8B8B8", "#9FD8C8",
            "#C9A0DC", "#FFD8A8"                                              // 16-23 fantasía
        };
        string[] pelo =
        {
            "hair_front", "hair_right_front", "hair_left_front", "hair_upper_back",
            "hair_lower_back", "hair_left_lower", "hair_right_lower"
        };

        grupos = new List<GrupoColor>
        {
            Grupo("skin",    "Tono de piel",                  piel,    1,  new[] { "skin_body", "face_ears" }, new[] { "skin_body" }),
            Grupo("hair",    "Color de pelo",                 general, 22, pelo, pelo),
            Grupo("eyes",    "Color de ojos",                 general, 10, new[] { "face_pupil" },     new[] { "face_pupil" }),
            Grupo("shirt",   "Color de camisa",               general, 0,  new[] { "clothes_shirt" },  new[] { "clothes_shirt" }),
            Grupo("shirt2",  "Color de camisa 2",             general, 0,  new[] { "clothes_shirt2" }, new[] { "clothes_shirt2" }),
            Grupo("jacket",  "Color de chaqueta",             general, 0,  new[] { "clothes_jacket" }, new[] { "clothes_jacket" }),
            Grupo("head",    "Color de accesorio de cabeza",  general, 0,  new[] { "acc_head" },       new[] { "acc_head" }),
            Grupo("collar",  "Color de collar",               general, 0,  new[] { "acc_collar" },     new[] { "acc_collar" }),
            Grupo("glasses", "Color de anteojos",             general, 0,  new[] { "acc_glasses" },    new[] { "acc_glasses" }),
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    static GrupoColor Grupo(string id, string nombre, string[] hex, int porDefecto, string[] tine, string[] aparece)
    {
        var g = new GrupoColor
        {
            id = id,
            nombre = nombre,
            colorPorDefecto = porDefecto,
            slotsQueTine = new List<string>(tine),
            slotsDondeAparece = new List<string>(aparece)
        };
        foreach (var h in hex)
            if (ColorUtility.TryParseHtmlString(h, out Color c)) g.colores.Add(c);
        return g;
    }

    // Avisa si un slot quedó en dos paletas a la vez
    void OnValidate()
    {
        var vistos = new HashSet<string>();
        foreach (var g in grupos)
            foreach (var s in g.slotsQueTine)
                if (!vistos.Add(s)) Debug.LogWarning($"[ColorPaletteConfig] El slot {s} está en dos paletas", this);
    }
}