using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnAnimationEventEnd : MonoBehaviour
{
    public UnityEvent EndOfAnimationEvent;

    public void EndAnimation()
    {
        EndOfAnimationEvent.Invoke();
    }
}
