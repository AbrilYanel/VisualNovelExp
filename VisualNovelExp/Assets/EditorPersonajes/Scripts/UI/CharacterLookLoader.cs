using UnityEngine;

public class CharacterLookLoader : MonoBehaviour
{
    public CharacterLookRenderer lookRenderer;

    [Tooltip("Registra en el log que empezó el juego. Se registra una sola vez por partida, " +
             "aunque haya varios loaders o se vuelva a la escena.")]
    public bool registrarPantallaJuego = true;

    void Start()
    {
        LoadCharacter();

        // RESEARCH: una sola vez por partida
        if (registrarPantallaJuego && Partida.vistos.Add("pantalla_juego"))
            ResearchAnalytics.LogGameplayScreenOn();
    }

    public void LoadCharacter()
    {
        if (lookRenderer == null)
        {
            Debug.LogError("[LookLoader] Falta el CharacterLookRenderer.", this);
            return;
        }
        lookRenderer.CargarGuardado();
    }
}