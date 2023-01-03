using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMPlayerMovement : MonoBehaviour
{

    public float Speed;
    public float RotationSpeed;
    public float JumpSpeed;

    CharacterController charController;

    float ySpeed;
    float originalStepOffset;

    void Start()
    {
        charController = GetComponent<CharacterController>();
        Debug.Log("char controller "+ charController);
    }

    // Update is called once per frame
    void Update()
    {
        float inputHor = Input.GetAxis("Horizontal");
        float inputVert = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(inputHor, 0, inputVert);
        float magnitude = Mathf.Clamp01(moveDirection.magnitude) * Speed;
        moveDirection.Normalize();

        //adjust gravity for jump
        ySpeed += Physics.gravity.y * Time.deltaTime;

        if (charController.isGrounded)
        {
            charController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;

            if (Input.GetButtonDown("Jump"))
            {
                ySpeed = JumpSpeed;
            }
        }
        else
        {
            charController.stepOffset = 0;
        }

        Vector3 velocity = moveDirection * magnitude;
        velocity.y = ySpeed;

        charController.Move(velocity * Time.deltaTime);

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, RotationSpeed * Time.deltaTime);
        }
    }
}
