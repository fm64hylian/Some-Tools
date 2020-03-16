using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    float speed = 6f;
    Vector3 inputVetor;
    bool isJumping;
    void Start()
    {
        isJumping = false;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        inputVetor = new Vector3(Input.GetAxis("Horizontal")* speed,
            rb.velocity.y, Input.GetAxis("Vertical")* speed);
        transform.LookAt(transform.position + new Vector3(inputVetor.x, 0 , inputVetor.z));
        rb.velocity = inputVetor * speed;

        if (Input.GetKey(KeyCode.Z)) {
            rb.AddForce(Vector3.up * 20f, ForceMode.VelocityChange);
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = inputVetor;
    }
}
