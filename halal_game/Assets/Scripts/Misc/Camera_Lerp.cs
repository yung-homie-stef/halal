using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Lerp : MonoBehaviour
{
    private Camera lerpedCamera;
    private bool hasSwapped = false;
    private Quaternion originalRotation;
    private float timeCount = 0.0f;

    public float lerpSpeed = 0.25f;
    public Vector3 newCameraPosition = Vector3.zero;
    public Vector3 newCameraRotation = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (hasSwapped)
        {
            lerpedCamera.transform.position = Vector3.Lerp(lerpedCamera.transform.position, newCameraPosition, lerpSpeed * Time.deltaTime);
            // Save the original rotation

            Quaternion newQuaternion = Quaternion.Euler(newCameraRotation.x, newCameraRotation.y, newCameraRotation.z);
            // Do your stuff here
            lerpedCamera.transform.rotation = Quaternion.Lerp(originalRotation, newQuaternion, timeCount);
            timeCount = timeCount + Time.deltaTime;
        }
    }

    public void LerpCamera(Camera cam)
    {
        lerpedCamera = cam;
        lerpedCamera.transform.parent = null;
        hasSwapped = true;
        originalRotation = cam.transform.rotation;
        

    }
}
