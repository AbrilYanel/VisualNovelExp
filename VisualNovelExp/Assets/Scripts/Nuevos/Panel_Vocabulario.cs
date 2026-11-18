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

    System.Action alEmpezar;

    void Awake()
    {
        if (panel) panel.SetActive(false);
        if (botonEmpezar) botonEmpezar.onClick.AddListener(Empezar);
        if (botonVolver) botonVolver.onClick.AddListener(Volver);
    }

    public void Mostrar(List<PalabraAprendida> palabras, System.Action empezar)
    {
        alEmpezar = empezar;
        var sb = new System.Text.StringBuilder();
        foreach (var p in palabras)
            sb.AppendLine($"<size=120%>{p.hiragana}</size>    <b>{p.romaji}</b>    =  {p.traduccion}");
        textoLista.text = sb.ToString();
        panel.SetActive(true);

        var journal = FindObjectOfType<Manager_Journal>();
        if (journal) journal.RegistrarPalabras(palabras);
        EventosJuego.Emitir("vocabulario_visto", palabras.Count.ToString());
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