using UnityEngine;

[RequireComponent(typeof(CharacterLookRenderer))]
public class Retrato_Personaje : MonoBehaviour
{
    void Start()
    {
        GetComponent<CharacterLookRenderer>().CargarGuardado();
    }
}