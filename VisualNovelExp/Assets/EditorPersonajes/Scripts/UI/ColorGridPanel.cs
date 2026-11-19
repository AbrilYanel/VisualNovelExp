using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorGridPanel : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Contenedor con Grid Layout Group")]
    public Transform contenedor;
    [Tooltip("Prefab de un Button. Su Image raíz (o un hijo llamado 'Swatch') muestra el color")]
    public GameObject botonColorPrefab;
    [Tooltip("Opcional: título sobre la grilla, ej. 'Color de pelo'")]
    public TextMeshProUGUI titulo;

    [Header("Resaltado")]
    public Vector3 escalaSeleccionado = new Vector3(1.25f, 1.25f, 1f);

    private readonly List<Button> botones = new List<Button>();

    public void Populate(GrupoColor grupo, int seleccionado, Action<int> alElegir)
    {
        if (contenedor == null || botonColorPrefab == null)
        {
            Debug.LogError("[ColorGridPanel] Faltan Contenedor o Boton Color Prefab.", this);
            return;
        }

        UiUtil.ClearChildren(contenedor);
        botones.Clear();
        if (titulo) titulo.text = string.IsNullOrEmpty(grupo.nombre) ? "Color" : grupo.nombre;

        for (int i = 0; i < grupo.colores.Count; i++)
        {
            int indice = i; // copia local para el closure
            var go = Instantiate(botonColorPrefab, contenedor);
            go.name = "Color_" + i;

            var swatch = go.transform.Find("Swatch");
            var img = swatch ? swatch.GetComponent<Image>() : go.GetComponent<Image>();
            if (img) img.color = grupo.colores[i];

            var boton = go.GetComponent<Button>();
            if (boton == null) continue;
            botones.Add(boton);
            boton.onClick.AddListener(() =>
            {
                Resaltar(indice);
                alElegir?.Invoke(indice);
            });
        }

        Resaltar(seleccionado);
    }

    private void Resaltar(int indice)
    {
        for (int i = 0; i < botones.Count; i++)
            botones[i].transform.localScale = i == indice ? escalaSeleccionado : Vector3.one;
    }
}