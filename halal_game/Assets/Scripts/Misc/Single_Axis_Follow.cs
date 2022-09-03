using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single_Axis_Follow : MonoBehaviour
{
    public Transform target;
    public float targetOffset = 0.0f;

    // Update is called once per frame
    void Update()
    {

        transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z + targetOffset);
    }
}
