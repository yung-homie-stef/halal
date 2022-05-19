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

    public Quaternion endRotation;
    public Quaternion cameraRotation;
    public float rotationSpeed = 0f;
    private bool hasSwapped = false;
    private GameObject playerThatWillBeRotated = null;

    public override void EndOfDialogueEvent()
    {
        hasSwapped = true;

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

    private void Update()
    {
        if (hasSwapped == true)
        {
            playerThatWillBeRotated.transform.rotation = Quaternion.RotateTowards(playerThatWillBeRotated.transform.rotation, endRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetPlayerToBeRotated(GameObject playerObj)
    {
        playerThatWillBeRotated = playerObj;
    }


}
