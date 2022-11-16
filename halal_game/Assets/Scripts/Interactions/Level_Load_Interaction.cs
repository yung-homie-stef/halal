using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Level_Load_Interaction : Interaction
{
    public int sceneIndex = 0;
    public AudioSource musicAudioSource;
    public AudioMixer sfxMixer;
    public float fadeDuration = 0.0f;
    public Animator transitionAnimator = null;

    public AudioClip[] interactSounds;

    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        StartCoroutine(LoadNextLevel(8.0f));
        Interaction_Label.globalGameLabelSystem.HideInteractionSprite();
        gameObject.tag = "Untagged";
        StartCoroutine(AudioSourceFade.StartFade(musicAudioSource, fadeDuration, 0));
        StartCoroutine(MixerFade.StartFade(sfxMixer, "SFXVolume", fadeDuration, 0.0001f));
        StartCoroutine(PlayTransitionGraphic(2.0f));
        _audioSource.PlayOneShot(interactSounds[Random.Range(0, interactSounds.Length)]);
    }

    private IEnumerator PlayTransitionGraphic(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        transitionAnimator.transform.parent.gameObject.GetComponent<Canvas>().enabled = true;
        transitionAnimator.SetTrigger("transition");
    }

    private IEnumerator LoadNextLevel(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneIndex);
    }
}
