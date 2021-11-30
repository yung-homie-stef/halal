using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Language_Updater : MonoBehaviour
{
    private void Awake()
    {
        Language_Resolver.globalLanguageResolver.ResolveTexts();
    }
}
