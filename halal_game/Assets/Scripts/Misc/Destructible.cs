using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField]
    private GameObject _destroyedObject;

    [SerializeField]
    private float _deathForceMultiplier = 0.0f;
    private Rigidbody[] _destructibleRigidBodies = null;


    public void DestroyMesh(Vector3 point = default(Vector3), Vector3 direction = default(Vector3))
    {
        Instantiate(_destroyedObject, transform.position, transform.rotation);
        _destructibleRigidBodies = _destroyedObject.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody body in _destructibleRigidBodies)
        {
            float bulletDistance = Vector3.Distance(body.position, point);
            float deathForceMultiplier = Mathf.Max(_deathForceMultiplier - (bulletDistance * 5), 0); // the closer the bullet to the body, the greater the force

            body.AddForceAtPosition((direction * deathForceMultiplier), point, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }
}
