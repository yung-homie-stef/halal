using System;
using UnityEngine;
using UnityEngine.Playables;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.SceneManagement;

public class DemoGUI: MonoBehaviour
{
    public Camera cam;
    public Color textColor = Color.white;
    public float dpiScale = 1;
    public float buttonWidthScale = 1;
    public float fontScale = 14;
    public Light sun;
    public GameObject environment;
    public WaterSystem water;
#if UNITY_POST_PROCESSING_STACK_V2
    public PostProcessLayer posteffects;
#endif
    public GameObject terrain;
    public Terrain terrainTrees;
    // public SEGI segiPostFX;
    //public VolumetricLightRenderer volumeLightRend;
    //public VolumetricLight volumeLight;
    public PlayableDirector timeline;

	void Start () {

	}

    bool isButtonPressed;

    //void DisableSEGI()
    //{
    //    segiPostFX.updateGI = false;
    //}

    private void OnGUI()
    {
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.R))
            isButtonPressed = false;

        if (Input.GetKeyDown(KeyCode.R) && !isButtonPressed)
        {
            isButtonPressed = true;
            if (timeline != null)
            {
                timeline.time = 0;

                timeline.Play();
            }
        }

        float offset = 10;

        dpiScale = Mathf.Clamp(cam.scaledPixelHeight, 1080, 2160) / 1080f - 1; //full hd = 0 , 4k = 1
        dpiScale = Mathf.Lerp(1, 2f, dpiScale) * 1.1f;


        var buttonWidth = 180 * buttonWidthScale;
        offset *= dpiScale;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = textColor;
        labelStyle.fontSize = (int) (fontScale * dpiScale * 1.3f);

        GUIStyle customButton = new GUIStyle("button");
        customButton.fontSize = (int)(fontScale * dpiScale);

        GUI.skin.horizontalSlider.stretchHeight = true;
        GUI.skin.horizontalSlider.stretchWidth = true;
        GUI.skin.horizontalSlider.fixedHeight = 15 * dpiScale;
        GUI.skin.horizontalSlider.fixedWidth = buttonWidth * dpiScale;

        GUI.skin.horizontalSliderThumb.stretchHeight = true;
        GUI.skin.horizontalSliderThumb.stretchWidth = true;
        GUI.skin.horizontalSliderThumb.fixedWidth = 15 * dpiScale;
        GUI.skin.horizontalSliderThumb.fixedHeight = 15 * dpiScale;
        GUI.skin.horizontalSliderThumb.padding = new RectOffset(-100, -100, 300, 300);
        GUI.skin.horizontalSliderThumb.margin = new RectOffset(-100, -100, 300, 300);

        if (GUI.Button(new Rect(10 * dpiScale, offset, buttonWidth * dpiScale, 20 * dpiScale * 1.5f), "Load next scene"))
        {
            var currentSceneID = SceneManager.GetActiveScene().buildIndex;
            if (currentSceneID < SceneManager.sceneCountInBuildSettings - 1) currentSceneID++;
            else currentSceneID = 0;
            SceneManager.LoadScene(currentSceneID);
        }

        offset += 25 * dpiScale * 1.5f;

        if (sun != null && GUI.Button(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 20*dpiScale), (sun.shadows != LightShadows.None) ? "Shadows: ON" : "Shadows : OFF", customButton))
        {
            sun.shadows = (sun.shadows == LightShadows.None) ? LightShadows.Soft : LightShadows.None;
        }
        if (environment != null && GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), environment.gameObject.activeSelf ? "Environment: ON" : "Environment : OFF", customButton))
        {
            environment.gameObject.SetActive(!environment.gameObject.activeSelf);
        }

        if (terrain != null && GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), terrain.gameObject.activeSelf ? "Terrain: ON" : "Terrain : OFF", customButton))
        {
            terrain.gameObject.SetActive(!terrain.gameObject.activeSelf);
        }
        if (terrainTrees != null && GUI.Button(new Rect(10 * dpiScale, offset += 25 * dpiScale, buttonWidth * dpiScale, 20 * dpiScale), terrainTrees.drawTreesAndFoliage ? "Terrain Trees: ON" : "Terrain Trees: OFF", customButton))
        {
            terrainTrees.drawTreesAndFoliage = !terrainTrees.drawTreesAndFoliage;
        }

