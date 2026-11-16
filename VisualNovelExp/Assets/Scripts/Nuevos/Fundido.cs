using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fundido : MonoBehaviour
{
    public static Fundido I;
    public CanvasGroup grupo;
    public float duracion = 0.6f;

    void Awake()
    {
        I = this;
        if (grupo) { grupo.alpha = 0f; grupo.blocksRaycasts = false; }
    }

    public void Hacer(System.Action enMedio, float espera)
    {
        StartCoroutine(Rutina(enMedio, espera));
    }

    IEnumerator Rutina(System.Action enMedio, float espera)
    {
        if (Manager_Interaccion.I) Manager_Interaccion.I.BloquearJugador();
        grupo.blocksRaycasts = true;
        yield return Fade(0f, 1f);
        enMedio?.Invoke();
        yield return new WaitForSeconds(espera);
        yield return Fade(1f, 0f);
        grupo.blocksRaycasts = false;
        if (Manager_Interaccion.I) Manager_Interaccion.I.LiberarJugador();
    }

    IEnumerator Fade(float desde, float hasta)
    {
        for (float t = 0f; t < duracion; t += Time.deltaTime)
        {
            grupo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        grupo.alpha = hasta;
    }
}