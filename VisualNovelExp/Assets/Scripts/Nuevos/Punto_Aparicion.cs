using UnityEngine;

public class Punto_Aparicion : MonoBehaviour
{
    public static string siguiente = "";

    [Tooltip("inicio, desde_casa, desde_ciudad...")]
    public string id = "inicio";
    [Tooltip("Uno por escena: se usa si no se llegó por una puerta")]
    public bool porDefecto = false;
    public Transform jugador;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar() { siguiente = ""; }

    void Start()
    {
        bool hayDestino = !string.IsNullOrEmpty(siguiente) && ExisteEnEscena(siguiente);
        bool meToca = hayDestino ? siguiente == id : porDefecto;
        if (!meToca) return;
        Mover();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static bool ExisteEnEscena(string buscado)
    {
        foreach (var p in FindObjectsOfType<Punto_Aparicion>())
            if (p.id == buscado) return true;
        return false;
    }

    void Mover()
    {
        if (jugador == null) { Debug.LogError("[Punto_Aparicion] Falta asignar el jugador en " + name); return; }
        var cc = jugador.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        jugador.SetPositionAndRotation(transform.position, transform.rotation);
        if (cc) cc.enabled = true;
        var rb = jugador.GetComponent<Rigidbody>();
        if (rb) rb.position = transform.position;
    }
}