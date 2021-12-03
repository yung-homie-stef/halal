using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Cast : MonoBehaviour
{
    public Camera playerCamera;
    public RaycastHit playerRaycastHit;

    void Update()
    {
        Debug.DrawRay(playerCamera.transform.position, transform.forward, Color.green);

        if (Input.GetMouseButtonDown(0))
        {
            Interact();
        }
    }

    void Interact()
    {
        Ray _ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        GameObject objectToInteractWith;

        if (Physics.Raycast(_ray, out playerRaycastHit, 1))
        {
            if (playerRaycastHit.transform.tag.Equals("Interactable"))
            {
                objectToInteractWith = playerRaycastHit.transform.gameObject;

                //objectToInteractWith.GetComponent<KeyInteraction>().Activate();
            }
        }
    }

}
