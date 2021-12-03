using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Loader : MonoBehaviour
{
    public static Level_Loader globalSceneLoader = null;

    private void Awake()
    {
        if (globalSceneLoader != null)
        {
            Destroy(this.gameObject);
            return;
        }

        globalSceneLoader = this;
        DontDestroyOnLoad(this.gameObject);

        Load("Church");
    }

    public void Load(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
