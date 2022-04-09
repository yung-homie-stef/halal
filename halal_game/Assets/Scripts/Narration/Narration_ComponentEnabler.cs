using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_ComponentEnabler : Narration_Trigger
{
    public string[] componentNames;
    public GameObject[] objectsWithScriptsToEnable = null;
    public bool[] scriptEnabled = null;

    public override void EndOfDialogueEvent()
    {
        for (int i = 0; i < objectsWithScriptsToEnable.Length; i++)
        {
            (objectsWithScriptsToEnable[i].GetComponent(componentNames[i]) as MonoBehaviour).enabled = scriptEnabled[i];
        }

        Destroy(gameObject);
    }
}
    
