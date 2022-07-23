using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg_Spawner : MonoBehaviour
{
    private Renderer meshRenderer;
    public GameObject eggPrefab;

    public float eggTimerMax = 0.0f;
    public float eggTimerMin = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        GenerateEggTime();
    }

    private IEnumerator DropEgg(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        float randomX = Random.Range(meshRenderer.bounds.min.x, meshRenderer.bounds.max.x);
        float randomZ = Random.Range(meshRenderer.bounds.min.z, meshRenderer.bounds.max.z);

        Instantiate(eggPrefab, new Vector3(randomX, transform.position.y, randomZ), Quaternion.identity);
        GenerateEggTime();
    }

    private void GenerateEggTime()
    {
        StartCoroutine(DropEgg(Random.Range(eggTimerMin, eggTimerMax)));
    }

    
}
