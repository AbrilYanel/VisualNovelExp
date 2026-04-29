using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgress", menuName = "Progress/PlayerProgress")]
public class PlayerProgress : ScriptableObject
{
    public int nivelActual = 1;
    public int interaccionesCompletadas = 0;
    public int interaccionesPorNivel = 2; // cuántas para subir de nivel

    public void CompletarInteraccion()
    {
        interaccionesCompletadas++;
        if (interaccionesCompletadas >= interaccionesPorNivel)
        {
            SubirNivel();
        }
    }

    public void SubirNivel()
    {
        nivelActual++;
        interaccionesCompletadas = 0;
        Debug.Log("¡Subiste al nivel " + nivelActual + "!");
    }

    public bool PuedeInteractuar(int nivelRequerido)
    {
        return nivelActual >= nivelRequerido;
    }

    public void Resetear()
    {
        nivelActual = 1;
        interaccionesCompletadas = 0;
    }
}