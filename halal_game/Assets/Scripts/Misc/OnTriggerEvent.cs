using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEvent : MonoBehaviour
{
    public UnityEvent OnTriggerBoxEnter;
    public UnityEvent OnTriggerBoxExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            OnTriggerBoxEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            OnTriggerBoxExit.Invoke();
    }
}
