using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Chicken_Peck : MonoBehaviour
{
    public enum ChickenStates
    {
        Pecking,
        Looking,
        Idling
    }

    public ChickenStates currentChickenState = ChickenStates.Idling;

    private Animator _animator = null;
    private float _idleTimer = 2.0f;
    private float _lookTimer = 4.0f;
    private float _peckTimer = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();

        int startingState = Random.Range(1, 3);

        if (startingState == 1)
        {
            currentChickenState = ChickenStates.Idling;
        }
        else if (startingState == 2)
        {
            currentChickenState = ChickenStates.Looking;
        }
        else
        currentChickenState = ChickenStates.Pecking;

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentChickenState)
        {
            case ChickenStates.Idling:
                {
                    IdleState(Time.deltaTime);
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

    void PeckState(float deltatime)
    {
        _animator.ResetTrigger("looking");
        _animator.ResetTrigger("idling");

        if (Random.Range(0, 2) == 0)
            _animator.SetTrigger("one_peck");      
        else
            _animator.SetTrigger("two_peck");

        _peckTimer -= Time.deltaTime;

        if (_peckTimer <= 0)
        {
            if (Random.Range(0, 2) == 0)
            {
                _idleTimer = (Random.Range(2, 3));
                currentChickenState = ChickenStates.Idling;
            }
            else
            {
                _lookTimer = (Random.Range(4, 5));
                currentChickenState = ChickenStates.Looking;
            }
        }

    }

    void LookState(float deltatime)
    {
        _animator.ResetTrigger("two_peck");
        _animator.ResetTrigger("one_peck");
        _animator.ResetTrigger("idling");
        _animator.SetTrigger("looking");

        _lookTimer -= Time.deltaTime;

        if (_lookTimer <= 0)
        {
            if (Random.Range(0, 2) == 0)
            {
                _idleTimer = (Random.Range(2, 3));
                currentChickenState = ChickenStates.Idling;
            }
            else
            {
                _peckTimer = (Random.Range(1, 2));
                currentChickenState = ChickenStates.Pecking;
            }
        }
    }

    void IdleState(float deltatime)
    {
        _animator.ResetTrigger("two_peck");
        _animator.ResetTrigger("one_peck");
        _animator.ResetTrigger("looking");
        _animator.SetTrigger("idling");

        _idleTimer -= Time.deltaTime;

        if (_idleTimer <= 0)
        {
            if (Random.Range(0, 2) == 0)
            {
                _lookTimer = (Random.Range(4, 5));
                currentChickenState = ChickenStates.Looking;
            }
            else
            {
                _peckTimer = (Random.Range(1, 2));
                currentChickenState = ChickenStates.Pecking;
            }
        }
    }
}
