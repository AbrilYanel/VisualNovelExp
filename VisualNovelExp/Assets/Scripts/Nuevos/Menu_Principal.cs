using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Principal : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena del editor de personajes (o de la casa, en la build sin editor)")]
    public string escenaSiguiente = "EditorComplejo";
    public string escenaAnterior = "MenuPrincipal";
    public Button botonContinuar;

    void Start()
    {
        // Al volver de otra escena, el cursor puede haber quedado bloqueado
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        EventosJuego.Emitir("menu_inicio");
        if (botonContinuar) botonContinuar.gameObject.SetActive(Partida.hayPartida);
    }

    // Botón "Jugar"
    public void Jugar()
    {
        // Partida nueva: se olvida lo de la partida anterior (si se volvió desde la pausa)
        Partida.Iniciada = false;          // PlayerProgress se resetea al entrar a la casa
        Partida.hayPartida = false;
        Partida.vistos.Clear();            // journal, palabras y mensajes vistos
        Punto_Aparicion.siguiente = "";    // aparecer en el punto inicial de la casa
        CharacterLookSaveSystem.DeleteSave(); // el editor empieza con el personaje inicial


        EventosJuego.Emitir("menu_jugar");
        SceneManager.LoadScene(escenaSiguiente);
    }

    public void Continuar()
    {
        if (!Partida.hayPartida) return;
        Punto_Aparicion.siguiente = Punto_Aparicion.CONTINUAR;
        EventosJuego.Emitir("menu_continuar");
        SceneManager.LoadScene(Partida.escenaGuardada);
    }


    public void Volver()
    {
        EventosJuego.Emitir("menu_volver");
        SceneManager.LoadScene(escenaAnterior);
    }

    // Botón "Salir"
    public void Salir()
    {
        EventosJuego.Emitir("menu_salir");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}