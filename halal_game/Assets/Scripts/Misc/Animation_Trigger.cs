using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Trigger : MonoBehaviour
{
    public Animator _animator = null;
    public string animationTrigger = null;

    private void OnTriggerEnter(Collider other)
    {
        _animator.SetTrigger(animationTrigger);
    }
}
