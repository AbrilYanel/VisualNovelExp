using UnityEngine;
using TMPro;

public class IndicadorInteraccion : MonoBehaviour
{
    [SerializeField] TMP_Text textoInteraccion;          // texto centrado en el Canvas
    [SerializeField] float distancia = 2.5f;             // usa la misma distancia que Player_Camera
    [SerializeField] string textoPorDefecto = "Presiona E";
    [Tooltip("Oculta el texto cuando el cursor está libre (diálogos, minijuegos, pausa, journal)")]
    [SerializeField] bool ocultarConCursorLibre = true;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        Mostrar(null);
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        Mostrar(BuscarTexto());
    }

    string BuscarTexto()
    {
        if (cam == null) return null;
        if (ocultarConCursorLibre && Cursor.lockState != CursorLockMode.Locked) return null;

        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distancia))
            return null;

        // Sirve para el objeto golpeado o cualquiera de sus padres
        var interactuable = hit.collider.GetComponentInParent<IInteractuable>();
        if (interactuable == null) return null;

        // Si el componente está desactivado (ej.: NPC que no atiende), no muestra nada
        if (interactuable is Behaviour b && !b.isActiveAndEnabled) return null;

        // NPC con su minijuego ya completado (y que no se repite): no muestra nada
        var npc = hit.collider.GetComponentInParent<Interaccion_NPC>();
        if (npc != null && npc.completado && !npc.repetible) return null;

        // Texto propio opcional (ej.: "Presiona E para hablar")
        var mensaje = hit.collider.GetComponentInParent<MensajeInteraccion>();
        if (mensaje != null && !string.IsNullOrEmpty(mensaje.texto)) return mensaje.texto;

        return textoPorDefecto;
    }

    void Mostrar(string texto)
    {
        if (textoInteraccion == null) return;

        bool visible = !string.IsNullOrEmpty(texto);
        if (visible && textoInteraccion.text != texto) textoInteraccion.text = texto;
        if (textoInteraccion.gameObject.activeSelf != visible) textoInteraccion.gameObject.SetActive(visible);
    }

    void OnDisable()
    {
        if (textoInteraccion != null) textoInteraccion.gameObject.SetActive(false);
    }
}