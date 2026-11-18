using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tienda reutilizada como konbini: misma UI, animación y sonido que la tienda de kanas,
// pero vende comida y ocio en lugar de filas de kana.
public class Manager_Tienda : MonoBehaviour
{
    [System.Serializable]
    public class Articulo
    {
        public string hiragana = "おにぎり";
        public string romaji = "onigiri";
        public string traduccion = "bola de arroz";
        public int precio = 10;
        [Tooltip("Si es comida, se guarda en el refrigerador de la casa; si no, aplica su efecto al comprar")]
        public bool esComida = true;
        public Necesidad necesidad = Necesidad.Energia;
        public float cantidad = 30f;
    }

    [Header("Datos")]
    public PlayerProgress playerProgress;
    public List<Articulo> catalogo = new List<Articulo>
    {
        new Articulo { hiragana = "おにぎり", romaji = "onigiri", traduccion = "bola de arroz",
                       precio = 10, esComida = true },
        new Articulo { hiragana = "コーヒー", romaji = "kōhī", traduccion = "café",
                       precio = 8, esComida = false, necesidad = Necesidad.Energia, cantidad = 30f },
        new Articulo { hiragana = "ざっし", romaji = "zasshi", traduccion = "revista",
                       precio = 12, esComida = false, necesidad = Necesidad.Diversion, cantidad = 35f },
    };

    [Header("UI (la misma de antes)")]
    public GameObject panelTienda;
    public Transform contenedorItems;
    public GameObject itemTiendaPrefab;
    public TextMeshProUGUI textoMonedas;
    public Button botonCerrar;
    [Tooltip("Opcional. Si está vacío, los mensajes salen en la caja de mensajes (Mensaje_UI)")]
    public TextMeshProUGUI textoMensaje;

    [Header("Control de jugador (solo si no hay Manager_Interaccion en la escena)")]
    public MonoBehaviour cameraController;
    public MonoBehaviour Player_Movement;

    [Header("Animación y sonido")]
    public Animator animator;
    public AudioClip sonidoComprar;
    private AudioSource audioSource;

