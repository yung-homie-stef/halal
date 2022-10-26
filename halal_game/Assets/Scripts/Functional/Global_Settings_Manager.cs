using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Global_Settings_Manager : MonoBehaviour
{
    public GameObject settingsMenu;
    public GameObject pauseMenu;

    public AudioMixer mixer;

    public bool isPaused = false;
    public float globalSFXMixerVolume = 1.0f;

    public static Global_Settings_Manager instance = null;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SetMusicMixerVolume(float vol)
    {
        mixer.SetFloat("MusicVolume", vol);
    }

    public void SetFXVolume(float number)
    {
        float fxVolume = number;
        Debug.Log(number);
        SetFXMixerVolume(fxVolume);
    }

    public void SetMusicVolume(float number)
    {
        float musicVolume = number;
        Debug.Log(number);
        SetMusicMixerVolume(musicVolume);
    }

    public void SetFXMixerVolume(float vol)
    {
        mixer.SetFloat("SFXVolume",vol);
        
    }

    public void SetGlobalFXMixerVolumeForLevelFadeInValue(float value)
    {
        globalSFXMixerVolume = value;
    }
}
