using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class Sheep_Wander : MonoBehaviour
{
    public enum SheepStates
    {
        Wandering,
        Eating,
        Idling
    }

    public SheepStates currentSheepStates = SheepStates.Wandering;

    private SteeringBasics _steerScript = null;
    private Wander2 _wander2Script = null;
    private Animator _animator = null;

    private float _wanderTimer = 10.0f;
    private float _eatTimer = 9.0f;
    private float _idleTimer = 9.0f;

    public GameObject sheep = null;

    // Start is called before the first frame update
    void Start()
    {
        _steerScript = gameObject.GetComponent<SteeringBasics>();
        _wander2Script = gameObject.GetComponent<Wander2>();

        _animator = sheep.GetComponent<Animator>();
        _animator.SetFloat("offset", Random.Range(0.0f, 1.0f));

        gameObject.transform.localScale *= Random.Range(.75f, 1.15f);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentSheepStates)
        {
            case SheepStates.Wandering:
                {
                    WanderState(Time.deltaTime);
                    break;
                }
            case SheepStates.Idling:
                {
                    IdleState(Time.deltaTime);
                    break;
                }
            case SheepStates.Eating:
                {
                    EatingState(Time.deltaTime);
                    break;
                }
        }
    }

    void WanderState(float deltatime)
    {
        _animator.ResetTrigger("eating");
        _animator.ResetTrigger("idling");

        _wander2Script.enabled = true;
        _steerScript.maxVelocity = 1.0f;
        _wanderTimer -= Time.deltaTime;

        if (_wanderTimer <= 0) // when enough time has passed go to either the eating or pissing state
        {
            if (Random.Range(0, 2) == 0)
            {
                _eatTimer = (Random.Range(8, 11));
                currentSheepStates = SheepStates.Eating;
            }
            else
            {
                _idleTimer = (Random.Range(7, 11));
                currentSheepStates = SheepStates.Idling;
            }
        }

    }

    void IdleState(float deltatime)
    {
        _animator.SetTrigger("idling");
        _steerScript.maxVelocity = 0;
        _idleTimer -= Time.deltaTime;

        if (_idleTimer <= 0) // after eating long enough proceed to wander again
        {
            _animator.SetTrigger("full");
            _wanderTimer = (Random.Range(9, 12));
            currentSheepStates = SheepStates.Wandering;
        }

    }

    void EatingState(float deltatime)
    {
        _animator.SetTrigger("eating");
        _steerScript.maxVelocity = 0;
        _eatTimer -= Time.deltaTime;

        if (_eatTimer <= 0) // after eating long enough proceed to wander again
        {
            _animator.SetTrigger("full");
            _wanderTimer = (Random.Range(9, 12));
            currentSheepStates = SheepStates.Wandering;
        }

    }
}
