using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;


public class Hopscotch : MonoBehaviour
{
    public GameObject hopscotch = null;
    public FPSController player = null;

    private Animator _animator = null;

    private void Start()
    {
        _animator = hopscotch.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            _animator.SetTrigger("lift");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _animator.SetTrigger("drop");
    }

}
