using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_Kill : MonoBehaviour
{
    [SerializeField]
    private float _deathForceMultiplier;
    private GameObject _contactPoint = null;
    private Rigidbody[] _pigRigidBodies = null;
    private BoxCollider _boxCollider = null;
    private Animator _animator;

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
            body.AddForceAtPosition((direction * _deathForceMultiplier), point, ForceMode.Impulse);
        }
        _boxCollider.enabled = false;
    }
}
