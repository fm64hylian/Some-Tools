using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMPlayerController : MonoBehaviour
{
    public Vector3 StartPosition;
    public FMInventorySlot[] Slots;
    Rigidbody rb;
    float speed = 6f;
    float maxVelocity = 4f;
    Vector3 inputVector;
    float jumpForce = 0.6f;
    //bool isJumping;
    Collider playerCollider;
    void Start()
    {
        //isJumping = false;
        playerCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        inputVector = new Vector3(Input.GetAxis("Horizontal")* speed,
            rb.velocity.y, Input.GetAxis("Vertical")* speed);
        transform.LookAt(transform.position + new Vector3(inputVector.x, 0 , inputVector.z));
        rb.velocity = inputVector * speed;

        //jump
        if (Input.GetKey(KeyCode.Z)){// && isJumping) {            
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            //rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
            //isJumping = true;
        }

        //if player falls
        if (transform.position.y < -3f) {
            ResetPosition();            
        }
    }

    private void FixedUpdate(){
        rb.velocity = inputVector;
        //rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }

    bool isGround()
    {
        //TODO NOT working for whatever reason
        return Physics.Raycast(transform.position, Vector3.down, 0.6f, LayerMask.GetMask("ground"));
        //return Physics.Raycast(transform.position, Vector3.down, 0.6f, groundLayer);
    }

    public void ResetPosition() {
        rb.velocity = Vector3.zero;
        transform.position = StartPosition;
        //isJumping = false;

        gameObject.SetActive(true);
    }
}
