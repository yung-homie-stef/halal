using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MonoBehaviour
{
    public Material[] eggshellMaterials;

    private GameObject yolk = null;
    private MeshRenderer meshRenderer = null;
    private Rigidbody rb = null;

    private void Start()
    {
        yolk = gameObject.transform.GetChild(0).gameObject;
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();

        meshRenderer.material = eggshellMaterials[Random.Range(0, 2)];
    }

    private void OnCollisionEnter(Collision collision)
    {
        meshRenderer.enabled = false;
        rb.useGravity = false;
        Destroy(rb);
        yolk.SetActive(true);
        StartCoroutine(DestroyEgg(5.0f));
    }

    private IEnumerator DestroyEgg(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(this.gameObject);
    }
}
