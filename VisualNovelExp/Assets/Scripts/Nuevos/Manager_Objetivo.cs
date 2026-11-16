using System.Linq;
using UnityEngine;
using TMPro;

public class Manager_Objetivo : MonoBehaviour
{
    public TextMeshProUGUI textoObjetivo;
    public PlayerProgress playerProgress;
    public string textoSinObjetivos = "explora el barrio o descansa en casa";

    Interaccion_NPC[] npcs;
    float proximaRevision;

    void Start()
    {
        npcs = FindObjectsOfType<Interaccion_NPC>()
            .Where(n => n.cuentaComoObjetivo)
            .OrderBy(n => n.orden)
            .ToArray();
    }

    void Update()
    {
        if (Time.time < proximaRevision) return;
        proximaRevision = Time.time + 0.5f;
        if (textoObjetivo) textoObjetivo.text = "Objetivo: " + Calcular();
    }

    string Calcular()
    {
        var tutorial = Manager_Tutorial.I;
        if (tutorial != null && !tutorial.Completo) return "sigue las indicaciones de la casa";

        var nec = Manager_Necesidades.I;
        if (nec != null)
        {
            float u = Manager_Necesidades.UMBRAL_BAJO;
            bool hayComida = playerProgress != null && playerProgress.comida > 0;
            if (nec.hambre < u) return hayComida ? "tienes hambre: come en casa" : "tienes hambre: compra comida en la konbini";
            if (nec.energia < u) return "tienes sueño: duerme en tu cama";
            if (nec.diversion < u) return "estás aburrido: usa la PC o compra una revista";
        }

        foreach (var n in npcs)
        {
            bool habilitado = playerProgress == null || playerProgress.PuedeInteractuar(n.nivelRequerido);
            if (!n.Completado && habilitado) return "habla con " + n.nombreVisible;
        }
        return textoSinObjetivos;
    }
}