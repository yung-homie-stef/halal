using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    public AnimationClip stabClip = null;

    private bool _isStabbing = false;
    private float _stabAnimationTime = 0.0f;
    private Animator _animator = null;

    // Start is called before the first frame update
    void Start()
    {
        _stabAnimationTime = stabClip.length;
        _animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_isStabbing == false)
            {
                StartCoroutine(ResetStabbing(_stabAnimationTime));
                _animator.SetTrigger("stab");
                Debug.Log("stab");
                _isStabbing = true;
            }
        }
    }

    private IEnumerator ResetStabbing(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _isStabbing = false;
    }
}
