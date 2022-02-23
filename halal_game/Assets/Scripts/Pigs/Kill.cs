using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill : MonoBehaviour
{
    [SerializeField]
    private float _deathForceMultiplier = 0.0f;

    private Rigidbody[] _pigRigidBodies = null;
    private BoxCollider _boxCollider = null;
    private Animator _animator = null;

     void Start()
    {
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider>();
        _pigRigidBodies = GetComponentsInChildren<Rigidbody>();
    }
    public void CatchHotOnes(Vector3 point = default(Vector3), Vector3 direction = default(Vector3))
    {
        Die(point, direction);
    }

     private void Die(Vector3 point = default(Vector3), Vector3 direction = default(Vector3))
    {
        foreach (Rigidbody body in _pigRigidBodies)
        {
            float bulletDistance = Vector3.Distance(body.position, point);
            float deathForceMultiplier = Mathf.Max(_deathForceMultiplier - (bulletDistance * 5), 0); // the closer the bullet to the body, the greater the force

            body.isKinematic = false;
            body.AddForceAtPosition((direction * deathForceMultiplier), point, ForceMode.Impulse);
        }

        _boxCollider.enabled = false;
        _animator.enabled = false;
    }
}
