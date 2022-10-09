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

    public void SetMusicMixerVolume(int vol)
    {
        //mixer.SetFloat("MusicVolume", volumeValues[vol]);
    }

    public void SetFXVolume(int number)
    {
        int fxVolume = number;
        Debug.Log(number);
        SetFXMixerVolume(fxVolume);
    }

    public void SetMusicVolume(int number)
    {
        int musicVolume = number;
        Debug.Log(number);
        SetMusicMixerVolume(musicVolume);
    }

    public void SetFXMixerVolume(int vol)
    {
       // mixer.SetFloat("SFXVolume", volumeValues[vol]);
    }
}
