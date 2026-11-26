using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Panel_Vocabulario : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI textoLista;
    public Button botonEmpezar;
    public Button botonVolver;
    public TextMeshProUGUI textoEstado;   // efectos de las necesidades

    System.Action alEmpezar;

    void Awake()
    {
        if (panel) panel.SetActive(false);
        if (botonEmpezar) botonEmpezar.onClick.AddListener(Empezar);
        if (botonVolver) botonVolver.onClick.AddListener(Volver);
    }

    public void Mostrar(List<PalabraAprendida> palabras, string efectos, System.Action empezar)
    {
        Debug.Log($"[Vocabulario] palabras recibidas: {(palabras != null ? palabras.Count : -1)}");
        alEmpezar = empezar;

        var sb = new System.Text.StringBuilder();
        if (palabras != null)
            foreach (var p in palabras)
                sb.AppendLine($"<size=120%>{p.hiragana}</size>    <b>{p.romaji}</b>    =  {p.traduccion}");
        textoLista.text = sb.ToString();

        if (textoEstado)
        {
            textoEstado.text = efectos;
            textoEstado.gameObject.SetActive(!string.IsNullOrEmpty(efectos));
        }

        panel.SetActive(true);

        if (palabras != null && palabras.Count > 0)
        {
            var journal = FindObjectOfType<Manager_Journal>();
            if (journal) journal.RegistrarPalabras(palabras);
            EventosJuego.Emitir("vocabulario_visto", palabras.Count.ToString());
        }
    }

    void Empezar()
    {
        panel.SetActive(false);
        var accion = alEmpezar;
        alEmpezar = null;
        accion?.Invoke();
    }

    void Volver()
    {
        panel.SetActive(false);
        alEmpezar = null;
        EventosJuego.Emitir("vocabulario_volver");
        if (Manager_Interaccion.I) Manager_Interaccion.I.SalirDelDialogo();
    }
}