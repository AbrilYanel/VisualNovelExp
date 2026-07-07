using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KanaCaracter
{
    public string kana;   // ej. "あ"
    public string romaji; // ej. "a"
}

[CreateAssetMenu(fileName = "KanaData", menuName = "Kanas/KanaData")]
public class KanaData : ScriptableObject
{
    [Header("Identificación")]
    public string id;          // ej. "fila_a", "fila_ka", ... único por fila
    public string nombreFila;  // ej. "Fila A (あいうえお)" - se muestra en la tienda

    [Header("Contenido")]
    public List<KanaCaracter> caracteres;

    [Header("Precio")]
    public int costo = 15;
}