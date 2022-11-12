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

    public Toggle dyslexicToggle = null;
    public Toggle sprintToggle = null;

    [SerializeField]
    private int sensitivity = 5;
    [SerializeField]
    private int musicVolume = 5;
    [SerializeField]
    private int sfxVolume = 5;

    private GameObject lastOpenedScreen = null;

    public AudioClip[] menuSelectClips;
    public AudioClip[] menuClickClips;
    public AudioSource audioSource = null;

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

    public void SetMusicLevel(float sliderValue)
    {
        float mixerVolume = Mathf.Log10(sliderValue) * 20;
        Global_Settings_Manager.instance.SetMusicVolume(mixerVolume);
        
    }

    public void SetSFXLevel(float sliderValue)
    {
        float mixerVolume = Mathf.Log10(sliderValue) * 20;
        Global_Settings_Manager.instance.SetFXVolume(mixerVolume);
        Global_Settings_Manager.instance.SetGlobalFXMixerVolumeForLevelFadeInValue(sliderValue);
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
        Narrator.gameNarrator.SetFontToDyslexic(fontFlag);
    }
    public void SetLastOpenedMenu(GameObject menu)
    {
        lastOpenedScreen = menu;
    }
    public void GoBack()
    {
        back.alignment = TextAlignmentOptions.Center;
        lastOpenedScreen.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void OnMenuItemSelected()
    {
        audioSource.PlayOneShot(menuSelectClips[Random.Range(0, menuSelectClips.Length)]);
    }

    public void OnMenuItemClicked()
    {
        audioSource.PlayOneShot(menuClickClips[Random.Range(0, menuClickClips.Length)]);
    }
}
