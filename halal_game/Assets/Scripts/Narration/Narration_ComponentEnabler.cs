using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_ComponentEnabler : Narration_Trigger
{
    public string[] componentNames;
    public GameObject[] objectsWithScriptsToEnable = null;
    public bool[] scriptEnabled = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            CheckIfNarratorIsTalking();
        }
    }

    public override void EndOfDialogueEvent()
    {
        for (int i = 0; i < objectsWithScriptsToEnable.Length; i++)
        {
            (objectsWithScriptsToEnable[i].GetComponent(componentNames[i]) as MonoBehaviour).enabled = scriptEnabled[i];
        }

        OnDialogueComplete.Invoke();

        Destroy(gameObject);
    }
}
    
