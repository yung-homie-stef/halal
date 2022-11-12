using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Delayed_Level_Load : MonoBehaviour
{
    public int sceneIndex = 0;
    public float loadDelayTime = 0;
    public AudioClip[] interactSounds;

    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void BeginLevelLoad()
    {
        StartCoroutine(LoadNextLevel(loadDelayTime));
        StartCoroutine(PlayLevelLoadRiser(3.0f));
    }

    private IEnumerator LoadNextLevel(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneIndex);
    }

    private IEnumerator PlayLevelLoadRiser(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _audioSource.PlayOneShot(interactSounds[Random.Range(0, interactSounds.Length)]);
    }
}
