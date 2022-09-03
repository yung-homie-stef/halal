using System.Collections;
using System.Collections.Generic;
using TheFirstPerson;
using UnityEngine;

public class Delayed_Controller_Enabler : MonoBehaviour
{
    public FPSController controller;

    public void BeginUnfreezing()
    {
        StartCoroutine(UnfreezePlayer(2.0f));
    }

    private IEnumerator UnfreezePlayer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        controller.enabled = true;
    }

}
