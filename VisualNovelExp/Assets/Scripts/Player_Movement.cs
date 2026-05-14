using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;

    public float gravedad = -9.81f;
    public float gravityMultiplier = 2f;
    private Vector3 velocidadVertical;

    private CharacterController controller;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        
        if (controller.isGrounded)
        {
            velocidadVertical.y = -2f;
        }
        else
        {
            velocidadVertical.y += gravedad * gravityMultiplier * Time.deltaTime;
        }

        controller.Move(move * speed * Time.deltaTime);
        controller.Move(velocidadVertical * Time.deltaTime);
    }
}
