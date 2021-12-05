using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;

public class Text_Animation : MonoBehaviour
{
    public FPSController playerController = null;
    public string animationTrigger = null;
    public AnimationClip clip = null;

    private Animator _animator = null;
    private float _clipLength = 0.0f;

    private void Start()
    {
        _clipLength = clip.length;
        _animator = gameObject.GetComponent<Animator>();
        _animator.SetTrigger(animationTrigger);
        StartCoroutine(EndOfTextAnimationEvent(_clipLength));
    }

    private IEnumerator EndOfTextAnimationEvent(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        playerController.enabled = true;
    }

    
}
