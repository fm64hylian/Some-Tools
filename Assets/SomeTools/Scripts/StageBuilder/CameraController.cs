using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraOrientation
{
    North,
    East,
    South,
    West
}
public class CameraController : MonoBehaviour
{
    [SerializeField]
    SelectionController selection;

    float speed = 15f; //25
    Quaternion originalRotation;
    Vector3 centerPoint;
    Camera cam;
    float angle;
    float targetAngle;
    float distance;

    private void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
        transform.position = cam.transform.position;
        centerPoint = selection.transform.position;

        originalRotation = transform.rotation;
        angle = 45;
        targetAngle = angle;
        distance = Vector3.Distance(centerPoint, transform.position);
    }

    private void LateUpdate()
    {
        centerPoint = selection.transform.position;

        //rotate
        Rotate();

        //zoom
        SimpleZoom();

        //reset camera
        if (Input.GetKey(KeyCode.R))
        {
            ResetCamera();
            return;
        }

        transform.position = centerPoint;
        transform.rotation = Quaternion.Euler(new Vector3(45f, angle, 0f));
        transform.position -= transform.forward * distance;
        CheckOrientation();
    }

    void Rotate()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetAngle += 90;
            //targetAngle = targetAngle > 360 ? targetAngle - 360 : targetAngle;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            targetAngle -= 90;
            //targetAngle = targetAngle < -360 ? targetAngle + 360 : targetAngle;
        }
        angle = Mathf.Lerp(angle, targetAngle, Time.deltaTime * speed);
    }

    void ResetCamera()
    {
        angle = 45f;
        targetAngle = 45f;
        centerPoint = selection.transform.position;

        transform.position = centerPoint;
        transform.eulerAngles = new Vector3(45f, 45f, 0f);
        transform.rotation = originalRotation;
        transform.position -= transform.forward * 45;

        distance = Vector3.Distance(centerPoint, transform.position);
    }

    void SimpleZoom()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        distance += scroll * -speed;
        distance = Mathf.Clamp(distance, 15f, 100f);
    }

    /// <summary>
    /// camera angles of -45 and 45 will be default orientation
    /// camera angles of -135 and 135 will be inverted orientation (invert arrow keys)
    /// </summary>
    public void CheckOrientation()
    {
        switch (Mathf.RoundToInt(transform.eulerAngles.y))
        {
            case 45:
                selection.UpdateOrientation(CameraOrientation.North);
                break;
            case 315:
                selection.UpdateOrientation(CameraOrientation.West);
                break;
            case 135:
                selection.UpdateOrientation(CameraOrientation.East);
                break;
            case 225:
                selection.UpdateOrientation(CameraOrientation.South);
                break;
        }
    }
}
