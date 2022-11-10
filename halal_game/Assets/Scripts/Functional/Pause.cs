using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public TextMeshProUGUI resume = null;
    public TextMeshProUGUI settings = null;
    public TextMeshProUGUI quitPrompt = null;
    public TextMeshProUGUI yesQuit = null;
    public TextMeshProUGUI noQuit = null;

    public Button continueButton = null;
    public Button settingsButton = null;
    public Button quitButton = null;

    public GameObject quitMenu = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResumeSelected()
    {
        resume.alignment = TextAlignmentOptions.Flush;
    }

    public void SettingsSelected()
    {
        settings.alignment = TextAlignmentOptions.Flush;
    }

    public void QuitPromptSelected()
    {
        quitPrompt.alignment = TextAlignmentOptions.Flush;
    }

    public void YesQuitSelected()
    {
        yesQuit.alignment = TextAlignmentOptions.Flush;
    }

    public void NoQuitSelected()
    {
        noQuit.alignment = TextAlignmentOptions.Flush;
    }

    public void RevertText()
    {
        resume.alignment = TextAlignmentOptions.Center;
        settings.alignment = TextAlignmentOptions.Center;
        quitPrompt.alignment = TextAlignmentOptions.Center;
        noQuit.alignment = TextAlignmentOptions.Center;
        yesQuit.alignment = TextAlignmentOptions.Center;
    }

    public void OpenSettings()
    {
        Global_Settings_Manager.instance.settingsMenu.SetActive(true);
        Global_Settings_Manager.instance.settingsMenu.GetComponent<Settings>().SetLastOpenedMenu(this.gameObject);
        Global_Settings_Manager.instance.isInSettings = true;
        RevertText();
        gameObject.SetActive(false);
    }

    public void OpenQuit()
    {
        quitMenu.SetActive(true);
        settingsButton.gameObject.SetActive(false); 
        continueButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DontQuitGame()
    {
        settingsButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
        RevertText();
        quitMenu.SetActive(false);
    }

    public void Resume()
    {
        Time.timeScale = 1.0f;
        gameObject.SetActive(false);
    }
}
