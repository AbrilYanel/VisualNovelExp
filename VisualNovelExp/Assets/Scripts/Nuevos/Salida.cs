using UnityEngine;
using TMPro;

public class PuertaMensaje : MonoBehaviour
{
    [SerializeField] TMP_Text textoInteraccion;   // texto centrado en el Canvas
    [SerializeField] float distancia = 2.5f;      // qué tan cerca hay que estar

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        textoInteraccion.text = "Presiona E para salir";
        textoInteraccion.gameObject.SetActive(false);
    }

    void Update()
    {
        bool mirando = Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distancia)
                       && hit.collider.transform.IsChildOf(transform);

        if (textoInteraccion.gameObject.activeSelf != mirando)
            textoInteraccion.gameObject.SetActive(mirando);
    }

    void OnDisable()
    {
        if (textoInteraccion != null) textoInteraccion.gameObject.SetActive(false);
    }
}