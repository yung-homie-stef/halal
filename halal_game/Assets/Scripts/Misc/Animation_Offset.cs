using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Offset : MonoBehaviour
{
    [SerializeField]
    private float minSpeed = 0.0f;
    [SerializeField]
    private float maxSpeed = 0.0f;

    public bool hasSpeed = false;

    void Start()
    {
        GetComponent<Animator>().SetFloat("offset", Random.Range(0.0f, 1.0f));

        if (hasSpeed)
        GetComponent<Animator>().SetFloat("speed", Random.Range(minSpeed, maxSpeed));
    }
}
