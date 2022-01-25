using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[Serializable]
public class WaterSystem : MonoBehaviour
{

    #region PublicVariables

    public Vector4 Test4 = Vector4.zero;
    public GameObject TestObj;

    //Color settings
    public bool ShowColorSettings = true;
   
    public float Transparent = 10;
    public Color WaterColor = new Color(175 / 255.0f, 225 / 255.0f, 240 / 255.0f);

    public Color TurbidityColor = new Color(10 / 255.0f, 110 / 255.0f, 100 / 255.0f);
    public float Turbidity = 0.08f;


    //Waves settings
    public bool ShowWaves;
    public FFT_GPU.SizeSetting FFT_SimulationSize = FFT_GPU.SizeSetting.Size_256;
    public bool UseMultipleSimulations = false;
   
    public float WindSpeed = 1.5f;
    public float WindRotation = 0;
    public float WindTurbulence = 0.75f;
    public float TimeScale = 1;

    //Reflection settings
    public bool ShowReflectionSettings;
    public bool ReflectSun = true;
    public ReflectionModeEnum ReflectionMode = ReflectionModeEnum.ScreenSpaceReflection;
    public float CubemapUpdateInterval = 6;
    public int CubemapCullingMask = 0;
    public int CubemapTextureSize = 128;
    public bool FixUnderwaterSkyReflection;
    public float ReflectionTextureScale = 0.75f;
    public float PlanarReflectionClipPlaneOffset = 0.002f;
    public int SSR_DepthHolesFillDistance = 5;
    public float ScreenSpaceClipPlaneOffset = 0.002f;
   // public bool SSR_ReflectImageEffects;
   // public float SSR_ReflectedImageEffectsHDR_Mul;


    //Volumetric settings
    public bool UseVolumetricLight = false;
    public bool ShowVolumetricLightSettings;
    public float VolumetricLightResolutionScale = 0.5f;
    public int VolumetricLightIteration = 4;
    //public float VolumetricLightDistance = 100;
    public float VolumetricLightBlurRadius = 1;
    ////InteractiveWaves settings
    //public bool UseInteractiveWaves = false;
    //public bool ShowInteractiveWaves;
    //public int InteractiveWavesAreaSize = 40;
    //public float InteractiveWavesQuality = 1;
    //public int InteractiveWavesFPS = 60;

    //FlowMap settings
    public bool UseFlowMap = false;
    public bool ShowFlowMap;
    public bool FlowMapInEditMode = false;
    public Vector3 FlowMapAreaPosition = new Vector3(0, 0, 0);
    //public Vector2 FlowMapOffset = new Vector2(0,  0);
    public int FlowMapAreaSize = 200;
    public int FlowMapTextureResolution = 2048;
    public float FlowMapBrushStrength = 0.75f;
    public float FlowMapSpeed = 1;
    public bool UseFluidsSimulation = false;
    public int FluidsAreaSize = 40;
    //public int FluidsObstacleAreaSize = 400;
    public int FluidsSimulationIterrations = 2;
    public int FluidsTextureSize = 1024;
    public int FluidsSimulationFPS = 60;
    public float FluidsSpeed = 1;
    public float FluidsFoamStrength = 0.5f;

    //Dynamic waves settings
    public bool UseDynamicWaves = false;
    public bool ShowDynamicWaves;
    public int DynamicWavesAreaSize = 25;
    public int DynamicWavesSimulationFPS = 60;
    public int DynamicWavesResolutionPerMeter = 40;
    public float DynamicWavesPropagationSpeed = 0.5f;
    //public float DynamicWavesQuality = 1;
    // public QualityEnum DynamicWavesQuality = QualityEnum.High;
    public bool UseDynamicWavesRainEffect;
    public float DynamicWavesRainStrength = 0.2f;


    //Shoreline settings
    public bool UseShorelineRendering = false;
    public bool ShowShorelineMap;
    public QualityEnum FoamLodQuality = QualityEnum.Medium;
    public bool UseFoamShadows = true;
    public bool ShorelineInEditMode = false;
    public Vector3 ShorelineAreaPosition;
    public int ShorelineAreaSize = 512;
    public QualityEnum ShorelineCurvedSurfacesQuality = QualityEnum.Medium;
    public int OrthoDepthAreaSize = 512;
    public int OrthoDepthTextureResolution = 2048;


    //Caustic settings
    public bool UseCausticEffect = false;
    public bool ShowCausticEffectSettings;
    public bool UseCausticBicubicInterpolation = true;
    public bool UseCausticDispersion = true;
    public int CausticTextureSize = 768;
    public int CausticMeshResolution = 320;
    public int CausticActiveLods = 4;
    public float CausticStrength = 1;
   
    public bool UseDepthCausticScale;
    public bool CausticDepthScaleInEditMode = false;
    public float CausticDepthScale = 1;
    public Vector3 CausticOrthoDepthPosition = Vector3.positiveInfinity;
    public int CausticOrthoDepthAreaSize = 512;
    public int CausticOrthoDepthTextureResolution = 2048;
   

    //Underwater settings
    public bool UseUnderwaterEffect = true;
    public bool ShowUnderwaterEffectSettings;
    public bool UseHighQualityUnderwater = false;
    public bool UseUnderwaterBlur = true;
    public float UnderwaterResolutionScale = 0.5f;
    public float UnderwaterBlurRadius = 1.75f;


    //Rendering settings
    public bool ShowRendering;
    public WaterMeshTypeEnum WaterMeshType;
    public bool UseFiltering = true;
    public AnisotropicEnum AnisotropicFiltering = AnisotropicEnum.None;
    public bool OffscreenRendering;
    public AntialiasingEnum OffscreenRenderingAA = AntialiasingEnum.x4;
    
    public float OffscreenRenderingResolution = 1.0f;
    public bool DrawToPosteffectsDepth;
    public int MeshQuality = 10;
    public Vector3 MeshSize = new Vector3(10, 10, 10);
    public bool UseTesselation = true;
    public float TesselationFactor = 0.6f;
    public float TesselationMaxDistance = 1000f;

    public enum QualityEnum
    {
        High = 0,
        Medium = 1,
        Low = 2,
    }

