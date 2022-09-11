using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single_Axis_Follow : MonoBehaviour
{
    public Transform target;
    public float targetOffset = 0.0f;

    [SerializeField]
    private bool locked = true;

    // Update is called once per frame
    void Update()
    {
        if (locked)
        transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z + targetOffset);
    }

    public void SetFollowStatus(bool flag)
    {
        locked = flag;
    }
}
