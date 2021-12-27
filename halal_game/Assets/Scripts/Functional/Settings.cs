using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;
using TMPro;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public TextMeshProUGUI back = null;
    public TextMeshProUGUI FOVNum = null;
    public TextMeshProUGUI sensitivityNum = null;

    public TMP_FontAsset linLibertine = null;
    public TMP_FontAsset dyslexiaFont = null;

    [SerializeField]
    private int sensitivity = 5;
    [SerializeField]
    private int FOV = 5;

    private Settings_Manager _settings_Manager = null;

    // Start is called before the first frame update
    void Start()
    {
        _settings_Manager = Settings_Manager.GetSettingsManager();

        DontDestroyOnLoad(this);
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
            }
        }
        else
        {
            if (sensitivity > 1)
            {
                sensitivity--;
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
        FOVNum.text = FOV.ToString();
    }

    public void SetSprintToggle(bool toggled)
    {
        _settings_Manager.sprintToggle = toggled;
    }
    public void SetDyslexicFont(bool fontFlag)
    {
        if (fontFlag)
            Narrator.gameNarrator.textComponent.font = dyslexiaFont;
        else
            Narrator.gameNarrator.textComponent.font = linLibertine;
    }
}
