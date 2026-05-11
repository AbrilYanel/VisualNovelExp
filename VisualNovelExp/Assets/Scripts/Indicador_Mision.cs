using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicador_Mision : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180f, 0); // corregir orientación
    }
}
