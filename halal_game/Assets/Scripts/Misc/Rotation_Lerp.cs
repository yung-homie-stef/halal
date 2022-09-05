using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Rotation_Lerp : MonoBehaviour
{
    public GameObject objectToLerp;
    public Vector3 newRotation;
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
            timeCount = timeCount + Time.deltaTime;
        }
    }

    public void BeginLerping()
    {
        originalRotation = objectToLerp.transform.rotation;
        hasTurned = true;
    }
}
