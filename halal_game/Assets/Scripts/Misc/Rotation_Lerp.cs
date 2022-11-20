using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Rotation_Lerp : MonoBehaviour
{
    public GameObject objectToLerp;
    public Vector3 newRotation;
    public Vector3 vectorAddition;

    private Vector3 newPosition;
    private bool hasTurned = false;
    private float timeCount = 0.0f;
    private Quaternion originalRotation;
    private Quaternion newQuaternion;

    // Start is called before the first frame update
    void Start()
    {
        newQuaternion = Quaternion.Euler(newRotation.x, newRotation.y, newRotation.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTurned)
        {
            objectToLerp.transform.rotation = Quaternion.Lerp(originalRotation, newQuaternion, timeCount);
            objectToLerp.transform.position = Vector3.Lerp(objectToLerp.transform.position, newPosition, 2 * Time.deltaTime);
            timeCount = timeCount + Time.deltaTime;
        }
    }

    public void BeginLerping()
    {
        originalRotation = objectToLerp.transform.rotation;
        newPosition = objectToLerp.transform.position + vectorAddition;
        hasTurned = true;
    }
}
