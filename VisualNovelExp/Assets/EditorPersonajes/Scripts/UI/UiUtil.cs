// UiUtil.cs - PASO 6
// Pon en: Assets/_CharacterEditor/Scripts/UI/UiUtil.cs
//
// Funciones chicas que usan TabBar y PartGridPanel.

using UnityEngine;
using UnityEngine.UI;

public static class UiUtil
{
    // Borra todos los hijos (los desactiva primero para que el layout se actualice ya)
    public static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            UnityEngine.Object.Destroy(child);
        }
    }

    // Pone texto en el boton. Funciona con TextMeshPro o con Text clasico.
    // Devuelve false si el boton no tiene ningun texto.
    public static bool SetLabel(GameObject go, string text)
    {
        var tmp = go.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null) { tmp.text = text; return true; }

        var legacy = go.GetComponentInChildren<Text>(true);
        if (legacy != null) { legacy.text = text; return true; }

        return false;
    }

    // Marca un boton como seleccionado / no seleccionado cambiando su color
    public static void SetSelected(Button button, bool selected, Color selectedColor, Color normalColor)
    {
        if (button == null) return;
        var colors = button.colors;
        Color c = selected ? selectedColor : normalColor;
        colors.normalColor = c;
        colors.selectedColor = c; // evita el gris que Unity pone al hacer click
        button.colors = colors;
    }
}