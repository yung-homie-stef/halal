using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Level_Name_Fade : MonoBehaviour
{
    public Delayed_Controller_Enabler controllerEnabler;
    public AudioMixer SFXaudioMixer;
    public AudioSource musicAudioSource;
    public float fadeDuration = 0.0f;
    

    void Start()
    {
        //StartCoroutine(MixerFade.StartFade(SFXaudioMixer, "SFXVolume", fadeDuration, 0.0f));
        StartCoroutine(AudioSourceFade.StartFade(musicAudioSource, fadeDuration, 1));
    }

    public void BeginControllerEnabling()
    {
        controllerEnabler.BeginUnfreezing();
    }
}