    public enum ReflectionModeEnum
    {
        CubemapReflection,
        PlanarReflection,
        ScreenSpaceReflection,
    }

    public enum WaterMeshTypeEnum
    {
        Infinite,
        Finite,
       // CustomMesh
    }

    public enum AntialiasingEnum
    {
        None = 1,
        x2 = 2,
        x4 = 4,
        x8 = 8
    }

    public enum AnisotropicEnum
    {
        None = 0,
        x2 = 2,
        x4 = 4,
        x8 = 8,
        x16 = 16
    }

    #endregion

    public string _waterGUID;
    private string waterGUID
    {
        get
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_waterGUID)) _waterGUID = UnityEditor.GUID.Generate().ToString();
            return _waterGUID;
#else
            if (string.IsNullOrEmpty(_waterGUID)) Debug.LogError("Water GUID is empty ");
            Debug.Log("Water GUID is empty " + _waterGUID);
            return _waterGUID;
#endif
        }
    }

#region private variables



    public Camera currentCamera;
    private Camera sceneCamera;
    private float prevWindSpeed = -1;
    int causticTexSizeScaledByWind = -1;
    int causticTrisCountScaledByWind = -1;


    private GameObject causticDecalGO;
    private GameObject causticMeshGO;
    private GameObject causticCameraGO;



    //private FFT_GPU fftGPU;
    //private FFT_GPU fftGPU_LOD1;
    //private FFT_GPU fftGPU_LOD2;
    //private FFT_GPU fftGPU_Detailed;

    //private KW_ComputeOrthoDepth computeOrthoDepth = new KW_ComputeOrthoDepth();

   // [SerializeField]




    private const string WaterShaderName = "KriptoFX/Water/Water";
    private const string WaterTesselatedShaderName = "KriptoFX/Water/Water(Tesselated)";
    private const string PlaneDepth_ShaderName = "Hidden/KriptoFX/Water/KW_DepthOtrhoPlane";
    private const string DistanceField_ShaderName = "Hidden/KriptoFX/Water30/KW_ComputeDistanceField";
    private const string Shoreline_ShaderName = "Hidden/KriptoFX/Water/ComputeShoreline";
    private const string interactiveWavesShaderName = "Hidden/KriptoFX/Water/KW_InteractiveWaves";



    //const string CausticComputeDX11ShaderName = "Hidden/KriptoFX/Water30/ComputeCaustic_DX11";


    private Material planeDepthMaterial;
    //private Material distanceFieldMaterial;
    // private Material shorelineMaterial;
    private Material interactiveWavesMaterial;








    //private KW_OnWillRenderWithoutRenderer updateHelper;

    private bool isGameView;
    private bool prevGameView;


    private const float UpdatePositionEveryMeters = 2.5f;
    private bool IsPositionMatrixChanged;
    private const float UpdateDepthPositionEveryMeters = 200;


    #endregion


    //------------------------------------------------------- new -----------------------------------------------------------------------------------------------

    [HideInInspector] public GameObject tempGameObject;
    public int BakedFluidsSimPercentPassed;

    public Mesh currentWaterMesh;
    public GameObject waterMeshGO;
    

    private MeshRenderer waterMeshRenderer;
    private MeshFilter waterMeshFilter;

    public List<Material> waterSharedMaterials;
    private Dictionary<ComputeShader, List<int>> waterSharedComputeShaders;

    public Material waterMaterial;
    // private KW_InteractiveWaves interactiveWaves;
    bool isWaterInitialized;
    bool isBuoyancyDataReadCompleted;
    bool isUnderwaterMaskRenderingEnabled;

    #region  properties

    private FFT_GPU _fft_lod0;
    private FFT_GPU fft_lod0
    {
        get
        {
            if (_fft_lod0 == null) _fft_lod0 = tempGameObject.AddComponent<FFT_GPU>();
            else if (!_fft_lod0.enabled) _fft_lod0.enabled = true;
            return _fft_lod0;
        }
    }

    private FFT_GPU _fft_lod1;
    private FFT_GPU fft_lod1
    {
        get
        {
            if (_fft_lod1 == null) _fft_lod1 = tempGameObject.AddComponent<FFT_GPU>();
            else if (!_fft_lod1.enabled) _fft_lod1.enabled = true;
            return _fft_lod1;
        }
    }

    private FFT_GPU _fft_lod2;
    private FFT_GPU fft_lod2
    {
        get
        {
            if (_fft_lod2 == null) _fft_lod2 = tempGameObject.AddComponent<FFT_GPU>();
            else if (!_fft_lod2.enabled) _fft_lod2.enabled = true;
            return _fft_lod2;
        }
    }

    private KW_FFT_HeightData _fft_HeightData;
    private KW_FFT_HeightData fft_HeightData
    {
        get
        {
            if (_fft_HeightData == null) _fft_HeightData = tempGameObject.AddComponent<KW_FFT_HeightData>();
            else if (!_fft_HeightData.enabled) _fft_HeightData.enabled = true;
            return _fft_HeightData;
        }
    }

    private KW_FlowMap _flowMap;
    KW_FlowMap flowMap
    {
        get
        {
            if (_flowMap == null) _flowMap = tempGameObject.AddComponent<KW_FlowMap>();
            else if (!_flowMap.enabled) _flowMap.enabled = true;
            return _flowMap;
        }
    }

    private KW_FluidsSimulation2D _fluidsSimulation;
    KW_FluidsSimulation2D fluidsSimulation
    {
        get
        {
            if (_fluidsSimulation == null) _fluidsSimulation = tempGameObject.AddComponent<KW_FluidsSimulation2D>();
            else if (!_fluidsSimulation.enabled) _fluidsSimulation.enabled = true;
            return _fluidsSimulation;
        }
    }

    private KW_DynamicWaves _dynamicWaves;
    KW_DynamicWaves dynamicWaves
    {
        get
        {
            if (_dynamicWaves == null) _dynamicWaves = tempGameObject.AddComponent<KW_DynamicWaves>();
            else if (!_dynamicWaves.enabled) _dynamicWaves.enabled = true;
            return _dynamicWaves;
        }
    }
    private KW_CausticRendering _causticRendering;
    public KW_CausticRendering causticRendering
    {
        get
        {
            if (_causticRendering == null) _causticRendering = tempGameObject.AddComponent<KW_CausticRendering>();
            else if (!_causticRendering.enabled) _causticRendering.enabled = true;
            return _causticRendering;
        }
    }

    private KW_CameraReflection_URP _cameraReflection;
    private KW_CameraReflection_URP cameraReflection
    {
        get
        {
            if (_cameraReflection == null) _cameraReflection = tempGameObject.AddComponent<KW_CameraReflection_URP>();
            else if (!_cameraReflection.enabled) _cameraReflection.enabled = true;
            return _cameraReflection;
        }
    }

    private KW_ShorelineWaves _shorelineMap;
    private KW_ShorelineWaves shorelineMap
    {
        get
        {
            if (_shorelineMap == null) _shorelineMap = tempGameObject.AddComponent<KW_ShorelineWaves>();
            else if (!_shorelineMap.enabled) _shorelineMap.enabled = true;
            return _shorelineMap;
        }
    }

    #endregion

    private int prevMeshQuality = -1;
   
    public enum AsyncInitializingStatusEnum
    {
        NonInitialized,
        StartedInitialize,
        Initialized,
        Failed,
        BakingStarted
    }

    AsyncInitializingStatusEnum shoreLineInitializingStatus;
    AsyncInitializingStatusEnum flowmapInitializingStatus;
    AsyncInitializingStatusEnum causticInitializingStatus;
    AsyncInitializingStatusEnum orthoDepthInitializingStatus;
    AsyncInitializingStatusEnum fluidsSimInitializingStatus;

    private const int waterLayer = 4; //water layer
    private const int waterCullingMask = 1<<4; //water layer

    private const float DomainSize = 10f;
    private const float DomainSize_LOD1 = 40f;
    private const float DomainSize_LOD2 = 160f;

    private const float VolumeLightDistanceLimitMin = 3;
    private const float VolumeLightDistanceLimitMax = 40;
    private const float VolumeLightBlurLimitMin = 1;
    private const float VolumeLightBlurLimitMax = 4.5f;

    private const float MaxTesselationFactor = 10;
    const int BakeFluidsLimitFrames = 200;
    int currentBakeFluidsFrames = 0;
    private const int DepthMaskTextureHeightLimit = 540; //fullHD * 0.5 enough even for 4k

    //bool isDynamicWavesInvoreRepeatingStarted;
    //bool isFluidsInvoreRepeatingStarted;
    KW_CustomFixedUpdate fixedUpdateFluids;
    KW_CustomFixedUpdate fixedUpdateBakingFluids;
    KW_CustomFixedUpdate fixedUpdateDynamicWaves;

    #region  ShaderID

    private int ID_transparent = Shader.PropertyToID("KW_Transparent");
    private int ID_turbidity = Shader.PropertyToID("KW_Turbidity");
    private int ID_turbidityColor = Shader.PropertyToID("KW_TurbidityColor");
    private int ID_waterColor = Shader.PropertyToID("KW_WaterColor");

    private int ID_TesselationMaxDisplace = Shader.PropertyToID("_TesselationMaxDisplace");
    private int ID_tesselationFactor = Shader.PropertyToID("_TesselationFactor");
    private int ID_tesselationMaxDistance = Shader.PropertyToID("_TesselationMaxDistance");

    private int ID_KW_ShadowDistance = Shader.PropertyToID("KW_ShadowDistance");
    private int ID_KW_NormalScattering_Lod = Shader.PropertyToID("KW_NormalScattering_Lod");
    private int ID_KW_WaterPosition = Shader.PropertyToID("KW_WaterPosition");
    private int ID_KW_ViewToWorld = Shader.PropertyToID("KW_ViewToWorld");
    private int ID_KW_ProjToView = Shader.PropertyToID("KW_ProjToView");
    private int ID_KW_CameraMatrix = Shader.PropertyToID("KW_CameraMatrix");

    private int ID_KW_WaterFarDistance = Shader.PropertyToID("KW_WaterFarDistance");
    private int ID_FFT_Size_Normalized = Shader.PropertyToID("KW_FFT_Size_Normalized");
    private int ID_KW_FlowMapSize = Shader.PropertyToID("KW_FlowMapSize");
    private int ID_KW_FlowMapOffset = Shader.PropertyToID("KW_FlowMapOffset");
    private int ID_KW_FlowMapSpeed = Shader.PropertyToID("KW_FlowMapSpeed");
    private int ID_KW_FlowMapFluidsStrength = Shader.PropertyToID("KW_FlowMapFluidsStrength");
    private int ID_WindSpeed = Shader.PropertyToID("KW_WindSpeed");
    private int ID_PlanarReflectionClipOffset = Shader.PropertyToID("KW_PlanarReflectionClipOffset");
    private int ID_KW_SSR_ClipOffset = Shader.PropertyToID("KW_SSR_ClipOffset");
