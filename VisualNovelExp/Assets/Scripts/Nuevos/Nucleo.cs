using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IInteractuable
{
    void Interact();
}

// true mientras hay diálogo, minijuego, tienda o panel abierto
public static class EstadoUI
{
    public static bool Bloqueada;
    public static int FrameLiberada = -10;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar()
    {
        Bloqueada = false;
        FrameLiberada = -10;
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    // Al entrar a otra escena no queda ninguna interfaz abierta
    static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Bloqueada = false;
        FrameLiberada = Time.frameCount;
        Time.timeScale = 1f;
    }
}

// Estado de la partida que no hace falta guardar en el asset
public static class Partida
{
    public static bool Iniciada;                                   // PlayerProgress ya se reseteó
    public static HashSet<string> vistos = new HashSet<string>();  // palabras y mensajes ya mostrados

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar() { Iniciada = false; vistos.Clear(); }
}

// Cualquier script avisa que algo pasó; el tutorial y el log escuchan
public static class EventosJuego
{
    public static event System.Action<string, string> AlOcurrir;

    public static void Emitir(string evento, string detalle = "")
    {
        AlOcurrir?.Invoke(evento, detalle);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar() { AlOcurrir = null; }
}