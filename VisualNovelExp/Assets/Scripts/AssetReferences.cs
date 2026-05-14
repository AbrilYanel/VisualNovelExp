using UnityEngine;

public class AssetReferences : MonoBehaviour
{
    public EntrevistaData entrevistaData;

    void Awake()
    {
        // Pasárselo al Manager_Entrevista si no lo tiene
        Manager_Entrevista manager = FindObjectOfType<Manager_Entrevista>();
        if (manager != null && manager.entrevistaData == null)
        {
            manager.entrevistaData = entrevistaData;
            Debug.Log("EntrevistaData inyectado desde ForzarAssets");
        }
    }
}