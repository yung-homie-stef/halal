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

    public AudioClip[] menuSelectClips;
    public AudioClip[] menuClickClips;
    public AudioClip startOinkClip;
    public AudioSource audioSource = null;

    public GameObject startMenu = null;
    public Image blowfly = null;

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this);
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
        Global_Settings_Manager.instance.settingsMenu.SetActive(true);
        Global_Settings_Manager.instance.settingsMenu.GetComponent<Settings>().SetLastOpenedMenu(startMenu);
        startMenu.SetActive(false);
        RevertText();
    }
    public void PlayGame()
    {
        startMenu.SetActive(false);
        Global_Settings_Manager.instance.settingsMenu.SetActive(false);
        blowfly.enabled = true;
        StartCoroutine(LoadIntroSequence(4.0f));
        audioSource.PlayOneShot(startOinkClip);
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

    public void OnMenuItemSelected()
    {
        audioSource.PlayOneShot(menuSelectClips[Random.Range(0, menuSelectClips.Length)]);
    }

    public void OnMenuItemClicked()
    {
        audioSource.PlayOneShot(menuClickClips[Random.Range(0, menuClickClips.Length)]);
    }
}
