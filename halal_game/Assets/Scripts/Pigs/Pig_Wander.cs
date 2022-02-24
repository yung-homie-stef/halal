using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class Pig_Wander : MonoBehaviour
{
    public enum PigStates
    {
        Wandering,
        Fleeing,
        Eating,
        Pissing,
        Dead
    }

    public PigStates currentPigStates = PigStates.Wandering;

    private Flee _fleeScript = null;
    private FleeUnit _fleeUnitScript = null;
    private SteeringBasics _steerScript = null;
    private Wander2 _wander2Script = null;
    private Animator _animator = null;
    private Transform target = null;

    private float distance = 0.0f;
    private float _fleeDistance = 0.0f;
    private float _wanderTimer = 10.0f;
    private float _eatTimer = 9.0f;
    private float _pissTimer = 6.0f;

    public float initialSpeed = 2.0f;
    
    public GameObject pig = null;
    public GameObject player = null;
    public GameObject urine;
   
    // Start is called before the first frame update
    void Start()
    {
        _fleeScript = gameObject.GetComponent<Flee>();
        _fleeUnitScript = gameObject.GetComponent<FleeUnit>();
        _steerScript = gameObject.GetComponent<SteeringBasics>();
        _wander2Script = gameObject.GetComponent<Wander2>();

        _animator = pig.GetComponent<Animator>();
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

        switch (currentPigStates)
        {
            case PigStates.Wandering:
                {
                    WanderState(Time.deltaTime);
                    break;
                }
            case PigStates.Fleeing:
                {
                    FleeState(Time.deltaTime);
                    break;
                }
            case PigStates.Eating:
                {
                    EatState(Time.deltaTime);
                    break;
                }
            case PigStates.Pissing:
                {
                    PissState(Time.deltaTime);
                    break;
                }
            case PigStates.Dead:
                {
                    DeadState();
                    break;
                }
        }
    }

    void WanderState(float deltatime)
    {
        _animator.ResetTrigger("eating");
        _animator.ResetTrigger("running");
        _animator.SetTrigger("walking");

        _wander2Script.enabled = true;
        _steerScript.maxVelocity = 1.0f;
        _wanderTimer -= Time.deltaTime;

        if (distance < _fleeDistance) // if player gets too close
        {
            currentPigStates = PigStates.Fleeing;
        }

        if (_wanderTimer <= 0) // when enough time has passed go to either the eating or pissing state
        {
            if (Random.Range(0, 2) == 0)
            {
                _eatTimer = (Random.Range(8, 11));
                currentPigStates = PigStates.Eating;
            }
            else
            {
                _pissTimer = (Random.Range(7, 11));
                currentPigStates = PigStates.Pissing;
            }
        }
    }

    void FleeState(float deltatime)
    {
        _animator.SetTrigger("running");
        _steerScript.maxVelocity = 4;

        if (distance > ((_fleeDistance) * 5)) // if pig gets too far
        {
            _wanderTimer = (Random.Range(9, 12));
            currentPigStates = PigStates.Wandering;
        }
    }

    void EatState(float deltatime)
    {
        _animator.SetTrigger("eating");
        _steerScript.maxVelocity = 0;
        _eatTimer -= Time.deltaTime;

        if (_eatTimer <= 0) // after eating long enough proceed to wander again
        {
            _animator.SetTrigger("full");
            _wanderTimer = (Random.Range(9, 12));
            currentPigStates = PigStates.Wandering;
        }
    }

    void PissState(float deltatime)
    {
        _animator.SetTrigger("pissing");
        urine.SetActive(true);
        _steerScript.maxVelocity = 0;
        _pissTimer -= Time.deltaTime;

        if (_pissTimer <= 0)
        {
            _wanderTimer = (Random.Range(9, 12));
            urine.SetActive(false);
            currentPigStates = PigStates.Wandering;
        }
    }

    void DeadState()
    {
        _fleeScript.enabled = false;
        _fleeUnitScript.enabled = false;
        _wander2Script.enabled = false;
        _steerScript.enabled = false;
    }

}
