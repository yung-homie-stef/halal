using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    public AnimationClip stabClip = null;

    private bool _isStabbing = false;
    private float _stabAnimationTime = 0.0f;
    private Animator _animator = null;
    private BoxCollider _boxCollider = null;

    // Start is called before the first frame update
    void Start()
    {
        _stabAnimationTime = stabClip.length;
        _animator = gameObject.GetComponent<Animator>();
        _boxCollider = gameObject.GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Narrator.gameNarrator.chatting != true)
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
    }

    public void EnableHitbox(int flag)
    {
        if (flag == 0)
        {
            _boxCollider.enabled = true;
        }
        else
            _boxCollider.enabled = false;
    }

    private IEnumerator ResetStabbing(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _isStabbing = false;
    }
}
