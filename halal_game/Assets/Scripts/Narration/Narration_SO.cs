using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_SO : Narration_Trigger
{
    public static int scriptableIndex = 0;
    public NonlinearPhrases[] phrases;
    public Tag_Changer tagger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            SetSentences(phrases[scriptableIndex]);
            CheckIfNarratorIsTalking();
        }

    }

    public override void EndOfDialogueEvent()
    {
        scriptableIndex++;
        tagger.ChangeTag();
        //OnDialogueComplete.Invoke();
    }
}
