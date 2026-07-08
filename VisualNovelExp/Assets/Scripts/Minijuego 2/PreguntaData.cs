// Reemplaza tu PreguntaData.cs con este
// Añade idFilaKana para bloqueo visual

using System.Collections.Generic;
using UnityEngine;

public enum TipoPregunta { FillBlank, WordOrder }

[System.Serializable]
public class PreguntaData
{
    public TipoPregunta tipo;

    [Header("Compartido")]
    public string instruccion;

    [Header("Progresión Kanas")]
    public string idFilaKana = ""; // ej: "fila_ta" - si está vacío, no bloquea. Si el jugador no la tiene comprada, se muestra 🔒

    [Header("Fill in the Blank")]
    public string oracionConBlanco;
    public string[] opciones;
    public int indiceRespuestaCorrecta;

    [Header("Word Order")]
    public string oracionEspanol;
    public string[] palabrasDesordenadas;
    public string[] ordenCorrecto;

    [Header("Journal")]
    public List<PalabraAprendida> palabrasQueEnsena;
}
