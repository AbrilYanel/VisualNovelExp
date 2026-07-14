// CategoryPanel.cs - PASO 3
// Pon en: Assets/_CharacterEditor/Scripts/UI/CategoryPanel.cs
using UnityEngine;
using UnityEngine.UI;

public class CategoryPanel : MonoBehaviour
{
    [Header("Referencias UI")]
    public Transform buttonContainer; // El Content del ScrollView con Grid Layout Group
    public GameObject buttonPrefab;   // Prefab del botón con Image de miniatura

    [Header("Feedback visual")]
    public Color selectedColor = new Color(1f, 0.8f, 0.2f); // Amarillo selección
    public Color normalColor = Color.white;

    private Button selectedButton;

    /// <summary>
    /// Borra los botones viejos y genera nuevos a partir de la lista de items
    /// </summary>
    public void Populate(CosmeticItem[] items, System.Action<CosmeticItem> onSelect)
    {
        // Limpiar grid
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        selectedButton = null;

        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("[CategoryPanel] No hay items para mostrar en esta categoría");
            return;
        }

        foreach (var item in items)
        {
            if (item == null) continue;

            var go = Instantiate(buttonPrefab, buttonContainer);

            // Asignar icono
            var images = go.GetComponentsInChildren<Image>();
            // images[0] es la imagen del Button, images[1] suele ser la del icono si tenés estructura Button>Icon
            // Buscamos la más adecuada
            Image iconImage = null;
            if (images.Length > 1)
            {
                // Si el prefab tiene hijo Icon, usa ese
                iconImage = images[1];
            }
            else
            {
                iconImage = images[0];
            }

            if (iconImage != null)
            {
                iconImage.sprite = item.icon != null ? item.icon : item.sprite;
                iconImage.preserveAspect = true;
            }

            // Configurar click
            var button = go.GetComponent<Button>();
            if (button != null)
            {
                // Captura local para el closure
                var capturedItem = item;
                var capturedButton = button;
                button.onClick.AddListener(() =>
                {
                    OnItemClicked(capturedButton);
                    onSelect?.Invoke(capturedItem);
                });
            }

            // Tooltip por nombre
            go.name = $"Btn_{item.id}";
        }
    }

    private void OnItemClicked(Button clicked)
    {
        // Restaurar color anterior
        if (selectedButton != null)
        {
            var colors = selectedButton.colors;
            colors.normalColor = normalColor;
            selectedButton.colors = colors;
        }

        // Marcar nuevo seleccionado
        selectedButton = clicked;
        if (selectedButton != null)
        {
            var colors = selectedButton.colors;
            colors.normalColor = selectedColor;
            colors.selectedColor = selectedColor;
            selectedButton.colors = colors;
        }
    }
}
