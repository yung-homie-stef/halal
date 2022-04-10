using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyRotation : MonoBehaviour
{
    // Start is called before the first frame update
    public Material skyboxMat = null;
    public float spinSpeed = 0.0f;
    public float initialRot = 0.0f;

    private void Start()
    {
        if (skyboxMat.HasProperty("_Rotation"))
        {
            skyboxMat.SetFloat("_Rotation", initialRot);
        }
    }

    void Update()
    {
        if (skyboxMat.HasProperty("_Rotation"))
        {
            skyboxMat.SetFloat("_Rotation", initialRot + (Time.time * spinSpeed));
        }
    }
}
