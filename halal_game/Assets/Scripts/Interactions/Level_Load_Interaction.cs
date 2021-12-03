using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Load_Interaction : Interaction
{
    public int sceneIndex = 0;

    public override void Interact()
    {
        StartCoroutine(LoadNextLevel(3.0f));
    }

    private IEnumerator LoadNextLevel(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneIndex);
    }
}
