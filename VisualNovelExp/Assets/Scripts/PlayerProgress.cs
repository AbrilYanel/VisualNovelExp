using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgress", menuName = "Progress/PlayerProgress")]
public class PlayerProgress : ScriptableObject
{
    public int nivelActual = 1;
    public int interaccionesCompletadas = 0;
    public int interaccionesPorNivel = 3; // 3 interacciones por nivel (9 NPCs / 3 niveles)

    [Header("Monedas")]
    public int monedas = 0;
    public int monedaInicial = 15; // Debe alcanzar justo para comprar la primera fila de kana (a,i,u,e,o)

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
        Debug.Log("�Subiste al nivel " + nivelActual + "!");
    }

    public bool PuedeInteractuar(int nivelRequerido)
    {
        return nivelActual >= nivelRequerido;
    }

    public void GanarMonedas(int cantidad)
    {
        monedas += cantidad;
        Debug.Log($"Ganaste {cantidad} monedas. Total: {monedas}");
    }

    public bool GastarMonedas(int cantidad)
    {
        if (monedas < cantidad)
        {
            Debug.Log("No tenés monedas suficientes.");
            return false;
        }

        monedas -= cantidad;
        return true;
    }

    public void Resetear()
    {
        nivelActual = 1;
        interaccionesCompletadas = 0;
        monedas = monedaInicial;
    }
}