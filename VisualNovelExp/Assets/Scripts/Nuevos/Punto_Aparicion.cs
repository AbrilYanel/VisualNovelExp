using UnityEngine;

public class Punto_Aparicion : MonoBehaviour
{
    public const string CONTINUAR = "__continuar__";
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
        // Volver desde el menú: solo el punto por defecto se encarga, y usa la posición guardada
        if (siguiente == CONTINUAR)
        {
            if (!porDefecto) return;
            siguiente = "";
            Mover(Partida.posicionGuardada, Partida.rotacionGuardada);
            return;
        }

        bool hayDestino = !string.IsNullOrEmpty(siguiente) && ExisteEnEscena(siguiente);
        bool meToca = hayDestino ? siguiente == id : porDefecto;
        if (meToca) Mover(transform.position, transform.rotation);
    }

    static bool ExisteEnEscena(string buscado)
    {
        foreach (var p in FindObjectsOfType<Punto_Aparicion>())
            if (p.id == buscado) return true;
        return false;
    }

    void Mover(Vector3 posicion, Quaternion rotacion)
    {
        if (jugador == null) { Debug.LogError("[Punto_Aparicion] Falta asignar el jugador en " + name); return; }
        var cc = jugador.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        jugador.SetPositionAndRotation(posicion, rotacion);
        if (cc) cc.enabled = true;
        var rb = jugador.GetComponent<Rigidbody>();
        if (rb) rb.position = posicion;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}