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
        offset = (Vector3.back * 8f) + (Vector3.up * 6f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() //LateUpdate
    {
        //rotate
        if (Input.GetKey(KeyCode.A)) {
            transform.RotateAround(target.transform.position, Vector3.up, 4f);
            return;
        }

        if (Input.GetKey(KeyCode.D)) {
            transform.RotateAround(target.transform.position, Vector3.down, 4f);
            return;
        }

        Vector3 smoothedPos = Vector3.Lerp(transform.position, target.position + offset ,smoothSpeed); //Time.deltaTime
        transform.position = smoothedPos;
        transform.LookAt(target);
    }
}
