using UnityEngine;
using UnityEngine.UI;

public enum Necesidad { Hambre, Energia, Diversion }

public class Manager_Necesidades : MonoBehaviour
{
    public static Manager_Necesidades I;
    public const float UMBRAL_BAJO = 25f;
    public const float UMBRAL_ALTO = 100f;

    [Tooltip("Los valores viven en el asset para sobrevivir al cambio de escena")]
    public PlayerProgress playerProgress;

    [Header("Caída por segundo")]
    public float caidaHambre = 0.15f;
    public float caidaEnergia = 0.10f;
    public float caidaDiversion = 0.13f;

    bool ceroHambre, ceroEnergia, ceroDiversion;

    public float hambre => playerProgress ? playerProgress.hambre : 100f;
    public float energia => playerProgress ? playerProgress.energia : 100f;
    public float diversion => playerProgress ? playerProgress.diversion : 100f;

    void Awake() { I = this; }

    void Update()
    {
        if (EstadoUI.Bloqueada || playerProgress == null) return;
        float dt = Time.deltaTime;
        playerProgress.hambre = Mathf.Max(0f, playerProgress.hambre - caidaHambre * dt);
        playerProgress.energia = Mathf.Max(0f, playerProgress.energia - caidaEnergia * dt);
        playerProgress.diversion = Mathf.Max(0f, playerProgress.diversion - caidaDiversion * dt);
        AvisarCero(hambre, ref ceroHambre, "hambre");
        AvisarCero(energia, ref ceroEnergia, "energia");
        AvisarCero(diversion, ref ceroDiversion, "diversion");
    }

    void AvisarCero(float valor, ref bool avisado, string nombre)
    {
        if (valor <= 0f && !avisado) { avisado = true; EventosJuego.Emitir("necesidad_cero", nombre); }
        if (valor > 0f) avisado = false;
    }

    public void Sumar(Necesidad n, float cantidad)
    {
        if (playerProgress == null) return;
        if (n == Necesidad.Hambre) playerProgress.hambre = Mathf.Clamp(playerProgress.hambre + cantidad, 0f, 100f);
        if (n == Necesidad.Energia) playerProgress.energia = Mathf.Clamp(playerProgress.energia + cantidad, 0f, 100f);
        if (n == Necesidad.Diversion) playerProgress.diversion = Mathf.Clamp(playerProgress.diversion + cantidad, 0f, 100f);
        EventosJuego.Emitir("necesidad_sube", n + ":" + cantidad);
    }

    public void ActualizarBarras(Slider h, Slider e, Slider d)
    {
        if (h) h.value = hambre / 100f;
        if (e) e.value = energia / 100f;
        if (d) d.value = diversion / 100f;
    }

    public float MultiplicadorVelocidad => (hambre <= 0f || energia <= 0f) ? 0.5f : 1f;
    public bool TodoAlto => hambre >= 75f && energia >= 75f && diversion >= 75f;
    public float MultiplicadorMonedas => diversion <= 0f ? 0.5f : (TodoAlto ? 1.5f : 1f);


}