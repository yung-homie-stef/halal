using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TheFirstPerson;


public class Narration_Camera_Switch : Narration_Trigger
{
    public FPSController controllerToDeactivate;
    public FPSController controllerToActivate;
    public Camera currentCamera;

    public Camera_Lerp lerpScript;

    public override void EndOfDialogueEvent()
    {
        controllerToDeactivate.enabled = false;

        if (controllerToActivate != null)
        controllerToActivate.enabled = true;

        lerpScript.LerpCamera(currentCamera);

        Destroy(gameObject);
    }


}
