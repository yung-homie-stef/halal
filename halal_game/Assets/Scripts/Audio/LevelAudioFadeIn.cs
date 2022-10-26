using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class LevelAudioFadeIn : MonoBehaviour
{
    //public AudioSource musicAudioSource;
    public AudioMixer sfxMixer;
    public float fadeDuration = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(AudioSourceFade.StartFade(musicAudioSource, fadeDuration, 1));
        Debug.Log(Global_Settings_Manager.instance.globalSFXMixerVolume);
        StartCoroutine(MixerFade.StartFade(sfxMixer, "SFXVolume", fadeDuration, Global_Settings_Manager.instance.globalSFXMixerVolume));
    }


}
