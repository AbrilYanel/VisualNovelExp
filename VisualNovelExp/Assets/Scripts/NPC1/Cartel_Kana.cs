using UnityEngine;

// Poner en cada cartel del mundo (wa, wo, n)
// Requiere un Collider con IsTrigger + tag "Player" o interacción por Interaccion_NPC
public class Cartel_Kana : MonoBehaviour
{
    [Header("Kana que desbloquea")]
    public string kanaId = "wa"; // "wa", "wo", "n"
    public string nombreMostrar = "わ (wa)";

    [Header("Refs")]
    public KanaInventario kanaInventario;
    public Manager_LimpiezaCartel managerLimpieza; // UI manager

    [Header("Estado")]
    public bool yaDesbloqueado = false;
    public GameObject indicadorInteractuable; // ej: ícono E
    public GameObject versionLimpia;  // mesh / sprite limpio
    public GameObject versionSucia;   // mesh / sprite sucio

    void Start()
    {
        if (kanaInventario != null)
            yaDesbloqueado = kanaInventario.EstaDesbloqueado(kanaId);
        ActualizarVisual();
    }

    public void Interact()
    {
        if (yaDesbloqueado)
        {
            Debug.Log($"[Cartel] {kanaId} ya estaba desbloqueado");
            return;
        }
        if (managerLimpieza == null)
        {
            Debug.LogError("[Cartel_Kana] managerLimpieza no asignado");
            // desbloqueo directo de emergencia
            Desbloquear();
            return;
        }
        managerLimpieza.IniciarLimpieza(this);
    }

    // Llamado por Manager_LimpiezaCartel al completar
    public void Desbloquear()
    {
        if (yaDesbloqueado) return;
        yaDesbloqueado = true;
        if (kanaInventario != null)
            kanaInventario.Desbloquear(kanaId);
        Debug.Log($"¡Kana desbloqueado por cartel! {kanaId} – {nombreMostrar}");
        ActualizarVisual();

        // feedback opcional
        var journal = FindObjectOfType<Manager_Journal>();
        if (journal != null)
        {
            journal.RegistrarPalabras(new System.Collections.Generic.List<PalabraAprendida>{
                new PalabraAprendida{
                    hiragana = nombreMostrar,
                    romaji = kanaId,
                    traduccion = "Partícula / kana especial",
                    idFuente = "cartel_"+kanaId
                }
            });
        }
    }

    void ActualizarVisual()
    {
        if (versionLimpia) versionLimpia.SetActive(yaDesbloqueado);
        if (versionSucia) versionSucia.SetActive(!yaDesbloqueado);
        if (indicadorInteractuable) indicadorInteractuable.SetActive(!yaDesbloqueado);
    }

    // Integración simple con Interaccion_NPC si querés reusar el sistema E
    void OnTriggerEnter(Collider other)
    {
        if (yaDesbloqueado) return;
        if (other.CompareTag("Player") && indicadorInteractuable) indicadorInteractuable.SetActive(true);
    }
    void OnTriggerExit(Collider other)
    {
        if (indicadorInteractuable) indicadorInteractuable.SetActive(!yaDesbloqueado && false);
    }
    void Update()
    {
        // Interacción simple con E si el jugador está cerca (opcional)
        // Mejor: llamá cartel.Interact() desde tu sistema de interacción existente
    }
}