#endregion

#region ShaderKeywords

    private string Keyword_MultipleSimulation = "USE_MULTIPLE_SIMULATIONS";
    private string Keyword_FlowMapEditMode = "KW_FLOW_MAP_EDIT_MODE";
    private string Keyword_FlowMap = "KW_FLOW_MAP";
    private string Keyword_FlowMapFluids = "KW_FLOW_MAP_FLUIDS";
    private string Keyword_DynamicWaves = "KW_DYNAMIC_WAVES";
    private string Keyword_Caustic = "USE_CAUSTIC";
    private string Keyword_ReflectSun = "REFLECT_SUN";
    private string Keyword_UseVolumetricLight = "USE_VOLUMETRIC_LIGHT";
    private string Keyword_FixUnderwaterSkyReflection = "FIX_UNDERWATER_SKY_REFLECTION";
    private string Keyword_Filtering = "USE_FILTERING";

#endregion

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        KW_WaterDynamicScripts.SetCurrentWater(this);
    }

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        var isGameView = isGameViewWindowSelected();
        if (isGameView)
        {
            currentCamera = Camera.main;
            UpdateManual(context);
        }
        else if (cam.cameraType == CameraType.SceneView)
        {
            currentCamera = cam;
            UpdateManual(context); 
        }
    }

    void InitializeWater()
    {
        //Debug.Log("Water.OnEnable");
#if KWS_DEBUG
        tempGameObject = new GameObject("TempWaterResources") { hideFlags = HideFlags.DontSave };
#else
        tempGameObject = new GameObject("TempWaterResources") { hideFlags = HideFlags.HideAndDontSave };
#endif
        tempGameObject.transform.parent = transform;
        tempGameObject.transform.localPosition = Vector3.zero;

        waterSharedMaterials = new List<Material>();
        waterSharedComputeShaders = new Dictionary<ComputeShader, List<int>>();

        InitializeWaterMeshGO();
        InitializeMesh();
        InitializeWaterMaterial(UseTesselation);

        fft_HeightData.IsDataReadCompleted += () => isBuoyancyDataReadCompleted = true;

        isWaterInitialized = true;
        
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;

        if (!isWaterInitialized) return;
        // Debug.Log("Water.OnDisable");

        if (_fft_lod0 != null) _fft_lod0.Release();
        if (_fft_lod1 != null) _fft_lod1.Release();
        if (_fft_lod2 != null) _fft_lod2.Release();
        if (_fft_HeightData != null) _fft_HeightData.Release();
        if (_cameraReflection != null) _cameraReflection.Release();
        if (_shorelineMap != null) _shorelineMap.Release();
        if (_flowMap != null) _flowMap.Release();
        if (_fluidsSimulation != null) _fluidsSimulation.Release();
        if (_dynamicWaves != null) _dynamicWaves.Release();
        if (_causticRendering != null) _causticRendering.Release();

        KW_Extensions.SafeDestroy(waterMaterial);
        KW_Extensions.SafeDestroy(currentWaterMesh);
        KW_Extensions.SafeDestroy(waterMeshGO);
        KW_Extensions.SafeDestroy(tempGameObject);

        waterSharedMaterials.Clear();
        waterSharedComputeShaders.Clear();

        prevMeshQuality = -1;
        shoreLineInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        flowmapInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        causticInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        orthoDepthInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        fluidsSimInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;

        isWaterInitialized = false;
        //Resources.UnloadUnusedAssets();
    }

    public void VariablesChanged()
    {
        //if (IsWaterSettingsRequiredUpdateCommandBuffers())
        //{
        //    RemoveCommandBuffersFromCameras();
        //    commandBuffers.Clear();
        //}
    }

    async void UpdateManual(ScriptableRenderContext context)
    {
        if (!isWaterInitialized)
        {
            InitializeWater();
        }

        if (!isWaterInitialized) return;

#if UNITY_PIPELINE_URP
        KW_WaterDynamicScripts.SetCurrentWaterContext(context);
#endif
        KW_Extensions.UpdateDeltaTime();
        var currentDeltaTime = KW_Extensions.DeltaTime();
        if (currentDeltaTime < 0.0001f) return;
  
        UpdateWaterPos();
        RenderFFT(UseMultipleSimulations);

        if (KW_WaterDynamicScripts.IsRequiredBuoyancyRendering()) UpdateFFT_HeightData();
        else isBuoyancyDataReadCompleted = false;

        if (UseShorelineRendering)
        {
            if (shoreLineInitializingStatus == AsyncInitializingStatusEnum.NonInitialized)
            {
                shoreLineInitializingStatus = AsyncInitializingStatusEnum.StartedInitialize;

                RenderShorelineWavesWithFoam();
            }
            shorelineMap.UpdateLodLevels(currentCamera.transform.position, (int)FoamLodQuality, UseFoamShadows);
        }

        if (UseFlowMap)
        {
            if (flowmapInitializingStatus == AsyncInitializingStatusEnum.NonInitialized)
            {
                flowmapInitializingStatus = AsyncInitializingStatusEnum.StartedInitialize;
                await ReadFlowMap();
            }
        }

        if (UseFlowMap && UseFluidsSimulation && fluidsSimInitializingStatus == AsyncInitializingStatusEnum.NonInitialized)
        {
            fluidsSimInitializingStatus = AsyncInitializingStatusEnum.StartedInitialize;
            await ReadPrebakedFluidsSimulation();
        }

        if (UseFlowMap && UseFluidsSimulation)
        {
            if (fixedUpdateFluids == null) fixedUpdateFluids = new KW_CustomFixedUpdate(RenderFluidsSimulation, 1);
            fixedUpdateFluids.Update(currentDeltaTime, 1.0f / FluidsSimulationFPS);
        }

        if(fluidsSimInitializingStatus == AsyncInitializingStatusEnum.BakingStarted)
        {
            if (fixedUpdateBakingFluids == null) fixedUpdateBakingFluids = new KW_CustomFixedUpdate(BakeFluidSimulationFrame, 2);
            fixedUpdateBakingFluids.Update(currentDeltaTime, 1.0f / 60f);
        }

        if (UseDynamicWaves)
        {
            if (fixedUpdateDynamicWaves == null) fixedUpdateDynamicWaves = new KW_CustomFixedUpdate(RenderDynamicWaves, 2);
            fixedUpdateDynamicWaves.Update(currentDeltaTime, 1.0f / DynamicWavesSimulationFPS);
        }

        if (UseCausticEffect) RenderCausticEffect(context);
        if (ReflectionMode == ReflectionModeEnum.PlanarReflection) RenderPlanarReflection(context);
        if (ReflectionMode == ReflectionModeEnum.CubemapReflection || ReflectionMode == ReflectionModeEnum.ScreenSpaceReflection) RenderSkyReflection(context);

        if (OffscreenRendering)
        {
            waterMeshRenderer.enabled = false;
        }
        else waterMeshRenderer.enabled = true;
        UpdateShaderParameters();
    }


    void InitializeWaterMeshGO()
    {
        waterMeshGO = new GameObject("WaterMesh");
        waterMeshGO.hideFlags = HideFlags.DontSave;
        waterMeshGO.layer = waterLayer;
        waterMeshGO.transform.parent = tempGameObject.transform;
        waterMeshGO.transform.localPosition = Vector3.zero;
        waterMeshRenderer = waterMeshGO.AddComponent<MeshRenderer>();

        waterMeshFilter = waterMeshGO.AddComponent<MeshFilter>();
    }

    public void InitializeMesh()
    {
        float farDist = 1000;
        var mainCam = Camera.main;
        if (mainCam != null) farDist = mainCam.farClipPlane;

        int quadsPerStartSize = (MeshQuality + 1) * 4;
        if(currentWaterMesh != null) KW_Extensions.SafeDestroy(currentWaterMesh);
        if(WaterMeshType == WaterMeshTypeEnum.Infinite) currentWaterMesh = KW_MeshGenerator.GeneratePlane(DomainSize * 2, quadsPerStartSize, farDist);
        if (WaterMeshType == WaterMeshTypeEnum.Finite) currentWaterMesh = KW_MeshGenerator.GenerateFinitePlane(quadsPerStartSize, MeshSize);
        KW_Extensions.SafeDestroy(waterMeshFilter.sharedMesh);
        waterMeshFilter.sharedMesh = currentWaterMesh;
        waterMeshRenderer.sharedMaterial = waterMaterial;
    }

    public void InitializeWaterMaterial(bool tryUseTesselation)
    {
        KW_Extensions.SafeDestroy(waterMaterial);
        var currentWaterShaderName = (tryUseTesselation && SystemInfo.graphicsShaderLevel >= 46) ? WaterTesselatedShaderName : WaterShaderName;
        waterMaterial = KW_Extensions.CreateMaterial(currentWaterShaderName);
        if (!waterSharedMaterials.Contains(waterMaterial)) waterSharedMaterials.Add(waterMaterial);
        if (waterMeshRenderer != null) waterMeshRenderer.sharedMaterial = waterMaterial;
    }

    bool isGameViewWindowSelected()
    {
#if UNITY_EDITOR
        
        if (Application.isPlaying) return true;
        if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Game") return true;
        else return false;
#else
        return true;
#endif
    }

    void RenderFFT(bool useMultipleSimulations)
    {
        var time = KW_Extensions.Time();
        time *= TimeScale;
        //time = 0.001f;
        var windDir = Mathf.Lerp(0.05f, 0.5f, WindTurbulence);
        int fftSize = (int) FFT_SimulationSize;
        var timeScaleRelativeToFFTSize = (Mathf.RoundToInt(Mathf.Log(fftSize, 2)) - 5)/4.0f;


        float lod0_Time = Mathf.Lerp(time, time, WindTurbulence);
        lod0_Time = Mathf.Lerp(lod0_Time, lod0_Time * 0.65f, timeScaleRelativeToFFTSize);


        fft_lod0.ComputeFFT(FFT_GPU.LodPrefix.LOD0, fftSize, (int)AnisotropicFiltering, DomainSize, windDir, Mathf.Clamp(WindSpeed, 0, 2), WindRotation * Mathf.Deg2Rad, lod0_Time, waterSharedMaterials, waterSharedComputeShaders);

        if (useMultipleSimulations)
        {
            var fftSizeLod = (FFT_SimulationSize == FFT_GPU.SizeSetting.Size_512) ? 128 : 64;

            fft_lod1.ComputeFFT(FFT_GPU.LodPrefix.LOD1, fftSizeLod, 0, DomainSize_LOD1, windDir, Mathf.Clamp(WindSpeed, 0, 6), WindRotation * Mathf.Deg2Rad, time * 0.9f, waterSharedMaterials, waterSharedComputeShaders);
            fft_lod2.ComputeFFT(FFT_GPU.LodPrefix.LOD2, fftSizeLod, 0, DomainSize_LOD2, windDir, Mathf.Clamp(WindSpeed, 0, 40), WindRotation * Mathf.Deg2Rad, time * 0.4f, waterSharedMaterials, waterSharedComputeShaders);
        }
    }

    void UpdateFFT_HeightData()
    {
        fft_HeightData.AddMaterialsToWaterRendering(waterSharedMaterials);
        var heightDataSize = UseMultipleSimulations ? 256 : 64;
        fft_HeightData.UpdateHeightData(heightDataSize, UseMultipleSimulations ? DomainSize_LOD2 : DomainSize, transform.position.y);
    }

    public void BakeFluidSimulation()
    {
        fluidsSimInitializingStatus = AsyncInitializingStatusEnum.BakingStarted;
        currentBakeFluidsFrames = 0;
    }

    void BakeFluidSimulationFrame()
    {
        if (currentBakeFluidsFrames < BakeFluidsLimitFrames)
        {
            currentBakeFluidsFrames++;
            BakedFluidsSimPercentPassed = (int)((100f / BakeFluidsLimitFrames) * currentBakeFluidsFrames);
            for (int j = 0; j < 10; j++)
                fluidsSimulation.PrebakeSimulation(transform.position, FlowMapAreaSize, FluidsTextureSize * 2, FluidsSpeed, 0.1f, waterGUID);
        }
        else
        {
            fluidsSimulation.SavePrebakedSimulation(waterGUID);
            BakedFluidsSimPercentPassed = 0;
            ReadPrebakedFluidsSimulation();
            fluidsSimInitializingStatus = AsyncInitializingStatusEnum.Initialized;
            Debug.Log("Fluids obstacles saved!");
        }
    }

    public async Task ReadPrebakedFluidsSimulation()
    {
        await fluidsSimulation.ReadPrebakedSimulation(waterGUID);
        fluidsSimInitializingStatus = AsyncInitializingStatusEnum.Initialized;
    }



    void RenderFluidsSimulation()
    {
        if (fluidsSimInitializingStatus != AsyncInitializingStatusEnum.Initialized) return;
        fluidsSimulation.AddMaterialsToWaterRendering(waterSharedMaterials);

        for (int i = 0; i < FluidsSimulationIterrations; i++)
        {
            fluidsSimulation.RenderFluids(currentCamera, transform.position, FluidsAreaSize, FluidsTextureSize, FluidsSpeed, FluidsFoamStrength, waterGUID);
        }
    }

    void RenderDynamicWaves()
    {
        
        dynamicWaves.RenderWaves(currentCamera, DynamicWavesSimulationFPS, DynamicWavesAreaSize, DynamicWavesResolutionPerMeter, DynamicWavesPropagationSpeed, UseDynamicWavesRainEffect ? DynamicWavesRainStrength : 0);
        //dynamicWaves.RenderWaves(currentCamera, 1, DynamicWavesAreaSize);

    }

    void RenderPlanarReflection(ScriptableRenderContext context)
    {
        cameraReflection.RenderPlanar(context, currentCamera, transform.position, ReflectionTextureScale*0.75f, waterSharedMaterials);
    }

    void RenderSkyReflection(ScriptableRenderContext context)
    {
        cameraReflection.RenderCubemap(context, currentCamera, transform.position, CubemapUpdateInterval, CubemapCullingMask, CubemapTextureSize, FixUnderwaterSkyReflection, waterSharedMaterials);
    }

    void RenderCausticEffect(ScriptableRenderContext context)
    {
        causticRendering.AddMaterialsToWaterRendering(waterSharedMaterials);
        var dispersionStrength = 1 - (Mathf.RoundToInt(Mathf.Log((int)FFT_SimulationSize, 2)) - 5) / 4.0f; // 0 - 4 => 1-0
        causticRendering.Render(context, currentCamera, CausticStrength, CausticDepthScale, CausticTextureSize, CausticActiveLods, CausticMeshResolution,
            UseCausticBicubicInterpolation, UseCausticDispersion, UseDepthCausticScale, dispersionStrength, waterSharedMaterials, null, waterGUID);


        causticInitializingStatus = AsyncInitializingStatusEnum.Initialized;
    }

   

