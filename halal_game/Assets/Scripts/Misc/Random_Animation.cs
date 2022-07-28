using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Random_Animation : MonoBehaviour
{
    public string[] animatorTriggerList;

    private Animator _animator = null;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.SetTrigger(animatorTriggerList[Random.Range(0, animatorTriggerList.Length)]);

    }

    
}
