using UnityEngine;

public class Luz_Nocturna : MonoBehaviour
{
    public Light[] luces;
    [Tooltip("Marcado: se encienden desde la tarde; desmarcado: solo de noche")]
    public bool encenderEnTarde = true;

    void Update()
    {
        var d = Manager_Dia.I;
        bool encender = d != null && (d.EsDeNoche ||
            (encenderEnTarde && d.playerProgress != null && d.playerProgress.franjasUsadas >= 2));

        foreach (var l in luces)
            if (l && l.enabled != encender) l.enabled = encender;
    }
}