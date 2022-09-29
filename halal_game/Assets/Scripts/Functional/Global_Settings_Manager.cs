using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Global_Settings_Manager : MonoBehaviour
{
    public int sensitivity = 5;
    public int musicVolume = 5;
    public int fxVolume = 5;

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

    public void SetMusicMixerVolume()
    {
        mixer.SetFloat("MusicVolume", (float)musicVolume);

        // divide mixer volume values into a range from 1-10
    }

    public void SetFXMixerVolume()
    {
        mixer.SetFloat("SFXVolume", (float)fxVolume);
    }
}
