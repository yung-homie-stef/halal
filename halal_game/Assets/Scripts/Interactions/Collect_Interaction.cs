using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Collect_Interaction : Interaction
{
    public UnityEvent onAllCollected;

    private static int objectsCollected = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Interact()
    {
        objectsCollected++;

        if (objectsCollected == 3)
        {
            onAllCollected.Invoke();
        }
        gameObject.tag = "Untagged";

        gameObject.SetActive(false);
    }
}
