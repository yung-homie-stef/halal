using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings_Manager : MonoBehaviour
{
    public static Settings_Manager globalSettingsManager = null;

    public float sensitivity = 5.0f;
    public float FOV = 80.0f;
    public bool sprintToggle = true;

    private void Awake()
    {
        if (globalSettingsManager != null)
        {
            Destroy(this.gameObject);
            return;
        }

        globalSettingsManager = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public static Settings_Manager GetSettingsManager()
    {
        return globalSettingsManager;
    }
}