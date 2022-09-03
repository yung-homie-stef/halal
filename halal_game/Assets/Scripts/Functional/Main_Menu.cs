using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main_Menu : MonoBehaviour
{
    public TextMeshProUGUI play = null;
    public TextMeshProUGUI settings = null;
    public TextMeshProUGUI quit = null;

    public GameObject settingsMenu = null;
    public GameObject startMenu = null;
    public Image blowfly = null;

    private Settings _settings = null;

    // Start is called before the first frame update
    void Start()
    {
        _settings = settingsMenu.GetComponent<Settings>();

        DontDestroyOnLoad(this);
    }
    public void PlaySelected()
    {
        play.alignment = TextAlignmentOptions.Flush;
    }
    public void SettingsSelected()
    {
        settings.alignment = TextAlignmentOptions.Flush;
    }
    public void QuitSelected()
    {
        quit.alignment = TextAlignmentOptions.Flush;
    }
    public void RevertText()
    {
        play.alignment = TextAlignmentOptions.Center;
        settings.alignment = TextAlignmentOptions.Center;
        quit.alignment = TextAlignmentOptions.Center;
    }
    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
        _settings.SetLastOpenedMenu(startMenu);
        startMenu.SetActive(false);
        RevertText();
    }
    public void PlayGame()
    {
        startMenu.SetActive(false);
        settingsMenu.SetActive(false);
        blowfly.enabled = true;
        StartCoroutine(LoadIntroSequence(4.0f));
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private IEnumerator LoadIntroSequence(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        blowfly.enabled = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
