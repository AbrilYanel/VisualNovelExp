using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerProgress", menuName = "Progress/PlayerProgress")]
public class PlayerProgress : ScriptableObject
{
    public int nivelActual = 1;
    public int interaccionesCompletadas = 0;
    public int interaccionesPorNivel = 3; // 3 interacciones por nivel (9 NPCs / 3 niveles)

    [Header("Monedas")]
    public int monedas = 0;
    public int monedaInicial = 15; 
    [Header("Comida en casa")]
    public int comida = 0;
    public int comidaInicial = 1; // la que se come en el tutorial

    [Header("Necesidades (0 a 100)")]
    public float hambre;
    public float energia;
    public float diversion;
    public float hambreInicial = 35f;   // bajo a propósito: el tutorial pide comer
    public float energiaInicial = 70f;
    public float diversionInicial = 60f;

    [Header("Estado de la partida")]
    public bool tutorialCompleto;
    public List<string> npcsCompletados = new List<string>();

    [Header("Días")]
    public int dia = 1;
    public int franjasUsadas = 0;
    public int monedasInicioDia;
    public int palabrasInicioDia;
    public bool festivalCompletado;

    public int pasoTutorial;

    [Header("Misión del festival")]
    public bool tieneCamara;
    public bool permisoObtenido;
    public bool entrevistaCompletada;
    public int puntajeEntrevista;

    public bool ConsumirComida()
    {
        if (comida <= 0) return false;
        comida--;
        return true;
    }


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
        comida = comidaInicial;
        hambre = hambreInicial;
        energia = energiaInicial;
        diversion = diversionInicial;
        tutorialCompleto = false;
        npcsCompletados.Clear();
        dia = 1;
        franjasUsadas = 0;
        monedasInicioDia = monedas;
        palabrasInicioDia = 0;
        festivalCompletado = false;
        pasoTutorial = 0;
        tieneCamara = false;
        permisoObtenido = false;
        entrevistaCompletada = false;
        puntajeEntrevista = 0;
    }
}