using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_Kill : MonoBehaviour
{
    [SerializeField]
    private float _deathForceMultiplier;
    [SerializeField]
    private Narration_Trigger _dialogueTrigger = null;

    public static int targetsNeededToKill = 0;

    private GameObject _contactPoint = null;
    private Rigidbody[] _pigRigidBodies = null;
    private BoxCollider _boxCollider = null;
    private Animator _animator = null;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider>();
        _pigRigidBodies = GetComponentsInChildren<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Weapon")
        {
            Debug.Log("ouch");
            _contactPoint = other.gameObject;
            Die(_contactPoint.transform.position, _contactPoint.transform.forward);
        }
    }

    private void Die(Vector3 point = default(Vector3), Vector3 direction = default(Vector3))
    {
        foreach (var body in _pigRigidBodies)
        {
            body.isKinematic = false;
            body.AddForceAtPosition(((direction + new Vector3(0.0f, 1.0f, 0.0f)) * _deathForceMultiplier), point, ForceMode.Impulse);
        }
        _boxCollider.enabled = false;
        _animator.enabled = false;
        _dialogueTrigger.gameObject.SetActive(true);

        if (gameObject.GetComponent<Door_Locker>() != null)
        {
            gameObject.GetComponent<Door_Locker>().CloseOrOpenDoor(false, "close");
        }

        targetsNeededToKill--;
        if (targetsNeededToKill == 0)
        {

        }
    }
}
