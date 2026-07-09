using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaMemorama : MonoBehaviour
{
    [Header("UI refs")]
    public Image imagenFondo;
    public Image imagenContenido; // para sprite (opcional)
    public TextMeshProUGUI textoKana;
    public TextMeshProUGUI textoRomaji;
    public Button boton;

    [HideInInspector] public ParejaDatos datoPareja;
    [HideInInspector] public bool estaRevelada = false;
    [HideInInspector] public bool estaEmparejada = false;

    System.Action<CartaMemorama> onClick;
    bool bloqueada = false;

    void Awake()
    {
        if (boton == null) boton = GetComponent<Button>();
        if (boton) boton.onClick.AddListener(() => onClick?.Invoke(this));
    }

    public void Configurar(ParejaDatos dato, bool esBloqueada, System.Action<CartaMemorama> callback)
    {
        datoPareja = dato;
        bloqueada = esBloqueada;
        onClick = callback;
        estaRevelada = false;
        estaEmparejada = false;
        ActualizarVisual(false);
    }

    public void Revelar(bool mostrar)
    {
        estaRevelada = mostrar;
        ActualizarVisual(mostrar);
    }

    public void MarcarEmparejada()
    {
        estaEmparejada = true;
        estaRevelada = true;
        if (imagenFondo) imagenFondo.color = new Color(0.6f, 1f, 0.6f);
        if (boton) boton.interactable = false;
        ActualizarVisual(true);
    }

    void ActualizarVisual(bool revelada)
    {
        if (!revelada)
        {
            // dorso
            if (textoKana) textoKana.text = "";
            if (textoRomaji) textoRomaji.text = "";
            if (imagenContenido) imagenContenido.enabled = false;
            if (imagenFondo) imagenFondo.color = Color.white;
        }
        else
        {
            if (bloqueada)
            {
                if (textoKana) textoKana.text = "?";
                if (textoRomaji) textoRomaji.text = "???";
                if (imagenContenido) imagenContenido.enabled = false;
            }
            else
            {
                if (textoKana) textoKana.text = datoPareja.palabraJaponesa;
                if (textoRomaji) textoRomaji.text = datoPareja.romaji;
                if (imagenContenido && datoPareja.imagen != null)
                {
                    imagenContenido.enabled = true;
                    imagenContenido.sprite = datoPareja.imagen;
                }
                else if (imagenContenido) imagenContenido.enabled = false;
            }
            if (imagenFondo && !estaEmparejada) imagenFondo.color = new Color(1f, 0.95f, 0.7f);
        }
    }
}