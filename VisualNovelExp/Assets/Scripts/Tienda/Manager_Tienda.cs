using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Manager_Tienda : MonoBehaviour
{
    [Header("Datos")]
    public KanaInventario kanaInventario;
    public PlayerProgress playerProgress;
    public List<KanaData> catalogoFilas;

    [Header("UI")]
    public GameObject panelTienda;
    public Transform contenedorItems;
    public GameObject itemTiendaPrefab;
    public TextMeshProUGUI textoMonedas;
    public Button botonCerrar;

    [Header("Control de jugador")]
    public MonoBehaviour cameraController;
    public MonoBehaviour Player_Movement;

    private bool tiendaAbierta = false;

    void Start()
    {
        if (panelTienda != null) panelTienda.SetActive(false);
        SetupBotonCerrar();
        NukeRaycastBlockers();
        SetupLayout();
        if (kanaInventario != null) kanaInventario.Reiniciar();
    }

    void SetupBotonCerrar()
    {
        if (botonCerrar == null) { Debug.LogError("botonCerrar NULL"); return; }
        botonCerrar.onClick.RemoveAllListeners();
        botonCerrar.onClick.AddListener(CerrarTienda);
        var nav = botonCerrar.navigation; nav.mode = Navigation.Mode.None; botonCerrar.navigation = nav;
    }

    // Agresivo: apaga raycast de TODO lo que no sea Button
    void NukeRaycastBlockers()
    {
        if (panelTienda == null) return;

        // 1. Todas las Images que NO sean un Button
        foreach (var img in panelTienda.GetComponentsInChildren<Image>(true))
        {
            bool esBoton = img.GetComponent<Button>() != null || img.GetComponentInParent<Button>() != null;
            if (!esBoton)
            {
                img.raycastTarget = false;
            }
            else
            {
                img.raycastTarget = true; // asegurar que los botones sí reciban
            }
        }

        // 2. Todos los TMP que NO estén dentro de un Button
        foreach (var tmp in panelTienda.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            bool esBoton = tmp.GetComponentInParent<Button>() != null;
            tmp.raycastTarget = esBoton; // solo los textos de botones bloquean raycast
        }

        Debug.Log("[Manager_Tienda] Raycast blockers limpiados", panelTienda);
    }

    void SetupLayout()
    {
        if (contenedorItems == null) return;
        var vlg = contenedorItems.GetComponent<VerticalLayoutGroup>() ?? contenedorItems.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 70f;
        

        var csf = contenedorItems.GetComponent<ContentSizeFitter>() ?? contenedorItems.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void Update()
    {
        if (!tiendaAbierta) return;
        if (Input.GetKeyDown(KeyCode.Escape)) CerrarTienda();
       
    }

    public void AbrirTienda()
    {
        tiendaAbierta = true;
        // cerrar diálogos que tapan
        var inter = FindObjectOfType<Manager_Interaccion>();
        if (inter != null)
        {
            if (inter.dialoguePanel) inter.dialoguePanel.SetActive(false);
            if (inter.choicePanel) inter.choicePanel.SetActive(false);
            if (inter.minigameUI) inter.minigameUI.SetActive(false);
            if (inter.minijuego2UI) inter.minijuego2UI.SetActive(false);
        }
        if (cameraController) cameraController.enabled = false;
        if (Player_Movement) Player_Movement.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panelTienda.SetActive(true);
        panelTienda.transform.SetAsLastSibling();
        if (botonCerrar) { botonCerrar.transform.SetAsLastSibling(); botonCerrar.interactable = true; }

        NukeRaycastBlockers(); // re-aplicar por si algo se reactivó
        RefrescarLista();
    }

    public void CerrarTienda()
    {
        Debug.Log("[Manager_Tienda] CerrarTienda OK");
        tiendaAbierta = false;
        if (panelTienda) panelTienda.SetActive(false);
        if (cameraController) cameraController.enabled = true;
        if (Player_Movement) Player_Movement.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void RefrescarLista()
    {
        if (contenedorItems == null || itemTiendaPrefab == null) return;
        for (int i = contenedorItems.childCount - 1; i >= 0; i--) Destroy(contenedorItems.GetChild(i).gameObject);
        if (textoMonedas && playerProgress) textoMonedas.text = $"Monedas: {playerProgress.monedas}";

        foreach (var fila in catalogoFilas)
        {
            if (!fila) continue;
            var obj = Instantiate(itemTiendaPrefab, contenedorItems, false);
            obj.transform.localScale = Vector3.one;
            var le = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            le.preferredHeight = 72; le.flexibleWidth = 1; le.minHeight = 60;
            var item = obj.GetComponent<ItemTienda>();
            if (!item) { Destroy(obj); continue; }
            bool desbloqueada = kanaInventario && kanaInventario.EstaDesbloqueado(fila.id);
            item.Configurar(fila, desbloqueada, () => {
                if (kanaInventario.Comprar(fila, playerProgress)) RefrescarLista();
            });
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenedorItems as RectTransform);
    }
}
