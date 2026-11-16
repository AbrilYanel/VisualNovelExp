using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Player_Camera : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public Transform playerBody;

    float xRotation = 0f;


    [Header("Interaccion")]
    public float interactDistance = 3f;
    

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.None) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);



        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        if (EstadoUI.Bloqueada) return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            var objetivo = hit.collider.GetComponentInParent<IInteractuable>();
            if (objetivo != null) objetivo.Interact();
        }
    }



}