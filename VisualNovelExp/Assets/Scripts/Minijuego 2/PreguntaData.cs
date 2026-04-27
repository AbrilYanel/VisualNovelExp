using System.Collections.Generic;
using UnityEngine;

public enum TipoPregunta { FillBlank, WordOrder }

[System.Serializable]
public class PreguntaData
{
    public TipoPregunta tipo;

    [Header("Compartido")]
    public string instruccion; // texto de ayuda arriba

    [Header("Fill in the Blank")]
    public string oracionConBlanco; // "Watashi ___ gakusei desu"
    public string[] opciones;       // ["wa", "wo", "ni", "ga"]
    public int indiceRespuestaCorrecta;

    [Header("Word Order")]
    public string oracionEspanol;   // "El gato come pescado"
    public string[] palabrasDesordenadas; // ["taberu","neko","sakana","wa","wo"]
    public string[] ordenCorrecto;        // ["neko","wa","sakana","wo","taberu"]

    [Header("Journal")]
    public List<PalabraAprendida> palabrasQueEnsena;
}