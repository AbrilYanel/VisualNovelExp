using UnityEngine;
using TMPro;

public class EntradaJournal : MonoBehaviour
{
    public TextMeshProUGUI textoHiragana;
    public TextMeshProUGUI textoRomaji;
    public TextMeshProUGUI textoTraduccion;

    public void Configurar(string hiragana, string romaji, string traduccion)
    {
        textoHiragana.text = hiragana;
        textoRomaji.text = romaji;
        textoTraduccion.text = traduccion;
    }
}