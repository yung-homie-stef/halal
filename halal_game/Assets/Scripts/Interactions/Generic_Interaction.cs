using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Generic_Interaction : Interaction
{
    public UnityEvent OnInteract;

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void Interact()
    {
        OnInteract.Invoke();
    }
}
