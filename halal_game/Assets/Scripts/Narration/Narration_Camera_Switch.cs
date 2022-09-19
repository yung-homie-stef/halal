using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TheFirstPerson;


public class Narration_Camera_Switch : Narration_Trigger
{
    public FPSController controllerToDeactivate;
    public FPSController controllerToActivate;
    public CharacterController characterController;
    public Camera currentCamera;

    public Camera_Lerp lerpScript;

    public override void EndOfDialogueEvent()
    {
        lerpScript.LerpCamera(controllerToDeactivate);

        controllerToDeactivate.enabled = false;

        if (controllerToActivate != null)
        controllerToActivate.enabled = true;

        characterController.enabled = false;

        OnDialogueComplete.Invoke();

        Destroy(gameObject);
    }


}
