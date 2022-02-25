using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TheFirstPerson;

public class Swallow : MonoBehaviour
{
    public Image blackImage = null;
    public GameObject fleshPit = null;
    public GameObject pigCentipede = null;
    public GameObject sausages = null;
    public Transform teleportationPoint = null;

    public Light directionalLight = null;

    public FPSController player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            blackImage.enabled = true;
            sausages.SetActive(false);
            pigCentipede.SetActive(false);
            fleshPit.SetActive(true);

            directionalLight.enabled = false;
            RenderSettings.skybox = null;

            StartCoroutine(TeleportPlayer(2.0f));
        }
    }

    private IEnumerator TeleportPlayer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        blackImage.enabled = false;
        player.transform.position = teleportationPoint.position;

    }
}
