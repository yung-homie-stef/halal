using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class Language_Resolver : MonoBehaviour
{
    public static Language_Resolver globalLanguageResolver = null;

    private const char Separator = '=';
    private readonly Dictionary<SystemLanguage, LangData> _langData = new Dictionary<SystemLanguage, LangData>();
    private readonly List<SystemLanguage> _supportedLanguages = new List<SystemLanguage>();
    private SystemLanguage _language;

    private void Awake()
    {
        if (globalLanguageResolver != null)
        {
            Destroy(this.gameObject);
            return;
        }

        globalLanguageResolver = this;

        DontDestroyOnLoad(gameObject);
        ReadProperties();
    }

    private void ReadProperties()
    {
        foreach (var file in Resources.LoadAll<TextAsset>("Language_Files"))
        {
            System.Enum.TryParse(file.name, out SystemLanguage language);
            var lang = new Dictionary<string, string>();
            foreach (var line in file.text.Split('\n'))
            {
                var prop = line.Split(Separator);
                lang[prop[0]] = prop[1];
            }
            _langData[language] = new LangData(lang);
            _supportedLanguages.Add(language);
        }
        ResolveLanguage();
    }

    private void ResolveLanguage()
    {
        _language = Prefs_Holder.GetLang();
        if (!_supportedLanguages.Contains(_language))
        {
            _language = SystemLanguage.English;
        }
    }

    public void ResolveTexts()
    {
        var lang = _langData[_language].Lang;
        foreach (var langText in Resources.FindObjectsOfTypeAll<Language_Text>())
        {
            langText.ChangeText(lang[langText.identifier]);
        }
    }

    private class LangData
    {
        public readonly Dictionary<string, string> Lang;

        public LangData(Dictionary<string, string> lang)
        {
            Lang = lang;
        }
    }

    public void ChangeLanguage(SystemLanguage language)
    {
        _language = language;
        ResolveTexts();
        Prefs_Holder.SaveLang(_language);
    }
}