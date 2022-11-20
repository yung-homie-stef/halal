using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Wavy_Text : MonoBehaviour
{
    public TMP_Text text;
    private BoxCollider _boxCollider;

    private void Start()
    {
        _boxCollider = gameObject.GetComponent<BoxCollider>();
    }


    // Update is called once per frame
    void Update()
    {
        text.ForceMeshUpdate();
        var textInfo = text.textInfo;

        for (int i =0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
            {
                continue;
            }

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                var orig = verts[charInfo.vertexIndex + j];
                verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time*2f + orig.x*0.01f) * 20f, 0);
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(RemoveWavyText(8.0f));
        text.gameObject.SetActive(true);
        _boxCollider.enabled = false;
    }

    private IEnumerator RemoveWavyText(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        text.gameObject.SetActive(false);
    }


}
