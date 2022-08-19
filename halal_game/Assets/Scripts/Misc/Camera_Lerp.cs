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

    public float lerpSpeed = 0.25f;
    public Vector3 newCameraPosition = Vector3.zero;
    public Vector3 newCameraRotation = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (hasSwapped)
        {
            lerpedCharacter.transform.position = Vector3.Lerp(lerpedCharacter.transform.position, newCameraPosition, lerpSpeed * Time.deltaTime);
            // Save the original rotation

            Quaternion newQuaternion = Quaternion.Euler(newCameraRotation.x, newCameraRotation.y, newCameraRotation.z);
            // Do your stuff here
            lerpedCharacter.transform.rotation = Quaternion.Lerp(originalRotation, newQuaternion, timeCount);
            timeCount = timeCount + Time.deltaTime;
        }
    }

    public void LerpCamera(FPSController controller)
    {
        lerpedCharacter = controller;
        lerpedCharacter.transform.parent = null;
        hasSwapped = true;
        originalRotation = controller.transform.rotation;
        

    }
}
