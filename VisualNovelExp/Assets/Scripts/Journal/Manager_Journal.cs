using System.Collections.Generic;
using System.Collections;
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
    public Animator animator;
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
        animator.SetTrigger("Open");
        RefrescarLista();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarJournal()
    {
        estaAbierto = false;
        animator.SetTrigger("Close");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(DesactivarTrasAnimacion());

    }

    IEnumerator DesactivarTrasAnimacion()
    {
        // Si no hay animator, desactivar directo
        if (animator == null)
        {
            journalPanel.SetActive(false);
            yield break;
        }

        yield return null;

        // Esperar a que arranque el estado "Close"
        float timeout = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Close"))
        {
            timeout += Time.deltaTime;
            if (timeout > 3f) break;
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        journalPanel.SetActive(false);
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