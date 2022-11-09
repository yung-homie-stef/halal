using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField]
    private GameObject _destroyedObject;
    [SerializeField]
    private GameObject _intactObject;

    public AudioClip[] breakingSounds;

    [SerializeField]
    private float _deathForceMultiplier = 0.0f;
    private Rigidbody[] _destructibleRigidBodies = null;
    private BoxCollider _boxCollider = null;
    private AudioSource _audioSource = null;

    private void Start()
    {
        _boxCollider = gameObject.GetComponent<BoxCollider>();
        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void DestroyMesh(Vector3 point = default(Vector3), Vector3 direction = default(Vector3))
    {
        _destructibleRigidBodies = _destroyedObject.GetComponentsInChildren<Rigidbody>();
        _destroyedObject.SetActive(true);

        foreach (Rigidbody body in _destructibleRigidBodies)
        {
            float bulletDistance = Vector3.Distance(body.position, point);
            float deathForceMultiplier = Mathf.Max(_deathForceMultiplier - (bulletDistance * 5), 0); // the closer the bullet to the body, the greater the force

            body.AddForceAtPosition((direction * deathForceMultiplier), point, ForceMode.Impulse);
        }

        _intactObject.SetActive(false);
        _boxCollider.enabled = false;
        _audioSource.PlayOneShot(breakingSounds[Random.Range(0, breakingSounds.Length)]);
        //Destroy(gameObject);
    }
}
