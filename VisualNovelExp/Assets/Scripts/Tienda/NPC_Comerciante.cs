using UnityEngine;

public class NPC_Comerciante : MonoBehaviour
{
    [Header("Referencias")]
    public Manager_Tienda managerTienda;

    public void Interact()
    {
        if (managerTienda != null)
            managerTienda.AbrirTienda();
        else
            Debug.LogError("[NPC_Comerciante] managerTienda no asignado");
    }
}