using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_Tag_Change : Narration_Trigger
{
    public Interaction objectToInteractWith;
    public string newTag = "";

    public override void EndOfDialogueEvent()
    {
        objectToInteractWith.gameObject.tag = newTag;
    }
}
