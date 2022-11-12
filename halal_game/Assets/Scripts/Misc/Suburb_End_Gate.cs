using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suburb_End_Gate : MonoBehaviour
{
    public Kill_Count killCountScript;
    public GameObject finalSuburbTrigger = null;

    public void CheckIfAllPigsAreDead()
    {
        if (killCountScript.GetKillCount() == 0)
        {
            finalSuburbTrigger.SetActive(true);
        }
    }
}
