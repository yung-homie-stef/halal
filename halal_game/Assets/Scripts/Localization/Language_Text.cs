using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class Language_Text : MonoBehaviour
{
    public string identifier;

    public void ChangeText(string text)
    {
        GetComponent<TextMeshProUGUI>().text = Regex.Unescape(text);
    }
}
