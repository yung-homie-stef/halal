using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Bool : MonoBehaviour
{
    private Animator _animator = null;

    [SerializeField]
    private string _boolName;

    // Start is called before the first frame update
    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
        SetAnimatorBool();
    }

    private void SetAnimatorBool()
    {
        if (Random.Range(1,2) == 1)
        {
            _animator.SetBool(_boolName, true);
        }
        else
            _animator.SetBool(_boolName, false);
    }

    
}
