using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Interaction : MonoBehaviour
{
    public Sprite interactTextImage;

    public abstract void Interact();

    public virtual void DisplayInteractText()
    {
        Interaction_Label.globalGameLabelSystem.ChangeInteractionSprite(interactTextImage);
    }

    public virtual void HideInteractText()
    {
        Interaction_Label.globalGameLabelSystem.HideInteractionSprite();
    }
}