#if UNITY_POST_PROCESSING_STACK_V2
        if (posteffects != null && GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), posteffects.enabled ? "Posteffects: ON" : "Posteffects : OFF", customButton))
        {
            posteffects.enabled = !posteffects.enabled;
            //if (segiPostFX != null)
            //{
            //    if (!segiPostFX.enabled && posteffects.enabled)
            //    {
            //        segiPostFX.updateGI = true;
            //        CancelInvoke("DisableSEGI");
            //        Invoke("DisableSEGI", 3);
            //    }
            //    segiPostFX.enabled = posteffects.enabled;
            //}
            if (volumeLightRend != null) volumeLightRend.enabled = posteffects.enabled;
            if (volumeLight != null)
            {
                if (volumeLight.enabled && !posteffects.enabled) volumeLight.GetComponent<Light>().RemoveAllCommandBuffers(); volumeLight.enabled = posteffects.enabled;
            }
        }
#endif

        if (GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), water.gameObject.activeSelf ? "Water rendering: ON" : "Water rendering : OFF", customButton))
        {
            water.gameObject.SetActive(!water.gameObject.activeSelf);
        }

        if (!water.gameObject.activeSelf) return;

        offset += 35 * dpiScale;
        GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "Transparent", labelStyle);
        water.Transparent = GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.Transparent, 0.1f, 50);

        offset += 25 * dpiScale;
        GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "Turbidity", labelStyle);
        water.Turbidity = GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.Turbidity, 0.01f, 1);

        offset += 25 * dpiScale;
        GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "Tesselation factor", labelStyle);
        water.TesselationFactor = GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.TesselationFactor, 0.1f, 1);


        offset += 30;

        string reflName;
        if (water.ReflectionMode == WaterSystem.ReflectionModeEnum.CubemapReflection) reflName = "Reflection mode: sky";
        else if (water.ReflectionMode == WaterSystem.ReflectionModeEnum.ScreenSpaceReflection) reflName = "Reflection mode: SSR";
        else reflName = "Reflection mode: Planar";

        if (GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), reflName, customButton))
        {
            if (water.ReflectionMode == WaterSystem.ReflectionModeEnum.CubemapReflection) water.ReflectionMode = WaterSystem.ReflectionModeEnum.ScreenSpaceReflection;
            else if (water.ReflectionMode == WaterSystem.ReflectionModeEnum.ScreenSpaceReflection) water.ReflectionMode = WaterSystem.ReflectionModeEnum.PlanarReflection;
            else water.ReflectionMode = WaterSystem.ReflectionModeEnum.CubemapReflection;
            water.VariablesChanged();
        }

        offset += 35 * dpiScale;
        GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "Reflection resolution scale ", labelStyle);
        water.ReflectionTextureScale = GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.ReflectionTextureScale, 0.25f, 1);


        if (GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), water.UseFlowMap ? "Flowmap mode: ON" : "Flowmap mode : OFF", customButton))
        {
            water.UseFlowMap = !water.UseFlowMap;

            water.VariablesChanged();
        }


        if (GUI.Button(new Rect(10 * dpiScale, offset += 25 * dpiScale, buttonWidth * dpiScale, 20 * dpiScale), water.UseShorelineRendering ? "Shoreline rendering: ON" : "Shoreline rendering : OFF", customButton))
        {
            water.UseShorelineRendering = !water.UseShorelineRendering;
            water.VariablesChanged();
        }

        if (water.UseShorelineRendering)
        {
            offset += 35 * dpiScale;
            GUI.Label(new Rect(buttonWidth * dpiScale + 20 * dpiScale, offset - 3, buttonWidth * dpiScale, 20 * dpiScale), "shoreline foam quality", labelStyle);
            water.FoamLodQuality = (WaterSystem.QualityEnum) GUI.HorizontalSlider(new Rect(10 * dpiScale, offset, buttonWidth * dpiScale, 15), (int) water.FoamLodQuality, 0, 2);
        }



        if (GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), water.UseVolumetricLight ? "Volume Light mode: ON" : "Volume Light mode : OFF", customButton))
        {
            water.UseVolumetricLight = !water.UseVolumetricLight;
            water.VariablesChanged();
        }

        if (water.UseVolumetricLight)
        {
            offset += 35 * dpiScale;
            GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "volume light resolution", labelStyle);
            water.VolumetricLightResolutionScale = GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.VolumetricLightResolutionScale, 0.15f, 0.75f);

            offset += 25 * dpiScale;
            GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "volume light iteration", labelStyle);
            water.VolumetricLightIteration = (int)GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), (int)(water.VolumetricLightIteration), 2, 8);
        }

        if (GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), water.UseCausticEffect ? "Caustic mode: ON" : "Caustic mode : OFF", customButton))
        {
            water.UseCausticEffect = !water.UseCausticEffect;
            water.VariablesChanged();
        }

        if (water.UseCausticEffect)
        {
            offset += 35 * dpiScale;
            GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "caustic resolution", labelStyle);
            water.CausticTextureSize = (int)GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.CausticTextureSize, 256, 1024);

            offset += 25 * dpiScale;
            GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "caustic cascades", labelStyle);
            water.CausticActiveLods = (int)GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), (int)(water.CausticActiveLods), 1, 4);
        }

        if (GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), water.UseUnderwaterEffect ? "Underwater mode: ON" : "Underwater mode : OFF", customButton))
        {
            water.UseUnderwaterEffect = !water.UseUnderwaterEffect;
            water.VariablesChanged();
        }

        if (water.UseUnderwaterEffect)
        {
            offset += 35 * dpiScale;
            GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "underwater resolution scale", labelStyle);
            water.UnderwaterResolutionScale = GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.UnderwaterResolutionScale, 0.25f, 1);
        }

        if (GUI.Button(new Rect(10*dpiScale, offset += 25 * dpiScale, buttonWidth*dpiScale, 20*dpiScale), water.OffscreenRendering ? "Draw mode: Screen Space" : "Draw mode : Mesh", customButton))
        {
            water.OffscreenRendering = !water.OffscreenRendering;
            water.VariablesChanged();
        }

        if (water.OffscreenRendering)
        {
            offset += 35 * dpiScale;
            GUI.Label(new Rect(buttonWidth*dpiScale + 20*dpiScale, offset - 3, buttonWidth*dpiScale, 20*dpiScale), "screen space resolution scale", labelStyle);
            water.OffscreenRenderingResolution = GUI.HorizontalSlider(new Rect(10*dpiScale, offset, buttonWidth*dpiScale, 15), water.OffscreenRenderingResolution, 0.25f, 1);
        }

        if (GUI.Button(new Rect(10 * dpiScale, offset += 25 * dpiScale, buttonWidth * dpiScale, 20 * dpiScale), "QUIT DEMO", customButton))
        {
            Application.Quit();
        }

        //GUI.Toggle(new Rect(10*dpiScale, 15, 135 * dpiScale, 37), "Dfgdf");


        // terrain.heightmapPixelError = GUI.HorizontalSlider(new Rect(12, 147 + offset, 285, 15), terrain.heightmapPixelError, 1, 20*dpiScale0);

        // var shorelineScript = water.GetComponentInChildren<KW_ShorelineWaves>();

        //if (shorelineScript.VAT_Position != null) {Debug.Log("VAT Pos format: " + shorelineScript.VAT_Position.format);}
        // if (shorelineScript.VAT_Alpha != null) Debug.Log( "VAT Alpha format: " + shorelineScript.VAT_Alpha.format);
    }


}
