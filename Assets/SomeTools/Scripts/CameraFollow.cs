using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    Transform target;

    float smoothSpeed = 0.15f;
    Vector3 offset;

    void Start()
    {
        //offset = (Vector3.back * 8f) + (Vector3.up * 6f);
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
        offset = transform.position - target.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() //LateUpdate
    {
        ////rotate
        //if (Input.GetKey(KeyCode.A)) {
        //    transform.RotateAround(target.transform.position, Vector3.up, 4f);
        //    return;
        //}

        //if (Input.GetKey(KeyCode.D)) {
        //    transform.RotateAround(target.transform.position, Vector3.down, 4f);
        //    return;
        //}

        //if (Vector3.Distance(transform.position, target.position) > 5f) {
        //    Vector3 smoothedPos = Vector3.Lerp(transform.position, target.position + offset, smoothSpeed); //Time.deltaTime
        //    transform.position = smoothedPos;
        //    transform.LookAt(target);
        //}

    }

    private void LateUpdate()
    {
        transform.position = target.transform.position + offset;
    }
}
