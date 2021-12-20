using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_Trigger : MonoBehaviour
{
    public string[] sentences;

    private void OnTriggerEnter(Collider other)
    {
        Narrator.gameNarrator.UnhideDialogue();

        if (other.tag == "Player")
        {
            // fill array with sentences from the trigger
            Narrator.gameNarrator.lines = new string[sentences.Length];
            for (int i = 0; i < sentences.Length; i++)
            {
                Narrator.gameNarrator.lines[i] = sentences[i];
            }

            Narrator.gameNarrator.StartDialogue();
        }

        Destroy(gameObject);
    }
}
