using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    public TextMeshProUGUI play = null;
    public TextMeshProUGUI settings = null;
    public TextMeshProUGUI quit = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        play.alignment = TextAlignmentOptions.Left;
        settings.alignment = TextAlignmentOptions.Left;
        quit.alignment = TextAlignmentOptions.Left;
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
