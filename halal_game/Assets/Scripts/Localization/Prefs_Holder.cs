using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefs_Holder : MonoBehaviour
{
    private const string Lang = "Lang";
    public static void SaveLang(SystemLanguage lang)
    {
        PlayerPrefs.SetString(Lang, lang.ToString());
    }
    public static SystemLanguage GetLang()
    {
        System.Enum.TryParse(
            PlayerPrefs.GetString(Lang, Application.systemLanguage.ToString()),
            out SystemLanguage language);
        return language;
    }
}
