using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Loader : MonoBehaviour
{
    public void Load(int sceneIndex)
    {
         SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }
}
