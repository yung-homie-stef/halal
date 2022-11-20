using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Narration_Trigger : MonoBehaviour
{
    public string[] sentences;

    public abstract void EndOfDialogueEvent();
    public UnityEvent OnDialogueComplete;

    protected void CheckIfNarratorIsTalking()
    {
            if (!Narrator.gameNarrator.triggered)
            {
                StartTalking();
                Narrator.gameNarrator.narrationTriggerObject = this;
            }
    }

    public void SetSentences(NonlinearPhrases words)
    {
        sentences = words.nonlinearPhrases;
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
