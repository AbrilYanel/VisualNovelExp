using UnityEngine;

public class Cielo_Por_Franja : MonoBehaviour
{
    [Header("Skyboxes")]
    public Material cieloManana;
    public Material cieloMediodia;
    public Material cieloTarde;
    public Material cieloNoche;

    [Header("Luz del sol (opcional)")]
    [Tooltip("Si queda vacío, usa la Light de este mismo objeto")]
    public Light sol;
    public Color[] coloresLuz =
    {
        new Color(1f, 0.92f, 0.8f),    // mañana
        Color.white,                    // mediodía
        new Color(1f, 0.7f, 0.45f),    // tarde
        new Color(0.45f, 0.55f, 0.85f) // noche
    };
    public float[] intensidadesLuz = { 0.9f, 1.1f, 0.8f, 0.25f };

    [Header("Luz ambiente (ilumina las zonas en sombra)")]
    public bool controlarAmbiente = true;
    public Color[] coloresAmbiente =
    {
        new Color(0.75f, 0.72f, 0.68f), // mañana
        new Color(0.80f, 0.80f, 0.80f), // mediodía
        new Color(0.70f, 0.60f, 0.55f), // tarde
        new Color(0.32f, 0.36f, 0.50f)  // noche: azulada pero visible
    };

    int franjaAplicada = -1;

    void Awake()
    {
        if (sol == null) sol = GetComponent<Light>();
    }

    void Update()
    {
        int actual = FranjaActual();
        if (actual == franjaAplicada) return;   // solo actúa cuando cambia
        franjaAplicada = actual;
        Aplicar(actual);
    }

    // 0 = mañana, 1 = mediodía, 2 = tarde, 3 = noche
    int FranjaActual()
    {
        var dia = Manager_Dia.I;
        if (dia == null || dia.playerProgress == null) return 0;
        if (dia.EsDeNoche) return 3;
        return Mathf.Clamp(dia.playerProgress.franjasUsadas, 0, 2);
    }

    void Aplicar(int i)
    {
        Material[] cielos = { cieloManana, cieloMediodia, cieloTarde, cieloNoche };
        if (cielos[i] != null)
        {
            RenderSettings.skybox = cielos[i];
            DynamicGI.UpdateEnvironment(); // actualiza la luz ambiente según el nuevo cielo
        }

        if (sol != null)
        {
            if (i < coloresLuz.Length) sol.color = coloresLuz[i];
            if (i < intensidadesLuz.Length) sol.intensity = intensidadesLuz[i];
        }

        if (controlarAmbiente && i < coloresAmbiente.Length)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = coloresAmbiente[i];
        }
    }
}