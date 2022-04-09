using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour
{
    public Camera playerCamera;
    public RaycastHit playerRaycastHit;
    public GameObject shotgun = null;

    private Vector3 _bulletPoint;
    private Vector3 _bulletDirection;
    [SerializeField]
    private float range = 0.0f;
    private RaycastHit _killedObject;
    private bool _canShoot = true;
    private Animator _animator = null;

    Kill _killScript = null;
    Destructible _destructibleScript = null;

    private void Start()
    {
        _animator = shotgun.GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_canShoot)
            {
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        if (!Narrator.gameNarrator.chatting)
        {
            _animator.SetTrigger("shoot");
            _canShoot = false;
            _bulletDirection = playerCamera.transform.forward;

            RaycastHit _hit;
            if (Physics.Raycast(playerCamera.transform.position, _bulletDirection, out _hit, range))
            {
                if (_hit.transform.GetComponent<Kill>())
                {
                    _killedObject = _hit;
                    _bulletPoint = _hit.point;

                    _killScript = _killedObject.transform.gameObject.GetComponent<Kill>();
                    _killScript.CatchHotOnes(_bulletPoint, _bulletDirection);
                }
                else if (_hit.transform.GetComponent<Destructible>())
                {
                    _killedObject = _hit;
                    _bulletPoint = _hit.point;
                    _destructibleScript = _killedObject.transform.gameObject.GetComponent<Destructible>();
                    _destructibleScript.DestroyMesh(_bulletPoint, _bulletDirection);

                }
            }

            StartCoroutine(Reload(1.16f));
        }
        
    }

    private IEnumerator Reload(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _canShoot = true;
    }
        
}
