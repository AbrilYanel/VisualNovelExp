using System.Linq;
using UnityEngine;
using TMPro;

public class Manager_Objetivo : MonoBehaviour
{
    public TextMeshProUGUI textoObjetivo;
    public PlayerProgress playerProgress;
    public string textoSinObjetivos = "explora el barrio o descansa en casa";
    public string nombreDirector = "Director";
    public TextMeshProUGUI textoDia;

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
        string prefijo = Manager_Dia.I ? $"Día {Manager_Dia.I.Dia} · {Manager_Dia.I.Franja}" : "";
        if (textoObjetivo) textoObjetivo.text = "<b>Objetivo</b> \n" + Calcular();
        if (textoDia) textoDia.text = prefijo;
    }

    string Calcular()
    {
        var tutorial = Manager_Tutorial.I;
        if (tutorial != null && !tutorial.Completo) return "Sigue las indicaciones de la casa";

        var dia = Manager_Dia.I;
        if (dia != null && dia.EsDeNoche) return "es de noche: vuelve a casa a dormir";

        var nec = Manager_Necesidades.I;
        if (nec != null)
        {
            float u = Manager_Necesidades.UMBRAL_BAJO;
            bool hayComida = playerProgress != null && playerProgress.comida > 0;
            if (nec.hambre < u) return hayComida ? "tienes hambre: come en casa" : "tienes hambre: compra comida en la tienda";
            if (nec.energia < u) return "tienes sueño: duerme en tu cama";
            if (nec.diversion < u) return "estás aburrido: usa la PC o compra una revista";
        }

        bool festivalPendiente = playerProgress != null && !playerProgress.festivalCompletado;
        if (festivalPendiente && dia != null && playerProgress.dia >= dia.diaDelFestival)
            return "¡es el festival! lleva la entrevista al " + nombreDirector;

        foreach (var n in npcs)
        {
            bool habilitado = playerProgress == null || playerProgress.PuedeInteractuar(n.nivelRequerido);
            if (!n.Completado && habilitado) return "habla con " + n.nombreVisible;
        }

        if (festivalPendiente) return "prepara la entrevista del festival con el " + nombreDirector;
        return textoSinObjetivos;
    }
}