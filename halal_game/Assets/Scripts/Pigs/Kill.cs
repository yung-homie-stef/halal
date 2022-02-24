using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill : MonoBehaviour
{
    public GameObject pig = null;

    [SerializeField]
    private float _deathForceMultiplier = 0.0f;

    private Rigidbody[] _pigRigidBodies = null;
    private CapsuleCollider[] _pigCapsuleCOlliders = null;
    private SphereCollider _sphereCollider = null;
    private Animator _animator = null;
    private Pig_Wander _wanderScript = null;

     void Start()
    {
        _animator = pig.GetComponent<Animator>();
        _sphereCollider = GetComponent<SphereCollider>();
        _pigRigidBodies = pig.GetComponentsInChildren<Rigidbody>();
        _pigCapsuleCOlliders = pig.GetComponentsInChildren<CapsuleCollider>();
        _wanderScript = gameObject.GetComponent<Pig_Wander>();
    }
    public void CatchHotOnes(Vector3 point = default(Vector3), Vector3 direction = default(Vector3))
    {
        _wanderScript.currentPigStates = Pig_Wander.PigStates.Dead;
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

        foreach (CapsuleCollider cap in _pigCapsuleCOlliders)
        {
            cap.enabled = true;
        }

        pig.transform.parent = null;
        _animator.enabled = false;
        Destroy(gameObject);
    }
}
