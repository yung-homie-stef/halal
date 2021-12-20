using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Cast : MonoBehaviour
{
    public Camera playerCamera;
    public RaycastHit playerRaycastHit;

    Interaction objectToInteractWith = null;

    void Update()
    {
        Ray _ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(_ray, out playerRaycastHit, 2))
        {
            if (playerRaycastHit.transform.tag.Equals("Interactable"))
            {
                objectToInteractWith = playerRaycastHit.transform.gameObject.GetComponent<Interaction>();
                objectToInteractWith.DisplayInteractText();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    objectToInteractWith.GetComponent<Interaction>().Interact();
                }
            }
        }
        else
        {
            if (objectToInteractWith != null)
            {
                objectToInteractWith.HideInteractText();
            }
        }
    }


}
