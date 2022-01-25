using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KW_AddLightToWaterRendering_URP : MonoBehaviour
{
    Light currentLight;

    void OnEnable()
    {
        currentLight = GetComponent<Light>();
        KW_WaterDynamicScripts.AddLight(currentLight);
    }

    private void OnDisable()
    {
        KW_WaterDynamicScripts.AddLight(currentLight);
    }


    void Update()
    {
        
    }
}
