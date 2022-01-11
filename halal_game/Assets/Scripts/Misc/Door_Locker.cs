using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Locker : MonoBehaviour
{
    public Open_Interaction openable;
    private Animator _animator;

    private void Start()
    {
        _animator = openable.gameObject.GetComponent<Animator>();
    }

    public void CloseOrOpenDoor(bool enabled, string trigger)
    {
        openable.enabled = enabled;

        _animator.SetTrigger(trigger);
    }
}
