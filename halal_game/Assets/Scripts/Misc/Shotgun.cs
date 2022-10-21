using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Shotgun : MonoBehaviour
{
    public Camera playerCamera;
    public RaycastHit playerRaycastHit;
    public GameObject shotgun = null;
    public AudioClip[] shellSounds;
    public AudioClip[] pumpSounds;
    public AudioClip[] shootSounds;

    private Vector3 _bulletPoint;
    private Vector3 _bulletDirection;
    [SerializeField]
    private float range = 0.0f;
    private RaycastHit _killedObject;
    private bool _canShoot = true;
    private Animator _animator = null;

    private AudioSource _audioSource = null;

    Kill _killScript = null;
    Destructible _destructibleScript = null;

    private void Start()
    {
        _animator = shotgun.GetComponent<Animator>();
        _audioSource = gameObject.GetComponent<AudioSource>();
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

    public void PlayShellShound()
    {
        _audioSource.PlayOneShot(shellSounds[Random.Range(0, shellSounds.Length)]);
    }

    public void PlayPumpSound()
    {
        _audioSource.PlayOneShot(pumpSounds[Random.Range(0, pumpSounds.Length)]);
    }

    public void PlayShootSound()
    {
        _audioSource.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
    }

    private IEnumerator Reload(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _canShoot = true;
    }
        
}
