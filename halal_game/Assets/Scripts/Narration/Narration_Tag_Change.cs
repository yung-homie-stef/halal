using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_Tag_Change : Narration_Trigger
{
    public Interaction objectToInteractWith;
    public string newTag = "";

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            CheckIfNarratorIsTalking();
        }
    }

    public override void EndOfDialogueEvent()
    {
        OnDialogueComplete.Invoke();
        objectToInteractWith.gameObject.tag = newTag;
        Destroy(gameObject);
    }
}
