using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;
using UnityEngine.SceneManagement;

public class Text_Animation : MonoBehaviour
{
    public FPSController playerController = null;
    public string animationTrigger = null;
    public AnimationClip clip = null;
    public bool hasLoadingEvent = false;

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

        if (hasLoadingEvent)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        playerController.enabled = true;
    }

    
}
