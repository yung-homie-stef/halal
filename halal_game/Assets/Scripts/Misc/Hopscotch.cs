using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;


public class Hopscotch : MonoBehaviour
{
    public GameObject hopscotch = null;
    public FPSController player = null;

    private Animator _animator = null;
    [SerializeField]
    private float triggerDistance = 0.0f;

    private void Start()
    {
        _animator = hopscotch.GetComponent<Animator>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(player.transform.position, gameObject.transform.position);
        Debug.Log(distance);

        if (distance < triggerDistance)
            _animator.SetTrigger("lift");
        else
            _animator.SetTrigger("drop");
    }
}
