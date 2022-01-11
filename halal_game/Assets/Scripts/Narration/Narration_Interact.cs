using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_Interact : Narration_Trigger
{
    public Interaction[] interactiveObjects;
    [SerializeField]
    private bool[] enabledOrNot;

    public override void EndOfDialogueEvent()
    {
        for (int i = 0; i < interactiveObjects.Length; i++)
        {
            interactiveObjects[i].enabled = enabledOrNot[i];
        }

        Destroy(gameObject);
    }


}
