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
    private int FOV = 5;

    private Settings_Manager _settings_Manager = null;
    private GameObject lastOpenedScreen = null;

    // Start is called before the first frame update
    void Start()
    {
        _settings_Manager = Settings_Manager.GetSettingsManager();

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

        _settings_Manager.sensitivity = sensitivity;
        sensitivityNum.text = sensitivity.ToString();
    }

    public void SetFOV(bool _increase)
    {
        if (_increase)
        {
            if (FOV < 10)
            {
                FOV++;
            }
        }
        else
        {
            if (FOV > 1)
            {
                FOV--;
            }
        }

        _settings_Manager.FOV = 50 + (6 * FOV);
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
