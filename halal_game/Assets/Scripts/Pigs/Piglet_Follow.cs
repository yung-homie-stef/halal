using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;
using UnityEngine.Animations.Rigging;

public class Piglet_Follow : MonoBehaviour
{
    public enum PigletStates
    {
        Following,
        Wandering,
        Staring
    }

    public PigletStates currentPigletState = PigletStates.Wandering;

    private Wander1 _wander1Script = null;
    private SeekUnit _seekScript = null;
    private SteeringBasics _steerScript = null;
    private Animator _animator = null;
    private Transform target = null;

    private float distance = 0.0f;
    public float minimumFollowAmount = 0.0f;
    public float closestPossibleDistance = 0.0f;

    public GameObject piglet = null;
    public MultiAimConstraint _skullAim;

    // Start is called before the first frame update
    void Start()
    {
        _wander1Script = gameObject.GetComponent<Wander1>();
        _seekScript = gameObject.GetComponent<SeekUnit>();
        _steerScript = gameObject.GetComponent<SteeringBasics>();

        target = _seekScript.target;

        _animator = piglet.GetComponent<Animator>();
        _animator.SetTrigger("walking");
        _animator.SetFloat("offset", Random.Range(0.0f, 1.0f));
        gameObject.transform.localScale *= Random.Range(.75f, 1.15f);
    }

    private void Update()
    {
        distance = (Vector3.Distance(target.position, gameObject.transform.position));

        switch (currentPigletState)
        {
            case PigletStates.Following:
                {
                    FollowState(Time.deltaTime);
                    break;
                }
            case PigletStates.Wandering:
                {
                    WanderState(Time.deltaTime);
                    break;
                }
            case PigletStates.Staring:
                {
                    StaringState(Time.deltaTime);
                    break;
                }
        }
    }

    void AffectMultiAimWeight(bool inRadius, float deltaTime)
    {
        if (inRadius)
            _skullAim.weight += 1.0f * Time.deltaTime;
        else
            _skullAim.weight -= 0.5f * Time.deltaTime;
    }

    void FollowState(float deltatime)
    {
        _animator.SetTrigger("running");

        _wander1Script.enabled = false;
        _seekScript.enabled = true;
        _steerScript.maxVelocity = 1.8f;

        if (distance < (minimumFollowAmount / 2))
        {
            AffectMultiAimWeight(true, Time.deltaTime);
        }
        else
        {
            AffectMultiAimWeight(false, Time.deltaTime);
        }

        if (distance < closestPossibleDistance)
        {
            currentPigletState = PigletStates.Staring;
        }
        else if (distance > minimumFollowAmount)
        {
            currentPigletState = PigletStates.Wandering;
        }
    }

    void WanderState(float deltatime)
    {
        _animator.SetTrigger("walking");

        _wander1Script.enabled = true;
        _seekScript.enabled = false;
        _steerScript.maxVelocity = 1.0f;

        if (distance < minimumFollowAmount)
        {
            currentPigletState = PigletStates.Following;
        }
    }

    void StaringState(float deltatime)
    {
        _animator.SetTrigger("breathing");

        _steerScript.maxVelocity = 0.0f;
        AffectMultiAimWeight(true, Time.deltaTime);

        if (distance > (closestPossibleDistance * 2))
        {
            currentPigletState = PigletStates.Following;
        }
    }
}