    private bool tiendaAbierta = false;
    private Coroutine corrutinaCierre;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (panelTienda != null) panelTienda.SetActive(false);
        SetupBotonCerrar();
        NukeRaycastBlockers();
        SetupLayout();
    }

    void SetupBotonCerrar()
    {
        if (botonCerrar == null) { Debug.LogError("botonCerrar NULL"); return; }
        botonCerrar.onClick.RemoveAllListeners();
        botonCerrar.onClick.AddListener(CerrarTienda);
        var nav = botonCerrar.navigation; nav.mode = Navigation.Mode.None; botonCerrar.navigation = nav;
    }

    // Apaga el raycast de todo lo que no sea Button
    void NukeRaycastBlockers()
    {
        if (panelTienda == null) return;

        foreach (var img in panelTienda.GetComponentsInChildren<Image>(true))
        {
            bool esBoton = img.GetComponent<Button>() != null || img.GetComponentInParent<Button>() != null;
            img.raycastTarget = esBoton;
        }

        foreach (var tmp in panelTienda.GetComponentsInChildren<TextMeshProUGUI>(true))
            tmp.raycastTarget = tmp.GetComponentInParent<Button>() != null;
    }

    void SetupLayout()
    {
        if (contenedorItems == null) return;
        var vlg = contenedorItems.GetComponent<VerticalLayoutGroup>() ?? contenedorItems.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 100f;

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
        if (tiendaAbierta) return;

        if (corrutinaCierre != null)
        {
            StopCoroutine(corrutinaCierre);
            corrutinaCierre = null;
        }

        if (animator)
        {
            animator.ResetTrigger("Close");
            animator.ResetTrigger("Open");
            animator.Play("Idle", 0, 0f);
        }

        tiendaAbierta = true;
        BloquearJugador();

        panelTienda.SetActive(true);
        if (animator) animator.SetTrigger("Open");

        panelTienda.transform.SetAsLastSibling();
        if (botonCerrar) { botonCerrar.transform.SetAsLastSibling(); botonCerrar.interactable = true; }

        NukeRaycastBlockers();
        RefrescarLista();

        Avisar("いらっしゃいませ (irasshaimase): ¡bienvenido!");
        EventosJuego.Emitir("tienda_abierta");
    }

    public void CerrarTienda()
    {
        if (!tiendaAbierta) return;
        tiendaAbierta = false;

        if (corrutinaCierre != null)
        {
            StopCoroutine(corrutinaCierre);
            corrutinaCierre = null;
        }

        if (animator) animator.SetTrigger("Close");
        LiberarJugador();
        EventosJuego.Emitir("tienda_cerrada");

        corrutinaCierre = StartCoroutine(DesactivarTrasAnimacion());
    }

    IEnumerator DesactivarTrasAnimacion()
    {
        if (animator == null)
        {
            panelTienda.SetActive(false);
            corrutinaCierre = null;
            yield break;
        }

        yield return null;

        float timeout = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Close"))
        {
            timeout += Time.deltaTime;
            if (timeout > 3f) break;
            yield return null;
        }

        float duracion = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duracion);

        if (!tiendaAbierta)
            panelTienda.SetActive(false);

        corrutinaCierre = null;
    }

    void RefrescarLista()
    {
        if (contenedorItems == null || itemTiendaPrefab == null) return;
        for (int i = contenedorItems.childCount - 1; i >= 0; i--) Destroy(contenedorItems.GetChild(i).gameObject);
        if (textoMonedas && playerProgress) textoMonedas.text = $"{playerProgress.monedas}";

        foreach (var articulo in catalogo)
        {
            if (articulo == null) continue;
            var a = articulo; // copia local para el closure

            var obj = Instantiate(itemTiendaPrefab, contenedorItems, false);
            obj.transform.localScale = Vector3.one;
            var le = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            le.preferredHeight = 72; le.flexibleWidth = 1; le.minHeight = 60;

            var item = obj.GetComponent<ItemTienda>();
            if (!item) { Destroy(obj); continue; }

            item.ConfigurarArticulo(
                $"{a.hiragana} ({a.romaji})",
                $"{a.precio} monedas",
                $"{a.traduccion} · {Efecto(a)}",
                () => Comprar(a));
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenedorItems as RectTransform);
    }

    void Comprar(Articulo a)
    {
        if (playerProgress == null) return;

        if (!playerProgress.GastarMonedas(a.precio))
        {
            Avisar("No tienes monedas suficientes. Ayuda a un vecino para ganar más.");
            return;
        }

        if (a.esComida) playerProgress.comida++;
        else if (Manager_Necesidades.I) Manager_Necesidades.I.Sumar(a.necesidad, a.cantidad);

        if (audioSource != null && sonidoComprar != null)
            audioSource.PlayOneShot(sonidoComprar);

        string extra = a.esComida ? $" Comida en casa: {playerProgress.comida}." : "";
        Avisar($"ありがとうございます (arigatō gozaimasu). Compraste: {a.traduccion}.{extra}");
        EventosJuego.Emitir("compra", a.romaji);

        RefrescarLista();
    }

    string Efecto(Articulo a)
    {
        if (a.esComida) return "para el refrigerador";
        switch (a.necesidad)
        {
            case Necesidad.Energia: return $"+{a.cantidad:0} energía";
            case Necesidad.Diversion: return $"+{a.cantidad:0} diversión";
            default: return $"+{a.cantidad:0} hambre";
        }
    }

    void Avisar(string texto)
    {
        if (textoMensaje) textoMensaje.text = texto;
        else if (Mensaje_UI.I) Mensaje_UI.I.Mostrar(texto, 3.5f);
    }

    void BloquearJugador()
    {
        if (Manager_Interaccion.I) { Manager_Interaccion.I.BloquearJugador(); return; }
        if (cameraController) cameraController.enabled = false;
        if (Player_Movement) Player_Movement.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LiberarJugador()
    {
        if (Manager_Interaccion.I) { Manager_Interaccion.I.LiberarJugador(); return; }
        if (cameraController) cameraController.enabled = true;
        if (Player_Movement) Player_Movement.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
