using System.Collections;
using UnityEngine;
using TMPro;

public class Mensaje_UI : MonoBehaviour
{
    public static Mensaje_UI I;
    public GameObject caja;
    public TextMeshProUGUI texto;
    Coroutine actual;

    void Awake()
    {
        I = this;
        if (caja) caja.SetActive(false);
    }

    public void Mostrar(string mensaje, float segundos = 4f)
    {
        if (actual != null) StopCoroutine(actual);
        actual = StartCoroutine(Rutina(mensaje, segundos));
    }

    IEnumerator Rutina(string mensaje, float segundos)
    {
        texto.text = mensaje;
        caja.SetActive(true);
        yield return new WaitForSeconds(segundos);
        caja.SetActive(false);
        actual = null;
    }
}