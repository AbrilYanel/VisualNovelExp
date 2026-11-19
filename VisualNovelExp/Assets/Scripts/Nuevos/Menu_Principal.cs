using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Principal : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena del editor de personajes (o de la casa, en la build sin editor)")]
    public string escenaSiguiente = "EditorComplejo";
    public string escenaAnterior = "MenuPrincipal";

    void Start()
    {
        // Al volver de otra escena, el cursor puede haber quedado bloqueado
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        EventosJuego.Emitir("menu_inicio");
    }

    // Botón "Jugar"
    public void Jugar()
    {
        EventosJuego.Emitir("menu_jugar");
        SceneManager.LoadScene(escenaSiguiente);
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