using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_Journal : MonoBehaviour
{
    [Header("Data")]
    public JournalData journalData;

    [Header("UI")]
    public GameObject journalPanel;
    public Transform contenedor;        // Vertical Layout Group
    public GameObject entradaPrefab;
    public TextMeshProUGUI textoVacio;  // "Mis conocimientos" inicial

    private bool estaAbierto = false;

    void Start()
    {
        journalPanel.SetActive(false);
        journalData.LimpiarTodo(); 
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (estaAbierto) CerrarJournal();
            else AbrirJournal();
        }
    }

    public void AbrirJournal()
    {
        estaAbierto = true;
        journalPanel.SetActive(true);
        RefrescarLista();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarJournal()
    {
        estaAbierto = false;
        journalPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void RefrescarLista()
    {
        // Limpiar entradas anteriores
        foreach (Transform t in contenedor)
            Destroy(t.gameObject);

        if (journalData.palabrasAprendidas.Count == 0)
        {
            textoVacio.gameObject.SetActive(true);
            textoVacio.text = "Todavía no aprendiste ninguna palabra.\n¡Hablá con los NPCs!";
            return;
        }

        textoVacio.gameObject.SetActive(false);

        foreach (var palabra in journalData.palabrasAprendidas)
        {
            GameObject entrada = Instantiate(entradaPrefab, contenedor);
            EntradaJournal ej = entrada.GetComponent<EntradaJournal>();
            ej.Configurar(palabra.hiragana, palabra.romaji, palabra.traduccion);
        }
    }

    // Llamás esto desde Manager_Minijuego al completar
    public void RegistrarPalabras(List<PalabraAprendida> palabras)
    {
        foreach (var p in palabras)
            journalData.AgregarPalabra(p);
    }
}