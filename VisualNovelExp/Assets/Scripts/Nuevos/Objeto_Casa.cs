using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Objeto_Casa : MonoBehaviour, IInteractuable
{
    public enum Accion { SoloPalabra, Espejo, Comer, Dormir, Entretener, Puerta }

    [Header("Qué hace")]
    public Accion accion = Accion.SoloPalabra;
    [Tooltip("Evento para el tutorial: espejo, refrigerador, cama, pc, puerta")]
    public string idEvento = "objeto";
    public float cantidad = 40f;

    [Header("Palabra que enseña")]
    public string hiragana = "かがみ";
    public string romaji = "kagami";
    public string traduccion = "espejo";

    [Header("Según la acción")]
    public PlayerProgress playerProgress;          // Comer
    [TextArea(2, 4)] public string mensajeExtra;   // PC: solo la primera vez en la partida
    public string escenaDestino = "Ciudad";        // Puerta: nombre exacto de la escena
    public string puntoDestino = "desde_casa";     // Puerta: id del Punto_Aparicion de esa escena

    public void Interact()
    {
        RegistrarPalabra();
        string palabra = $"{hiragana}  ({romaji})  =  {traduccion}";

        switch (accion)
        {
            case Accion.SoloPalabra:
                Mostrar(palabra);
                EventosJuego.Emitir(idEvento);
                break;

            case Accion.Espejo:
                Mostrar(palabra + "\nEste eres tú. Presiona C cuando quieras verte.");
                if (Panel_Personaje.I) Panel_Personaje.I.Abrir();
                EventosJuego.Emitir(idEvento);
                break;

            case Accion.Comer:
                if (playerProgress != null && playerProgress.ConsumirComida())
                {
                    Manager_Necesidades.I.Sumar(Necesidad.Hambre, cantidad);
                    Mostrar(palabra + $"\nComiste. Comida que queda: {playerProgress.comida}");
                    EventosJuego.Emitir(idEvento);
                }
                else
                {
                    Mostrar(palabra + "\nNo queda comida. Cómprala en la konbini.");
                }
                break;

            case Accion.Dormir:
                Fundido.I.Hacer(() =>
                {
                    Manager_Necesidades.I.Sumar(Necesidad.Energia, 100f);
                    Mostrar(palabra + "\nDormiste y recuperaste toda tu energía.");
                }, 1.2f);
                EventosJuego.Emitir(idEvento);
                break;

            case Accion.Entretener:
                bool primeraVez = Partida.vistos.Add("mensaje_" + idEvento);
                string extra = primeraVez ? mensajeExtra : "";
                Fundido.I.Hacer(() =>
                {
                    Manager_Necesidades.I.Sumar(Necesidad.Diversion, cantidad);
                    if (string.IsNullOrEmpty(extra)) Mostrar(palabra + "\nTe divertiste un rato.");
                    else Mostrar(palabra + "\n" + extra, 9f);
                }, 0.8f);
                EventosJuego.Emitir(idEvento);
                break;

            case Accion.Puerta:
                if (Manager_Tutorial.I != null && !Manager_Tutorial.I.PuedeSalir)
                {
                    Mostrar("Antes de salir, termina de revisar la casa.");
                    return;
                }
                EventosJuego.Emitir(idEvento);
                EventosJuego.Emitir("cambio_escena", escenaDestino);
                Punto_Aparicion.siguiente = puntoDestino;
                Fundido.I.Hacer(() => SceneManager.LoadScene(escenaDestino), 0.2f);
                break;
        }
    }

    void RegistrarPalabra()
    {
        if (string.IsNullOrEmpty(romaji)) return;
        if (!Partida.vistos.Add("palabra_" + romaji)) return; // ya registrada en esta partida
        var journal = FindObjectOfType<Manager_Journal>();
        if (journal) journal.RegistrarPalabras(new List<PalabraAprendida> {
            new PalabraAprendida {
                hiragana = hiragana, romaji = romaji,
                traduccion = traduccion, idFuente = "objeto_" + idEvento }
        });
        EventosJuego.Emitir("palabra_aprendida", romaji);
    }

    void Mostrar(string texto, float segundos = 4f)
    {
        if (Mensaje_UI.I) Mensaje_UI.I.Mostrar(texto, segundos);
    }
}