#region  Shoreline Methods

    public bool IsEditorAllowed()
    {
        if (tempGameObject == null) return false;
        else return true;
    }

  
    public async void RenderShorelineWavesWithFoam()
    {
        shorelineMap.AddMaterialsToWaterRendering(waterSharedMaterials);
        //var relativeShorelinePos = transform.TransformPoint(-ShorelineAreaPosition);
        var isInitialized = await shorelineMap.RenderShorelineWavesWithFoam(ShorelineAreaSize, ShorelineAreaPosition, waterGUID);
        shoreLineInitializingStatus = isInitialized ? AsyncInitializingStatusEnum.Initialized : AsyncInitializingStatusEnum.Failed;
    }

    public async Task BakeWavesToTexture()
    {
        var curvedSurfaceQualityScale = ShorelineCurvedSurfacesQuality == QualityEnum.High ? 20f : (ShorelineCurvedSurfacesQuality == QualityEnum.Medium ? 2.0f : 0.25f);
        await shorelineMap.BakeWavesToTexture(ShorelineAreaSize, ShorelineAreaPosition, curvedSurfaceQualityScale, transform.position, waterGUID);
    }


    public async Task InitialiseShorelineEditorResources()
    {
        await shorelineMap.InitializeShorelineResources(waterGUID);
    }

    public void SaveShorelineWavesParamsToDataFolder()
    {
        shorelineMap.SavesWavesParamsToDataFolder(waterGUID);
    }

    public void SaveShorelineToDataFolder()
    {
        shorelineMap.SaveWavesToDataFolder(waterGUID);
    }

    public void SaveShorelineDepth()
    {
        shorelineMap.SaveOrthoDepth(waterGUID);
    }

    public void ClearShorelineFoam()
    {
        shorelineMap.ClearFoam();
    }
    public void ClearShorelineWavesWithFoam()
    {
        shorelineMap.ClearShorelineWavesWithFoam(waterGUID);
    }

    public async Task<List<KW_ShorelineWaves.ShorelineWaveInfo>> GetShorelineWavesData()
    {
        return await shorelineMap.GetShorelineWavesData(waterGUID);
    }

