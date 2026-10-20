// PartGridPanel.cs - PASO 6
// Pon en: Assets/_CharacterEditor/Scripts/UI/PartGridPanel.cs
//
// Reemplaza a CategoryPanel. Muestra la grilla de piezas del slot elegido,
// con boton "ninguno" opcional y la pieza actual resaltada.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartGridPanel : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("El Content del ScrollView con Grid Layout Group")]
    public Transform buttonContainer;
    [Tooltip("Prefab del boton. Si tiene un hijo llamado 'Icon' con Image, ahi pone la miniatura")]
    public GameObject buttonPrefab;

    [Header("Boton 'ninguno'")]
    [Tooltip("Opcional: sprite (una X, por ejemplo). Si esta vacio se usa el texto de abajo")]
    public Sprite noneIcon;
    public string noneLabel = "X";

    [Header("Feedback visual")]
    public Color selectedColor = new Color(1f, 0.8f, 0.2f);
    public Color normalColor = Color.white;

    private readonly List<Button> buttons = new List<Button>();

    // items: piezas del slot | includeNone: mostrar boton "ninguno"
    // selectedId: id de la pieza actual (vacio = ninguna) | onSelect: recibe la pieza, o null si eligio "ninguno"
    public void Populate(List<PartEntry> items, bool includeNone, string selectedId, Action<PartEntry> onSelect)
    {
        if (buttonContainer == null || buttonPrefab == null)
        {
            Debug.LogError("[PartGridPanel] Faltan Button Container o Button Prefab.", this);
            return;
        }

        UiUtil.ClearChildren(buttonContainer);
        buttons.Clear();

        if (includeNone)
        {
            var noneButton = AddButton("Btn_none", noneIcon, noneLabel, () => onSelect?.Invoke(null));
            UiUtil.SetSelected(noneButton, string.IsNullOrEmpty(selectedId), selectedColor, normalColor);
        }

        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("[PartGridPanel] Este slot no tiene piezas importadas.", this);
            return;
        }

        foreach (var item in items)
        {
            if (item == null) continue;

            var captured = item; // copia local para el closure
            Sprite icon = item.icon != null ? item.icon : item.sprite;

            var button = AddButton("Btn_" + item.id, icon, null, () => onSelect?.Invoke(captured));
            UiUtil.SetSelected(button, item.id == selectedId, selectedColor, normalColor);
        }
    }

    private Button AddButton(string objectName, Sprite icon, string label, Action onClick)
    {
        var go = Instantiate(buttonPrefab, buttonContainer);
        go.name = objectName;

        var button = go.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[PartGridPanel] El prefab no tiene componente Button.", this);
            return null;
        }

        // Buscar la Image del icono: primero un hijo llamado "Icon", si no la segunda Image, si no la unica
        var rootImage = go.GetComponent<Image>();
        Image iconImage = null;

        var iconTransform = go.transform.Find("Icon");
        if (iconTransform != null) iconImage = iconTransform.GetComponent<Image>();

        if (iconImage == null)
        {
            var images = go.GetComponentsInChildren<Image>(true);
            if (images.Length > 1) iconImage = images[1];
            else if (images.Length == 1) iconImage = images[0];
        }

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
                iconImage.enabled = true;
            }
            else if (iconImage != rootImage)
            {
                iconImage.enabled = false; // sin sprite y enabled = cuadro blanco
            }
        }

        if (icon == null && label != null) UiUtil.SetLabel(go, label);

        buttons.Add(button);
        button.onClick.AddListener(() =>
        {
            Highlight(button);
            onClick?.Invoke();
        });

        return button;
    }

    private void Highlight(Button clicked)
    {
        foreach (var b in buttons)
            UiUtil.SetSelected(b, b == clicked, selectedColor, normalColor);
    }
}