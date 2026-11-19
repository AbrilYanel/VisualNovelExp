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

    [Header("Animación")]
    [Tooltip("Tiempo mínimo entre abrir y cerrar, para ignorar pulsaciones muy rápidas")]
    public float tiempoEntreToggles = 0.3f;
    private float proximoToggle;
    private Coroutine corrutinaCierre;

    void Start()
    {
        journalPanel.SetActive(false);
        if (Partida.vistos.Add("journal_limpio")) journalData.LimpiarTodo();
    }
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;
        if (Time.unscaledTime < proximoToggle) return;
        proximoToggle = Time.unscaledTime + tiempoEntreToggles;

        if (estaAbierto) CerrarJournal();
        else AbrirJournal();
    }

    public void AbrirJournal()
    {
        // Si había un cierre en curso, se cancela para que no apague el panel después
        if (corrutinaCierre != null) { StopCoroutine(corrutinaCierre); corrutinaCierre = null; }

        EventosJuego.Emitir("journal");
        estaAbierto = true;
        journalPanel.SetActive(true);
        if (animator)
        {
            animator.ResetTrigger("Close");
            animator.SetTrigger("Open");
        }
        RefrescarLista();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarJournal()
    {
        estaAbierto = false;
        if (animator)
        {
            animator.ResetTrigger("Open");
            animator.SetTrigger("Close");
        }

        if (!EstadoUI.Bloqueada)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (corrutinaCierre != null) StopCoroutine(corrutinaCierre);
        corrutinaCierre = StartCoroutine(DesactivarTrasAnimacion());
    }

    IEnumerator DesactivarTrasAnimacion()
    {
        if (animator == null)
        {
            journalPanel.SetActive(false);
            corrutinaCierre = null;
            yield break;
        }

        yield return null;

        // Espera a que empiece "Close" (máximo 1 segundo)
        float t = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Close") && t < 1f)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Espera a que "Close" termine
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Close") &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        if (!estaAbierto) journalPanel.SetActive(false);   // solo si sigue cerrado
        corrutinaCierre = null;
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