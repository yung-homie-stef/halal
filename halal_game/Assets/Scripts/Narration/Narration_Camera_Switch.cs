using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TheFirstPerson;


public class Narration_Camera_Switch : Narration_Trigger
{
    public FPSController[] playerControllers = new FPSController[3];
    [SerializeField]
    private int indexToActivate = 0;

    public PlayableDirector cameraSwapCutscene = null;

    public override void EndOfDialogueEvent()
    {
        StartCoroutine(SwapCameras((float)cameraSwapCutscene.duration));
    }

    private IEnumerator SwapCameras(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < playerControllers.Length; i++)
        {
            if (i == indexToActivate)
            {
                playerControllers[i].enabled = true;
            }
            else
                playerControllers[i].enabled = false;
        }
    }
}
