using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale_Randomizer : MonoBehaviour
{
    public float maxValue = 0.0f;
    public float minValue = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.localScale = Vector3.one * (Random.Range(minValue, maxValue));
    }
}