#endregion

    void InitializeFlowMapEditorResources()
    {
        flowMap.InitializeFlowMapEditorResources(FlowMapTextureResolution, FlowMapAreaSize);
        shoreLineInitializingStatus = AsyncInitializingStatusEnum.Initialized;
    }

    public void DrawOnFlowMap(Vector3 brushPosition, Vector3 brushMoveDirection, float circleRadius, float brushStrength, bool eraseMode = false)
    {
        InitializeFlowMapEditorResources();
        flowMap.DrawOnFlowMap(brushPosition, brushMoveDirection, circleRadius, brushStrength, eraseMode);
        flowmapInitializingStatus = AsyncInitializingStatusEnum.BakingStarted;
    }

    public void RedrawFlowMap(int newTexRes, int newAreaSize)
    {
        InitializeFlowMapEditorResources();
        flowMap.RedrawFlowMap(newTexRes, newAreaSize);
    }


    public void SaveFlowMap()
    {
        InitializeFlowMapEditorResources();
        flowMap.SaveFlowMap(FlowMapAreaSize, waterGUID);
    }

    public async Task ReadFlowMap()
    {
        var isInitialized = await flowMap.ReadFlowMap(waterSharedMaterials, waterGUID);
        var flowData = flowMap.GetFlowMapDataFromFile();
        flowmapInitializingStatus = (isInitialized && flowData != null)? AsyncInitializingStatusEnum.Initialized : AsyncInitializingStatusEnum.Failed;
        if (flowData == null) return;
        FlowMapTextureResolution = flowData.TextureSize;
        FlowMapAreaSize = flowData.AreaSize;
    }

    public void Editor_SaveCausticDepth()
    {
        causticRendering.SaveOrthoDepth(waterGUID, CausticOrthoDepthPosition, CausticOrthoDepthAreaSize, CausticOrthoDepthTextureResolution);
        causticInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
    }

    public void Editor_SaveFluidsDepth()
    {
        fluidsSimulation.SaveOrthoDepth(waterGUID, transform.position, FlowMapAreaSize, FlowMapTextureResolution);
    }


    public void ClearFlowMap()
    {
        flowMap.ClearFlowMap(waterGUID);
    }


    private RenderingPath lastRenderingPath;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private bool lastVolumeLightUsing;
    private float lastVolumeLightResolutuion;
    private int lastVolumeLightIterations;
    private float lastVolumeLightDistance;
    private float lastVolumeLightBlurRadius;

    private bool lastCausticEffectUsing;

    private int lastLightsCount;
    private Camera lastRenderedCamera;
    private int lastMeshQuality;
    private float lastYPos;
    private bool lastTesselationUsing;
    private float lastTesselationFactor;
    private float lastTesselationMaxDistance;
    private ReflectionModeEnum lastReflectionMethod;
    private float lastReflectionTexResolution;

    private int last_SSR_DepthHolesFillDistance;
    private int lastCubemapTextureSize;
    private bool lastFixUnderwaterSkyReflection;

    private bool lastUnderwaterEffectUsing;
    private bool lastUseHighQualityUnderwater;
    private bool lastUnderwaterEffectBlurUsing;
    private float lastUnderwaterResolutionScale;
    private float lastUnderwaterEffectBlurRadius;

    private int lastAnisotropicFiltering;
    private bool lastOffscreenRendering;
    float lastScreenSpaceWaterResolution;
    int lastOffscreenRenderingAA;
    private bool lastDrawToPosteffectsDepth;
    WaterMeshTypeEnum lastWaterMeshType;

    
    private void ReleaseInactiveResources()
    {
        if (_fft_lod1 != null && _fft_lod1.enabled && !UseMultipleSimulations)
        {
            _fft_lod1.Release();
            _fft_lod1.enabled = false;
        }

        if (_fft_lod2 != null && _fft_lod2.enabled && !UseMultipleSimulations)
        {
            _fft_lod2.Release();
            _fft_lod2.enabled = false;
        }

        if (_shorelineMap != null && _shorelineMap.enabled && !UseShorelineRendering)
        {
            _shorelineMap.Release();
            _shorelineMap.enabled = false;
            shoreLineInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

        if (_causticRendering != null && _causticRendering.enabled && !UseCausticEffect)
        {
            _causticRendering.Release();
            _causticRendering.enabled = false;
            causticInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

        if (_flowMap != null && _flowMap.enabled && !UseFlowMap)
        {
            _flowMap.Release();
            _flowMap.enabled = false;
            flowmapInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

       
        if (_fluidsSimulation != null && (_fluidsSimulation.enabled && !UseFluidsSimulation || !UseFlowMap))
        {
            _fluidsSimulation.Release();
            _fluidsSimulation.enabled = false;
            fluidsSimInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
           // CancelInvoke("RenderFluidsSimulation");
        }

        if(_dynamicWaves != null && _dynamicWaves.enabled && !UseDynamicWaves)
        {
            _dynamicWaves.Release();
            _dynamicWaves.enabled = false;
         
        }

       
    }

    //------------------------------------------------------- new -----------------------------------------------------------------------------------------------


    public Vector3 waterWorldPos { get; private set; }


    private Dictionary<int, Vector2> DetailRelativeSize = new Dictionary<int, Vector2>()
    {
        { 512, new Vector2(256, 2) },
        { 256,  new Vector2(128, 3)},
        { 128,  new Vector2(64, 4)},
        { 64,  new Vector2(64, 4)},
        { 32,  new Vector2(64, 5)},
    };

    private const float TrisCountLodStep = 25;
    private const float TrisTexSizeLodStep = 16;
    private Vector4[] CausticTexTrisScale =
    {
        new Vector4(400, 1024, 140, 250),
        new Vector4(350, 768, 120, 200),
        new Vector4(300, 512, 100, 160),
        new Vector4(200, 300, 80, 130),
        new Vector4(100, 200, 60, 110),
    }; //512 - 32




    public Color GetAmbientSceneColor()
    {
        SphericalHarmonicsL2 harmonics = new SphericalHarmonicsL2();
        LightProbes.GetInterpolatedProbe(gameObject.transform.position, null, out harmonics);
        Color ambient = Color.black;
        ambient.r = harmonics[0, 0];
        ambient.g = harmonics[1, 0];
        ambient.b = harmonics[2, 0];
        
        return ambient;
    }

    public async void UpdateShaderParameters()
    {
        Shader.SetGlobalVector("Test4", Test4);

        Shader.SetGlobalFloat("KW_CameraRotation", currentCamera.transform.eulerAngles.y / 360f);
        // print(currentCamera.transform.eulerAngles.y / 360f);

        Shader.SetGlobalFloat(ID_KW_ShadowDistance, QualitySettings.shadowDistance);

        var currentTessFactor = UseTesselation ? Mathf.Lerp(2, MaxTesselationFactor, TesselationFactor) : 0;


        var maxTessCullDisplace = Mathf.Max(WindSpeed, 2);

        var fftSize       = (int)FFT_SimulationSize;
        var normalLodScale = Mathf.RoundToInt(Mathf.Log(fftSize, 2)) - 4;
        var scatterLod = Mathf.Lerp(normalLodScale / 2.0f + 0.5f, normalLodScale / 2.0f + 1.5f, Mathf.Clamp01(WindSpeed / 3f));

        var projToView = GL.GetGPUProjectionMatrix(currentCamera.projectionMatrix, true).inverse;
        projToView[1, 1] *= -1;

        var viewProjection = currentCamera.nonJitteredProjectionMatrix * currentCamera.transform.worldToLocalMatrix;
        var viewToWorld = currentCamera.cameraToWorldMatrix;

        float farDist = 500;
        var mainCam = Camera.main;
        if(mainCam != null) farDist = mainCam.farClipPlane * 0.5f;

        var waterPos = transform.position;

        float fftSizeNormalized = (Mathf.RoundToInt(Mathf.Log((int)FFT_SimulationSize, 2)) - 5) / 4.0f;

        Shader.SetGlobalMatrix(ID_KW_ViewToWorld, viewToWorld);
        Shader.SetGlobalMatrix(ID_KW_ProjToView, projToView);
        Shader.SetGlobalMatrix(ID_KW_CameraMatrix, viewProjection);

        var ambientColor = GetAmbientSceneColor();
    
        Shader.SetGlobalColor("KW_AmbientColor", ambientColor);
        float fluidsSpeed = FlowMapSpeed;
        if (UseFluidsSimulation && !FlowMapInEditMode) fluidsSpeed = FluidsSpeed * Mathf.Lerp(0.125f, 1.0f, FluidsSimulationIterrations / 4.0f);
 
        foreach (var mat in waterSharedMaterials)
        {
            if(mat == null) continue;

            mat.SetMatrix(ID_KW_ViewToWorld, viewToWorld);
            mat.SetMatrix(ID_KW_ProjToView, projToView);
            mat.SetMatrix(ID_KW_CameraMatrix, viewProjection);

            mat.SetVector(ID_KW_WaterPosition, waterPos);

            mat.SetColor(ID_turbidityColor, TurbidityColor);
            mat.SetColor(ID_waterColor, WaterColor);

            mat.SetFloat(ID_FFT_Size_Normalized, fftSizeNormalized);
            mat.SetFloat(ID_WindSpeed, WindSpeed);
            mat.SetFloat(ID_KW_NormalScattering_Lod, scatterLod);
            mat.SetFloat(ID_KW_WaterFarDistance, farDist);
            mat.SetFloat(ID_transparent, Transparent);
            mat.SetFloat(ID_turbidity, Turbidity);
            mat.SetFloat(ID_tesselationFactor, currentTessFactor);
            mat.SetFloat(ID_tesselationMaxDistance, TesselationMaxDistance);
            mat.SetFloat(ID_TesselationMaxDisplace, maxTessCullDisplace);

            if (ReflectSun) mat.EnableKeyword(Keyword_ReflectSun);
            else mat.DisableKeyword(Keyword_ReflectSun);

            if(UseVolumetricLight) mat.EnableKeyword(Keyword_UseVolumetricLight);
            else mat.DisableKeyword(Keyword_UseVolumetricLight);

            if(FixUnderwaterSkyReflection) mat.EnableKeyword(Keyword_FixUnderwaterSkyReflection);
            else mat.DisableKeyword(Keyword_FixUnderwaterSkyReflection);

            if (UseFlowMap)
            {
                mat.SetFloat(ID_KW_FlowMapSize, FlowMapAreaSize);
                mat.SetVector(ID_KW_FlowMapOffset, FlowMapAreaPosition);
                mat.SetFloat(ID_KW_FlowMapSpeed, fluidsSpeed);
            }

            if (UseCausticEffect && causticInitializingStatus == AsyncInitializingStatusEnum.Initialized) mat.EnableKeyword(Keyword_Caustic);
            else mat.DisableKeyword(Keyword_Caustic);

            if (UseMultipleSimulations) mat.EnableKeyword(Keyword_MultipleSimulation);
            else mat.DisableKeyword(Keyword_MultipleSimulation);

            if (FlowMapInEditMode) mat.EnableKeyword(Keyword_FlowMapEditMode);
            else mat.DisableKeyword(Keyword_FlowMapEditMode);

            if (UseFlowMap && !UseFluidsSimulation && (flowmapInitializingStatus == AsyncInitializingStatusEnum.Initialized || flowmapInitializingStatus == AsyncInitializingStatusEnum.BakingStarted)) mat.EnableKeyword(Keyword_FlowMap);
            else mat.DisableKeyword(Keyword_FlowMap);

            if (UseFlowMap && UseFluidsSimulation && (fluidsSimInitializingStatus == AsyncInitializingStatusEnum.Initialized || fluidsSimInitializingStatus == AsyncInitializingStatusEnum.BakingStarted))
            {
                mat.EnableKeyword(Keyword_FlowMapFluids);
                mat.SetFloat(ID_KW_FlowMapFluidsStrength, FluidsFoamStrength);
            }
            else
            {
                mat.DisableKeyword(Keyword_FlowMapFluids);
            }

            if (UseDynamicWaves) mat.EnableKeyword(Keyword_DynamicWaves);
            else mat.DisableKeyword(Keyword_DynamicWaves);

            if (UseShorelineRendering && shoreLineInitializingStatus == AsyncInitializingStatusEnum.Initialized)
            {
                mat.EnableKeyword("USE_SHORELINE");
            }
            else mat.DisableKeyword("USE_SHORELINE");

            if (ReflectionMode == ReflectionModeEnum.PlanarReflection)
            {
                mat.SetFloat(ID_PlanarReflectionClipOffset, PlanarReflectionClipPlaneOffset);
                mat.EnableKeyword("PLANAR_REFLECTION");
                mat.DisableKeyword("SSPR_REFLECTION");
            }
            else if (ReflectionMode == ReflectionModeEnum.ScreenSpaceReflection)
            {
                mat.DisableKeyword("PLANAR_REFLECTION");
                mat.EnableKeyword("SSPR_REFLECTION");
                mat.SetFloat(ID_KW_SSR_ClipOffset, ScreenSpaceClipPlaneOffset);
            }
            else
            {
                mat.DisableKeyword("PLANAR_REFLECTION");
                mat.DisableKeyword("SSPR_REFLECTION");
            }

            if (UseFiltering) mat.EnableKeyword(Keyword_Filtering);
            else mat.DisableKeyword(Keyword_Filtering);
        }

    }

    void UpdateWaterPos()
    {
        Shader.SetGlobalFloat("KW_Time", KW_Extensions.Time());
        if (WaterMeshType == WaterMeshTypeEnum.Infinite)
        {
            if (currentCamera != null)
            {
                var pos = waterMeshGO.transform.position;
                var camPos = currentCamera.transform.position;
               
                //transform.position = new Vector3(camPos.x, pos.y, camPos.z);
                var relativeToCamPos = new Vector3(camPos.x, pos.y, camPos.z);
                waterWorldPos = relativeToCamPos;
                if (Vector3.Distance(pos, relativeToCamPos) >= UpdatePositionEveryMeters)
                {
                    IsPositionMatrixChanged = true;
                    waterMeshGO.transform.position = relativeToCamPos;
                }

            }
        }
      
    }

    public void ResetWaterPos()
    {
        if(waterMeshGO != null) waterMeshGO.transform.localPosition = Vector3.zero;
    }

    public Vector3? GetWaterSurfaceHeight(Vector3 worldPosition)
    {
        if (!isWaterInitialized || !isBuoyancyDataReadCompleted) return worldPosition;
        return fft_HeightData.GetWaterSurfaceHeight(worldPosition);
    }

    public void EnableBuoyancyRendering()
    {
        isBuoyancyDataReadCompleted = false;
    }

    public void DisableBuoyancyRendering()
    {
        isBuoyancyDataReadCompleted = false;
    }


    void Update()
    {
        //if (!Application.isPlaying && isWaterInitialized) UpdateWaterPos();
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/Effects/Water System")]
    static void CreateCamera(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Water System");
        go.transform.position = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * 3f);
        go.AddComponent<WaterSystem>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}
