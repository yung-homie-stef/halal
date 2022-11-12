using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Experimental.GlobalIllumination;

public class Shotgun : MonoBehaviour
{
    public Camera playerCamera;
    public RaycastHit playerRaycastHit;
    public GameObject shotgun = null;
    public AudioClip[] shellSounds;
    public AudioClip[] pumpSounds;
    public AudioClip[] shootSounds;

    public GameObject bloodAttach;
    public GameObject[] bloodFX;
    public Light directionalLight;

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


                    //LOGIC FOR SPAWNING BLOOD
                    int bloodFX_ID = Random.Range(0, bloodFX.Length);
                    float angle = Mathf.Atan2(_hit.normal.x, _hit.normal.z) * Mathf.Rad2Deg + 180;
                    var instance = Instantiate(bloodFX[bloodFX_ID], _hit.point, Quaternion.Euler(0, angle + 90, 0));
                    var settings = instance.GetComponent<BFX_BloodSettings>();
                    settings.LightIntensityMultiplier = directionalLight.intensity;

                    var nearestBone = GetNearestObject(_hit.transform.root, _hit.point);
                    if (nearestBone != null)
                    {
                        var attachBloodInstance = Instantiate(bloodAttach);
                        var bloodT = attachBloodInstance.transform;
                        bloodT.position = _hit.point;
                        bloodT.localRotation = Quaternion.identity;
                        bloodT.localScale = Vector3.one * Random.Range(0.75f, 1.2f);
                        bloodT.LookAt(_hit.point + _hit.normal, _bulletDirection);
                        bloodT.Rotate(90, 0, 0);
                        bloodT.transform.parent = nearestBone;
                        //Destroy(attachBloodInstance, 20);
                    }



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

    Transform GetNearestObject(Transform hit, Vector3 hitPos)
    {
        var closestPos = 100f;
        Transform closestBone = null;
        var childs = hit.GetComponentsInChildren<Transform>();

        foreach (var child in childs)
        {
            var dist = Vector3.Distance(child.position, hitPos);
            if (dist < closestPos)
            {
                closestPos = dist;
                closestBone = child;
            }
        }

        var distRoot = Vector3.Distance(hit.position, hitPos);
        if (distRoot < closestPos)
        {
            closestPos = distRoot;
            closestBone = hit;
        }
        return closestBone;
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
