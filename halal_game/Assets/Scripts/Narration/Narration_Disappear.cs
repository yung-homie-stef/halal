using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration_Disappear : Narration_Trigger
{
    [SerializeField]
    public GameObject[] objectsToModify = null;
    public bool[] appearOrNot = null;


    public override void EndOfDialogueEvent()
    {
        for (int i = 0; i < objectsToModify.Length; i++)
        {
            objectsToModify[i].SetActive(appearOrNot[i]);
        }

        Destroy(gameObject);
    }

}
