using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tag_Changer : MonoBehaviour
{
    public string newTag = "";

    public void ChangeTag()
    {
        gameObject.tag = newTag;
    }
}
