// TabBar.cs - PASO 6
// Pon en: Assets/_CharacterEditor/Scripts/UI/TabBar.cs
//
// Fila de botones de texto. Se usa dos veces en la escena:
//   - una para las CATEGORIAS (Skin, Hair, Face...)
//   - otra para las SUBCATEGORIAS (Front, Upper back, Ears...)

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabBar : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Donde se crean los botones. Si lo dejas vacio usa este mismo objeto.")]
    public Transform container;
    [Tooltip("Prefab de un Button con texto (TextMeshPro o Text)")]
    public GameObject buttonPrefab;

    [Header("Feedback visual")]
    public Color selectedColor = new Color(1f, 0.8f, 0.2f);
    public Color normalColor = Color.white;

    private readonly List<Button> buttons = new List<Button>();

    // Crea los botones. NO dispara onSelect: solo marca selectedIndex como resaltado.
    public void Populate(IList<string> labels, Action<int> onSelect, int selectedIndex = 0)
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("[TabBar] Falta asignar el Button Prefab.", this);
            return;
        }

        Transform parent = container != null ? container : transform;
        UiUtil.ClearChildren(parent);
        buttons.Clear();

        for (int i = 0; i < labels.Count; i++)
        {
            int index = i; // copia local para el closure

            var go = Instantiate(buttonPrefab, parent);
            go.name = "Tab_" + labels[i];
            UiUtil.SetLabel(go, labels[i]);

            var button = go.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("[TabBar] El prefab no tiene componente Button.", this);
                continue;
            }

            buttons.Add(button);
            button.onClick.AddListener(() =>
            {
                Highlight(index);
                onSelect?.Invoke(index);
            });
        }

        Highlight(selectedIndex);
    }

    private void Highlight(int index)
    {
        for (int i = 0; i < buttons.Count; i++)
            UiUtil.SetSelected(buttons[i], i == index, selectedColor, normalColor);
    }
}