using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PreguntaEntrevista
{
    public string preguntaEspanol;      // contexto para el jugador
    public string preguntaJapones;      // lo que dice el jugador
    public OpcionEntrevista[] opciones;
}

[System.Serializable]
public class OpcionEntrevista
{
    public string textoOpcion;          // lo que elige el jugador
    public string respuestaDelNPC;      // cómo reacciona el entrevistado
    public bool esCorrecta;
    public int puntaje;                 // 0, 1 o 2 puntos
    public ReaccionNPC reaccion;
}

public enum ReaccionNPC
{
    Feliz,
    Neutral,
    Incomodo,
    Enojado
}

[CreateAssetMenu(fileName = "EntrevistaData",
                 menuName = "Entrevista/EntrevistaData")]
public class EntrevistaData : ScriptableObject
{
    public string nombreEntrevistado;
    public List<PreguntaEntrevista> preguntas;
    public int puntajeMaximo;           // calculado automáticamente
    public int puntajeMinimoExito = 3;  // mínimo para entrevista exitosa
}