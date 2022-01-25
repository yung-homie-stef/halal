using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class KW_WaterDynamicScripts
{
    static WaterSystem currentWater;
  
    public static void SetCurrentWater(WaterSystem water)
    {
        currentWater = water;
    }

    public static WaterSystem GetCurrentWater()
    {
        return currentWater;
    }

#if UNITY_PIPELINE_URP
    static ScriptableRenderContext currentContext;

    public static void SetCurrentWaterContext(ScriptableRenderContext context)
    {
        currentContext = context;
    }

    public static ScriptableRenderContext GetCurrentWaterContext()
    {
        return currentContext;
    }
#endif

    //-------------------------------------------  Dynamic waves  -------------------------------------------------------------------------------------------------------------------

    public static int DefaultInteractWavesCapacity = 2;
    static List<KW_InteractWithWater> interactScripts = new List<KW_InteractWithWater>(DefaultInteractWavesCapacity);
    static KW_InteractWithWater[] nearScripts = new KW_InteractWithWater[DefaultInteractWavesCapacity];

    public static void AddInteractScript(KW_InteractWithWater script)
    {
        if(!interactScripts.Contains(script)) interactScripts.Add(script);
    }

    public static void RemoveInteractScript(KW_InteractWithWater script)
    {
        if (interactScripts.Contains(script)) interactScripts.Remove(script);
    }

    public static KW_InteractWithWater[] GetInteractScriptsInArea(Vector3 areaPos, int areaSize, out int endIdx)
    {
        if (interactScripts.Count > nearScripts.Length) Array.Resize(ref nearScripts, (int)(interactScripts.Count * 1.5f));

        var lastIdx = 0;
        foreach (var script in interactScripts)
        {
            var dist = Vector3.Distance(script.t.position, areaPos);
            if (dist < areaSize)
            {
                nearScripts[lastIdx] = script;
                lastIdx++;
            }
        }

        endIdx = lastIdx;
        return nearScripts;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------




    //-------------------------------------------  Buoyancy  -------------------------------------------------------------------------------------------------------------------

    public static int DefaultBuoyancyCapacity = 2;
    static List<KW_Buoyancy> buoyancyScripts = new List<KW_Buoyancy>(DefaultBuoyancyCapacity);

    public static void AddBuoyancyScript(KW_Buoyancy script)
    {
        if (!buoyancyScripts.Contains(script)) buoyancyScripts.Add(script);
    }

    public static void RemoveBuoyancyScript(KW_Buoyancy script)
    {
        if (buoyancyScripts.Contains(script)) buoyancyScripts.Remove(script);
    }

    public static List<KW_Buoyancy> GetBuoyancyScripts()
    {
        return buoyancyScripts;
    }

    public static bool IsRequiredBuoyancyRendering()
    {
        return buoyancyScripts.Count > 0;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    //-------------------------------------------  Lights  -------------------------------------------------------------------------------------------------------------------
    static List<Light> activeLights = new List<Light>();

    public static void AddLight(Light light)
    {
        if (!activeLights.Contains(light)) activeLights.Add(light);
    }

    public static void RemoveLight(Light light)
    {
        if (activeLights.Contains(light)) activeLights.Remove(light);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

}
