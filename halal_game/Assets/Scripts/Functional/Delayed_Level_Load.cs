using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Delayed_Level_Load : MonoBehaviour
{
    public int sceneIndex = 0;
    public float loadDelayTime = 0;

    public void BeginLevelLoad()
    {
        StartCoroutine(LoadNextLevel(loadDelayTime));
    }

    private IEnumerator LoadNextLevel(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneIndex);
    }
}
