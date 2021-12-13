using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class Piglet_Flee : MonoBehaviour
{
    public enum PigletStates
    {
        Wandering,
        Fleeing
    }

    PigletStates currentPigletState = PigletStates.Wandering;

    private Flee _fleeScript = null;
    private FleeUnit _fleeUnitScript = null;
    private SteeringBasics _steerScript = null;
    private Wander2 _wander2Script = null;
    private Animator _animator = null;
    private Transform target = null;

    private float distance = 0.0f;
    private float _fleeDistance = 0.0f;

    public GameObject piglet = null;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        _fleeScript = gameObject.GetComponent<Flee>();
        _fleeUnitScript = gameObject.GetComponent<FleeUnit>();
        _steerScript = gameObject.GetComponent<SteeringBasics>();
        _wander2Script = gameObject.GetComponent<Wander2>();

        _animator = piglet.GetComponent<Animator>();
        _animator.SetTrigger("walking");
        _animator.SetFloat("offset", Random.Range(0.0f, 1.0f));

        gameObject.transform.localScale *= Random.Range(.75f, 1.15f);
        _fleeUnitScript.target = player.transform;

        target = _fleeUnitScript.target;

        _fleeDistance = _fleeScript.panicDist;
    }

    // Update is called once per frame
    void Update()
    {
        distance = (Vector3.Distance(target.position, gameObject.transform.position));

        switch (currentPigletState)
        {
            case PigletStates.Wandering:
                {
                    WanderState(Time.deltaTime);
                    break;
                }
            case PigletStates.Fleeing:
                {
                    FleeState(Time.deltaTime);
                    break;
                }
        }
    }

    void FleeState(float deltatime)
    {
        _animator.SetTrigger("running");
        _steerScript.maxVelocity = 4;

        if (distance > ((_fleeDistance) * 5)) // if pig gets too far
        {
            currentPigletState = PigletStates.Wandering;
        }
    }

    void WanderState(float deltatime)
    {
        _animator.SetTrigger("walking");

        _wander2Script.enabled = true;
        _steerScript.maxVelocity = 1.0f;

        if (distance < _fleeDistance)
        {
            currentPigletState = PigletStates.Fleeing;
        }
    }
}
