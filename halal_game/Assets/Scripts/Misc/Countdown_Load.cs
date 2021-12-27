using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Countdown_Load : MonoBehaviour
{
    [SerializeField]
    private float _loadingTime = 0.0f;

    private void Start()
    {
        StartCoroutine(LoadNextScene(_loadingTime));
    }

    private IEnumerator LoadNextScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }
}
