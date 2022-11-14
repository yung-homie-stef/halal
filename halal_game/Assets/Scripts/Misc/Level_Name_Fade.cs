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
    public float maxVolume = 0.0f;
    

    void Start()
    {
        StartCoroutine(MixerFade.StartFade(SFXaudioMixer, "SFXVolume", fadeDuration, Global_Settings_Manager.instance.globalSFXMixerVolume));
        StartCoroutine(AudioSourceFade.StartFade(musicAudioSource, fadeDuration, maxVolume));
    }

    public void BeginControllerEnabling()
    {
        controllerEnabler.BeginUnfreezing();
    }
}
