using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KanaInventario", menuName = "Kanas/KanaInventario")]
public class KanaInventario : ScriptableObject
{
    public List<string> desbloqueadas = new List<string>();

    public bool EstaDesbloqueado(string id)
    {
        return desbloqueadas.Contains(id);
    }

    // Compra en la tienda: descuenta monedas de PlayerProgress
    public bool Comprar(KanaData kana, PlayerProgress progress)
    {
        if (kana == null || progress == null) return false;

        if (EstaDesbloqueado(kana.id))
        {
            Debug.Log(kana.nombreFila + " ya estaba desbloqueada.");
            return false;
        }

        if (!progress.GastarMonedas(kana.costo))
            return false;

        desbloqueadas.Add(kana.id);
        Debug.Log("Desbloqueaste: " + kana.nombreFila);
        return true;
    }

    // Desbloqueo sin costo, usado por los carteles del mapa (wa, wo, n)
    public void Desbloquear(string id)
    {
        if (!desbloqueadas.Contains(id))
            desbloqueadas.Add(id);
    }

    public void Reiniciar()
    {
        desbloqueadas.Clear();
    }
}