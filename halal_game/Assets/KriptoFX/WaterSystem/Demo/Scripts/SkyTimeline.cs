using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class SkyTimeline : MonoBehaviour
{
    public Material SkyMaterial;
    public AnimationCurve Curve1;
    public float TimeScale1 = 20;
    public string ShaderProperty1;

    public float SkyIntensity = 2;
    public float FogIntensity = 0;

    private float startValue;

    private float currentTime;

    void OnEnable()
    {
        currentTime = 0;
        if(SkyMaterial!=null) startValue = SkyMaterial.GetFloat(ShaderProperty1);
    }

    void OnDisable()
    {
        if (SkyMaterial != null) SkyMaterial.SetFloat(ShaderProperty1, startValue);
    }

    void Update()
    {
        currentTime += Time.deltaTime;
       
        if (SkyMaterial != null) 
        { 
            var param1 = Curve1.Evaluate(currentTime / TimeScale1);
            SkyMaterial.SetFloat(ShaderProperty1, param1); 
        }
        
        RenderSettings.ambientIntensity = SkyIntensity;
        if (FogIntensity > 0.0f) RenderSettings.fogDensity = FogIntensity;
    }
}
