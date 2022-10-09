using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;
using TMPro;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public TextMeshProUGUI back = null;
    public TextMeshProUGUI sensitivityNum = null;
    public TextMeshProUGUI musicVolNum = null;
    public TextMeshProUGUI sfxVolNum = null;

    public Toggle dyslexicToggle = null;
    public Toggle sprintToggle = null;

    [SerializeField]
    private int sensitivity = 5;
    [SerializeField]
    private int musicVolume = 5;
    [SerializeField]
    private int sfxVolume = 5;

    private GameObject lastOpenedScreen = null;

    // Start is called before the first frame update
    void Start()
    {

        if (!PlayerPrefs.HasKey("Sensitivity"))
        {
            PlayerPrefs.SetInt("Sensitivity", 5); // default sensitivity settings
        }

        if (!PlayerPrefs.HasKey("Dyslexic"))
        {
            PlayerPrefs.SetInt("Dyslexic", 1); 
        }

        if (!PlayerPrefs.HasKey("SprintToggle"))
        {
            PlayerPrefs.SetInt("SprintToggle", 1); 
        }

        if (PlayerPrefs.GetInt("Dyslexic") == 2)
            dyslexicToggle.isOn = true;
        else
            dyslexicToggle.isOn = false;

        if (PlayerPrefs.GetInt("SprintToggle") == 2)
            sprintToggle.isOn = true;
        else
            sprintToggle.isOn = false;
    }
    public void BackSelected()
    {
        back.alignment = TextAlignmentOptions.Flush;
    }
    public void RevertText()
    {
        back.alignment = TextAlignmentOptions.Center;
    }
    public void SetSensitivity(bool _increase)
    {
        if (_increase)
        {
            if ( sensitivity < 10)
            {
                sensitivity++;
                PlayerPrefs.SetInt("Sensitivity", sensitivity); 
            }
        }
        else
        {
            if (sensitivity > 1)
            {
                sensitivity--;
                PlayerPrefs.SetInt("Sensitivity", sensitivity);
            }
        }

        sensitivityNum.text = sensitivity.ToString();
    }

    #region MUSIC VOLUME
    public void SetMusicVolume(bool _increase)
    {
        if (_increase)
        {
            if (musicVolume < 10)
            {
                musicVolume++;
                Global_Settings_Manager.instance.SetMusicVolume(musicVolume);
               
            }
        }
        else
        {
            if (musicVolume > 0)
            {
                musicVolume--;
                Global_Settings_Manager.instance.SetMusicVolume(musicVolume);

            }
        }

        musicVolNum.text = musicVolume.ToString();
    }
    #endregion

    public void SetSFXVolume(bool _increase)
    {
        if (_increase)
        {
            if (sfxVolume < 10)
            {
                sfxVolume++;
                Global_Settings_Manager.instance.SetFXVolume(sfxVolume);

            }
        }
        else
        {
            if (sfxVolume > 0)
            {
                sfxVolume--;
                Global_Settings_Manager.instance.SetFXVolume(sfxVolume);

            }
        }

        sfxVolNum.text = sfxVolume.ToString();
    }

    public void SetSprintToggle(bool toggled)
    {
        if (toggled)
            PlayerPrefs.SetInt("SprintToggle", 2);
        else
            PlayerPrefs.SetInt("SprintToggle", 1);
    }
    public void SetDyslexicFont(bool fontFlag)
    {
        if (fontFlag)
            PlayerPrefs.SetInt("Dyslexic", 2);
        else
            PlayerPrefs.SetInt("Dyslexic", 1);
    }
    public void SetLastOpenedMenu(GameObject menu)
    {
        lastOpenedScreen = menu;
    }
    public void GoBack()
    {
        lastOpenedScreen.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
