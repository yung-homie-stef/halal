using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Narration_Trigger : MonoBehaviour
{
    public string[] sentences;

    public abstract void EndOfDialogueEvent();

    private void OnTriggerEnter(Collider other)
    {
       if (other.tag == "Player")
        {
            if (!Narrator.gameNarrator.triggered)
            {
                StartTalking();
                Narrator.gameNarrator.narrationTriggerObject = this;
            }
        }
    }

    public virtual void StartTalking()
    {
        Narrator.gameNarrator.UnhideDialogue();

        // fill array with sentences from the trigger
        Narrator.gameNarrator.lines = new string[sentences.Length];
        for (int i = 0; i < sentences.Length; i++)
        {
            Narrator.gameNarrator.lines[i] = sentences[i];
        }

        Narrator.gameNarrator.StartDialogue();
    }

}
