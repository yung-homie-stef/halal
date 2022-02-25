using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TheFirstPerson;

public class Swallow : MonoBehaviour
{
    public Image blackImage;
    public GameObject fleshPit;
    public GameObject pigCentipede;
    public GameObject sausages;
    public Transform teleportationPoint;

    //public TheFirstPerson player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            blackImage.enabled = true;
            sausages.SetActive(false);
            pigCentipede.SetActive(false);
            fleshPit.SetActive(true);
            StartCoroutine(TeleportPlayer(2.0f));
        }
    }

    private IEnumerator TeleportPlayer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        blackImage.enabled = false;
      //  .transform.position = teleportationPoint.position;

    }
}
