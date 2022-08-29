using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;

public class Camera_Lerp : MonoBehaviour
{
    private FPSController lerpedCharacter;
    private bool hasSwapped = false;
    private Quaternion originalRotation;
    private float timeCount = 0.0f;
    private GameObject childCamera;

    public float lerpSpeed = 0.25f;
    public Transform newPlayerTransform;
    public Vector3 newCameraRotation = Vector3.zero;
    public Vector3 otherNewCameraRotation = Vector3.zero;

    private Quaternion originalCameraRotation;

    // Update is called once per frame
    void Update()
    {
        if (hasSwapped)
        {
            lerpedCharacter.transform.position = Vector3.Lerp(lerpedCharacter.transform.position, newPlayerTransform.position, lerpSpeed * Time.deltaTime);
            // Save the original rotation

            Quaternion newQuaternion = Quaternion.Euler(newCameraRotation.x, newCameraRotation.y, newCameraRotation.z);
            Quaternion otherNewQuaternion = Quaternion.Euler(newCameraRotation.x, newCameraRotation.y, newCameraRotation.z);

            // Do your stuff here
            lerpedCharacter.transform.rotation = Quaternion.Lerp(originalRotation, newQuaternion, timeCount);

            childCamera.transform.rotation = Quaternion.Lerp(originalCameraRotation, otherNewQuaternion, timeCount);

            timeCount = timeCount + Time.deltaTime;
        }
    }

    public void LerpCamera(FPSController controller)
    {
        controller.gravity = 0;
        lerpedCharacter = controller;
        lerpedCharacter.transform.parent = null;
        originalRotation = controller.transform.rotation;

        childCamera = controller.transform.GetChild(0).gameObject;
        originalCameraRotation = childCamera.transform.rotation;

        hasSwapped = true;

    }
}
