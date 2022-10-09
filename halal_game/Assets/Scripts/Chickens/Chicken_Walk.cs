using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;
using static Pig_Wander;

public class Chicken_Walk : MonoBehaviour
{
    public enum ChickenStates
    {
        Pecking,
        Looking,
        Walking
    }

    public ChickenStates currentChickenState = ChickenStates.Walking;
    public GameObject chicken;

    private Animator _animator = null;
    private SteeringBasics _steerScript = null;
    private Wander2 _wander2Script = null;

    private float _chickenWalkTimer = 0.0f;
    private float _lookTimer = 4.0f;
    private float _peckTimer = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        _animator = chicken.GetComponent<Animator>();

        _steerScript = gameObject.GetComponent<SteeringBasics>();
        _wander2Script = gameObject.GetComponent<Wander2>();

        _chickenWalkTimer = Random.Range(5.0f, 12.0f);

        _animator.SetTrigger("walking");
        _animator.SetFloat("offset", Random.Range(0.0f, 1.0f));

        gameObject.transform.localScale *= Random.Range(.75f, 1.15f);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentChickenState)
        {
            case ChickenStates.Walking:
                {
                    WalkState(Time.deltaTime);
                    break;
                }
            case ChickenStates.Looking:
                {
                    LookState(Time.deltaTime);
                    break;
                }
            case ChickenStates.Pecking:
                {
                    PeckState(Time.deltaTime);
                    break;
                }
        }
    }

    void WalkState(float deltatime)
    {
        _animator.ResetTrigger("one_peck");
        _animator.ResetTrigger("two_peck");
        _animator.ResetTrigger("running");
        _animator.SetTrigger("walking");

        _chickenWalkTimer -= Time.deltaTime;
        _wander2Script.enabled = true;
        _steerScript.maxVelocity = 0.5f;

        if (_chickenWalkTimer <= 0) // when enough time has passed go to either the eating or pissing state
        {
            if (Random.Range(0, 2) == 0)
            {
                _peckTimer = 1.0f;
                currentChickenState = ChickenStates.Pecking;
            }
            else
            {
                _lookTimer = 4.0f;
                currentChickenState = ChickenStates.Looking;
            }
        }
    }

    void PeckState(float deltatime)
    {
        _animator.ResetTrigger("looking");
        _animator.ResetTrigger("walking");

        if (Random.Range(0, 2) == 0)
            _animator.SetTrigger("one_peck");
        else
            _animator.SetTrigger("two_peck");

        _steerScript.maxVelocity = 0;
        _peckTimer -= Time.deltaTime;

        if (_peckTimer <= 0)
        {
            if (Random.Range(0, 2) == 0)
            {
                _chickenWalkTimer = (Random.Range(8, 12));
                currentChickenState = ChickenStates.Walking;
            }
            else
            {
                _lookTimer = 4.0f;
                currentChickenState = ChickenStates.Looking;
            }
        }

    }

    void LookState(float deltatime)
    {

        _animator.ResetTrigger("two_peck");
        _animator.ResetTrigger("one_peck");
        _animator.ResetTrigger("walking");
        _animator.SetTrigger("looking");

        _steerScript.maxVelocity = 0;
        _lookTimer -= Time.deltaTime;

        if (_lookTimer <= 0)
        {
            if (Random.Range(0, 2) == 0)
            {
                _chickenWalkTimer = (Random.Range(8, 12));
                currentChickenState = ChickenStates.Walking;
            }
            else
            {
                _peckTimer = (Random.Range(1, 2));
                currentChickenState = ChickenStates.Pecking;
            }
        }
    }
}
