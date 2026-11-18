using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Manager_Tutorial : MonoBehaviour
{
    public static Manager_Tutorial I;

    [System.Serializable]
    public class Paso
    {
        [TextArea(2, 4)] public string texto;
        public string eventoEsperado;
    }

    public PlayerProgress playerProgress;
    public GameObject panelTutorial;
    public TextMeshProUGUI textoTutorial;
    public List<Paso> pasos = new List<Paso>();

    int indice;
    public bool Completo { get; private set; }
    public bool PuedeSalir => Completo ||
        (indice < pasos.Count && pasos[indice].eventoEsperado == "puerta");

    void Awake() { I = this; }
    void OnDestroy() { if (I == this) I = null; }
    void OnEnable() { EventosJuego.AlOcurrir += AlEvento; }
    void OnDisable() { EventosJuego.AlOcurrir -= AlEvento; }

    void Start()
    {
        // Al volver a casa desde la ciudad, el tutorial no se repite
        if (playerProgress != null && playerProgress.tutorialCompleto)
        {
            Completo = true;
            if (panelTutorial) panelTutorial.SetActive(false);
            return;
        }

        if (playerProgress == null)
            Debug.LogWarning("[Tutorial] Falta asignar Player Progress: el tutorial se repetirá al volver a casa.", this);

        if (pasos.Count == 0) { Terminar(); return; }
        EventosJuego.Emitir("tutorial_inicio");
        Mostrar();
    }

    void AlEvento(string evento, string detalle)
    {
        if (Completo || indice >= pasos.Count) return;
        if (evento != pasos[indice].eventoEsperado) return;
        indice++;
        EventosJuego.Emitir("tutorial_paso", indice.ToString());
        if (indice >= pasos.Count) Terminar();
        else Mostrar();
    }

    void Mostrar()
    {
        if (panelTutorial) panelTutorial.SetActive(true);
        if (textoTutorial) textoTutorial.text = pasos[indice].texto;
    }

    void Terminar()
    {
        Completo = true;
        Debug.Log("[Tutorial] Terminado");
        if (playerProgress != null) playerProgress.tutorialCompleto = true;
        if (panelTutorial) panelTutorial.SetActive(false);
        EventosJuego.Emitir("tutorial_fin");

    }
}