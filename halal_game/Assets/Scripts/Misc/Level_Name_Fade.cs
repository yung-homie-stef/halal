using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_Name_Fade : MonoBehaviour
{
    public Delayed_Controller_Enabler controllerEnabler;

    public void BeginControllerEnabling()
    {
        controllerEnabler.BeginUnfreezing();
    }
}
