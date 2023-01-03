using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMRotateObject : MonoBehaviour
{
    public float Speed;
    public Vector3 RotationAxis;
    /// <summary>
    /// true for right, false for left
    /// </summary>
    public bool ReverseRot = false;
    public bool isRotating = true;


    // Update is called once per frame
    void Update()
    {
        if (isRotating) {
            int orientation = ReverseRot ? 1 : -1;
            //float rotSpeed = Speed * Time.deltaTime;
            transform.Rotate(RotationAxis, Speed * orientation);
        }
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
    }
}
