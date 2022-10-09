using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Load_Interaction : Interaction
{
    public int sceneIndex = 0;
    public AudioSource musicAudioSource;
    public float fadeDuration = 0.0f;

    public override void Interact()
    {
        StartCoroutine(LoadNextLevel(5.0f));
        Interaction_Label.globalGameLabelSystem.HideInteractionSprite();
        gameObject.tag = "Untagged";
        StartCoroutine(AudioSourceFade.StartFade(musicAudioSource, fadeDuration, 0));
    }

    private IEnumerator LoadNextLevel(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneIndex);
    }
}
