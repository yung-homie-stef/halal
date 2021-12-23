using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Interaction : Interaction
{
    private Animator _animator;

    private void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
    }

    public override void Interact()
    {
        _animator.SetTrigger("open");
        Interaction_Label.globalGameLabelSystem.HideInteractionSprite();
        gameObject.tag = "Untagged";
    }
}
