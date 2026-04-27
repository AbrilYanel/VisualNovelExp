using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PalabraAprendida
{
    public string hiragana;
    public string romaji;
    public string traduccion;
    public string idFuente; // "neko_minijuego", para no duplicar
}

[CreateAssetMenu(fileName = "JournalData", menuName = "Journal/JournalData")]
public class JournalData : ScriptableObject
{
    public List<PalabraAprendida> palabrasAprendidas = new List<PalabraAprendida>();

    public void AgregarPalabra(PalabraAprendida palabra)
    {
        // Evitar duplicados
        if (palabrasAprendidas.Exists(p => p.idFuente == palabra.idFuente))
            return;

        palabrasAprendidas.Add(palabra);
    }

    public void LimpiarTodo()
    {
        palabrasAprendidas.Clear();
    }
}