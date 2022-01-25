using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static KW_WaterOrthoDepth;

public class KW_ShorelineWaves : MonoBehaviour
{
    private const string shorelineWaveBakinng_shaderName = "Hidden/KriptoFX/Water/KW_ShorelineWavePosition";
    private const string VAT_shaderName = "Hidden/KriptoFX/Water/KW_FoamParticles";

    private const string path_shoreLineFolder = "ShorelineMaps";
    private const string path_shoreLineWavesData = "KW_ShorelineWavesData";
    private const string path_shorelineDepthData = "KW_Shoreline_DepthData";
    private const string path_shorelineDepth = "KW_Shoreline_Depth";
    private const string path_shorelineMapUV1 = "KW_Shoreline1_UV_Angle_Alpha";
    private const string path_shorelineMapData1 = "KW_BakedWaves1_TimeOffset_Scale";
    private const string path_shorelineMapUV2 = "KW_Shoreline2_UV_Angle_Alpha";
    private const string path_shorelineMapData2 = "KW_BakedWaves2_TimeOffset_Scale";

    private const string path_ToShorelineWaveTex = "Shoreline_Pos_14x15";
    private const string path_ToShorelineWaveNormTex = "Shoreline_Norm_14x15";
    private const string path_VAT_MeshTexLod0 = "VAT_Mesh_Lod0";
    private const string path_VAT_MeshTexLod1 = "VAT_Mesh_Lod1";
    private const string path_VAT_MeshTexLod2 = "VAT_Mesh_Lod2";
    private const string path_VAT_MeshTexLod3 = "VAT_Mesh_Lod3";
    private const string path_VAT_MeshTexLod4 = "VAT_Mesh_Lod4";
    private const string path_VAT_MeshTexLod5 = "VAT_Mesh_Lod5";
    private const string path_VAT_MeshTexLod6 = "VAT_Mesh_Lod6";
    private const string path_VAT_MeshTexLod7 = "VAT_Mesh_Lod7";
    private const string path_VAT_PositionTex = "VAT_Position";
    private const string path_VAT_AlphaTex = "VAT_Alpha";
    private const string path_VAT_RangeLookupTex = "VAT_RangeLookup";
    private const string path_VAT_OffsetTex = "BeachWaveParticlesOffset";

    private int ID_KW_ShorelineAreaPos = Shader.PropertyToID("KW_ShorelineAreaPos");

    private int ID_KW_ShorelineDepth = Shader.PropertyToID("KW_ShorelineDepthTex");
    private int ID_KW_ShorelineDepthOrthoSize = Shader.PropertyToID("KW_ShorelineDepthOrthoSize");
    private int ID_KW_ShorelineDepthNearFarDistance = Shader.PropertyToID("KW_ShorelineDepth_Near_Far_Dist");
    

    private int ID_KW_ShorelineWaveDisplacement = Shader.PropertyToID("KW_ShorelineWaveDisplacement");
    private int ID_KW_ShorelineWaveNormal = Shader.PropertyToID("KW_ShorelineWaveNormal");
    private int ID_KW_ShorelineVAT_Mesh = Shader.PropertyToID("KW_Vat_Mesh");
    private int ID_KW_ShorelineVAT_Position = Shader.PropertyToID("KW_VAT_Position");
    private int ID_KW_ShorelineVAT_Alpha = Shader.PropertyToID("KW_VAT_Alpha");
    private int ID_KW_ShorelineVAT_RangeLookup = Shader.PropertyToID("KW_VAT_RangeLookup");
    private int ID_KW_ShorelineVAT_Offset = Shader.PropertyToID("KW_VAT_Offset");
    private int ID_KW_ShorelineMapSize = Shader.PropertyToID("KW_WavesMapSize");

    private Texture2D shorelineWaveDisplacementTex;
    private Texture2D shorelineWaveNormalTex;
    private Mesh VAT_Mesh_Lod0;
    private Mesh VAT_Mesh_Lod1;
    private Mesh VAT_Mesh_Lod2;
    private Mesh VAT_Mesh_Lod3;
    private Mesh VAT_Mesh_Lod4;
    private Mesh VAT_Mesh_Lod5;
    private Mesh VAT_Mesh_Lod6;
    private Mesh VAT_Mesh_Lod7;
    public Texture2D VAT_Position;
    public Texture2D VAT_Alpha;
    private Texture2D VAT_RangeLookup;
    private Texture2D VAT_Offset;

    private Camera cam;
    private GameObject camGO;

    private Camera depthCam;
    private GameObject depthCamGO;

    Material waveMaterial;
    private Material vatMaterial;
    private GameObject quad;

    public RenderTexture depth_rt;
    public RenderTexture waves1_rt;
    public RenderTexture waves2_rt;
    public RenderTexture wavesData1_rt;
    public RenderTexture wavesData2_rt;

    public Texture2D depth_tex;
    public Texture2D waves_tex1;
    public Texture2D waves_tex2;
    public Texture2D wavesData1_tex;
    public Texture2D wavesData2_tex;

    List<GameObject> wavesObjects =  new List<GameObject>();
    private Dictionary<int, CustomLod> foamGameObjects = new Dictionary<int, CustomLod>();
    List<GameObject> foamLodsForLateDeactivation = new List<GameObject>();
    List<ShorelineWaveInfo> _shorelineWavesData;
    OrthoDepthParams depthParams;

    private bool isInitializedEditorResources;
    private bool isInitializedShorelineResources;

    private const float HeightWave1 = 7000;
    private const float HeightWave2 = 7010;
    private const float GlobalTimeOffsetMultiplier = 34;
    private const float GlobalTimeSpeedMultiplier = 1.0f;
    private const int nearPlaneDepth = -2;
    private const int farPlaneDepth = 100;
    private const int depthTextureSize = 4096;

    [Serializable]
    public class ShorelineWaveInfo
    {
        [SerializeField] public int ID;
        [SerializeField] public float PositionX;
        [SerializeField] public float PositionZ;
        [SerializeField] public float EulerRotation;
        [SerializeField] public float ScaleX = 14;
        [SerializeField] public float ScaleY = 4.5f;
        [SerializeField] public float ScaleZ = 16;
        [SerializeField] public float TimeOffset = 0;
        [SerializeField] public float DefaultScaleX = 14;
        [SerializeField] public float DefaultScaleY = 4.5f;
        [SerializeField] public float DefaultScaleZ = 16;
    }
  
   
    public float[] LodFoamSize = new float[] { 0, 0.02f, 0.045f, 0.07f, 0.11f, 0.15f, 0.22f, 0.3f };
    public float[] LodDistances_High = new float[] { 40, 43, 46, 50, 54, 60, 70, 90 };
    public float[] LodDistances_Medium = new float[] { 20, 25, 30, 35, 40, 45, 50, 70 };
    //public float[] LodFoamSize_Medium = new float[] { 0, 0.02f, 0.045f, 0.07f, 0.11f, 0.15f, 0.22f, 0.3f };
    public float[] LodDistances_Low = new float[] { 10, 13, 16, 20, 24, 30, 40, 60 };
    //public float[] LodFoamSize_Low = new float[] { 0, 0.02f, 0.045f, 0.07f, 0.11f, 0.15f, 0.22f, 0.3f };

    class CustomLod
    {
        public GameObject Parent;
        public GameObject[] LodObjects;
        public GameObject[] ShadowObjects;

        public Vector3 Position;
        public int ActiveLod = -1;
    }

    public void AddMaterialsToWaterRendering(List<Material> waterSharedMaterials)
    {
        if (vatMaterial == null) vatMaterial = KW_Extensions.CreateMaterial(VAT_shaderName);
        if (!waterSharedMaterials.Contains(vatMaterial)) waterSharedMaterials.Add(vatMaterial);
    }

    public async Task<List<ShorelineWaveInfo>> GetShorelineWavesData(string GUID)
    {
        if (_shorelineWavesData == null || _shorelineWavesData.Count == 0)
        {
            var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

            var pathToShorelineDirectory = Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID);
            if (!Directory.Exists(pathToShorelineDirectory))
            {
                _shorelineWavesData = new List<ShorelineWaveInfo>();
                return _shorelineWavesData;
            }

            _shorelineWavesData = await KW_Extensions.DeserializeFromFile<List<ShorelineWaveInfo>>(Path.Combine(pathToShorelineDirectory, path_shoreLineWavesData));
            if(_shorelineWavesData == null) _shorelineWavesData = new List<ShorelineWaveInfo>();
        }

        return _shorelineWavesData;
    }


    public async Task BakeWavesToTexture(int areaSize, Vector3 shorelineAreaPos, float curvedSurfacesQualityScale, Vector3 waterPos, string GUID)
    {
        var shorelineWavesData = await GetShorelineWavesData(GUID);
        if (shorelineWavesData == null || shorelineWavesData.Count == 0) return;

        if (!isInitializedEditorResources) InitializeEditorResources();

        foreach (var wavesObject in wavesObjects) wavesObject.SetActive(false);

        int texSize = areaSize * 2;
        texSize = Math.Min(texSize, 4096);

        for (var i = 0; i < shorelineWavesData.Count; i++)
        {
            if (i == wavesObjects.Count) wavesObjects.Add(CreateTempGO());
            wavesObjects[i].SetActive(true);

            var meterInPixels = (1f * areaSize / texSize);

            var roundedPos = new Vector2(Mathf.Round(shorelineWavesData[i].PositionX / meterInPixels) * meterInPixels, Mathf.Round(shorelineWavesData[i].PositionZ / meterInPixels) * meterInPixels);
            var roundedScale = new Vector2(Mathf.Round(shorelineWavesData[i].ScaleX / meterInPixels) * meterInPixels, Mathf.Round(shorelineWavesData[i].ScaleZ / meterInPixels) * meterInPixels);
            var height = shorelineWavesData[i].ID % 2 == 0 ? HeightWave1 : HeightWave2;

            var tempT = wavesObjects[i].transform;
            tempT.position = new Vector3(roundedPos.x, height, roundedPos.y);
            tempT.rotation = Quaternion.Euler(270, 0, shorelineWavesData[i].EulerRotation);
            tempT.localScale = roundedScale;

            var scaleTexels = new Vector2(meterInPixels * tempT.localScale.x, meterInPixels * tempT.localScale.y);
            var uvOffset = new Vector2(1 - (scaleTexels.x - meterInPixels) / scaleTexels.x, 1 - (scaleTexels.y - meterInPixels) / scaleTexels.y);

            var pixelsInQuad = new Vector2(meterInPixels * tempT.localScale.x, meterInPixels * tempT.localScale.y);
            var uvScale = new Vector2(1f - (pixelsInQuad.x - 1) / pixelsInQuad.x, 1f - (pixelsInQuad.y - 1) / pixelsInQuad.y);

            var props = new MaterialPropertyBlock();
            var scaleMultiplier = shorelineWavesData[i].ScaleY / shorelineWavesData[i].DefaultScaleY;
            if (scaleMultiplier < 1.0f) scaleMultiplier *= Mathf.Lerp(0.25f, 1f, scaleMultiplier);

            var rend = wavesObjects[i].GetComponent<MeshRenderer>();
            rend.GetPropertyBlock(props);
            props.SetVector("KW_WavesUVOffset", new Vector4(uvOffset.x, uvOffset.y, uvScale.x, uvScale.y));
            props.SetFloat("KW_WaveScale", scaleMultiplier);
            props.SetFloat("KW_WaveTimeOffset", shorelineWavesData[i].TimeOffset);
            props.SetFloat("KW_WaveAngle", ((tempT.rotation.eulerAngles.y) % 360f) / 360f);
            rend.SetPropertyBlock(props);
        }

        
        InitializeEditorTextures(texSize);

        shorelineAreaPos.y = HeightWave1; 
        cam.transform.position = shorelineAreaPos;
        cam.orthographicSize = areaSize * 0.5f;
        cam.nearClipPlane = -2;
        cam.farClipPlane = 2;

        Shader.SetGlobalFloat("_ShorelineBake_mrtBufferIdx", 0);
        cam.targetTexture = waves1_rt;
        cam.Render();

        Shader.SetGlobalFloat("_ShorelineBake_mrtBufferIdx", 1);
        cam.targetTexture = wavesData1_rt;
        cam.Render();
        //cam.SetTargetBuffers(new [] { waves1_rt.colorBuffer, wavesData1_rt.colorBuffer}, waves1_rt.depthBuffer); //SetTargetBuffers doesn't work on URP and camera.Render or UniversalRenderPipeline.RenderSingleCamera


        //KW_Extensions.CameraRender(cam);

        shorelineAreaPos.y = HeightWave2;
        cam.transform.position = shorelineAreaPos;
        //cam.SetTargetBuffers(new[] { waves2_rt.colorBuffer, wavesData2_rt.colorBuffer }, waves2_rt.depthBuffer);
        //cam.targetTexture = waves2_rt;
        Shader.SetGlobalFloat("_ShorelineBake_mrtBufferIdx", 0);
        cam.targetTexture = waves2_rt;
        cam.Render();

        Shader.SetGlobalFloat("_ShorelineBake_mrtBufferIdx", 1);
        cam.targetTexture = wavesData2_rt;
        cam.Render();
        //KW_Extensions.CameraRender(cam);

        //cam.transform.position = waterPos;
        //cam.orthographic = true;
        //cam.nearClipPlane = nearPlaneDepth;
        //cam.farClipPlane = farPlaneDepth;
        //cam.transform.position = waterPos;
        //cam.cullingMask = ~waterLayer;
        //cam.targetTexture = depth_rt;
        //cam.Render();
        var depthSize = Mathf.Min(4096, (int)(texSize * curvedSurfacesQualityScale));
       
        shorelineAreaPos.y = transform.position.y;
        depth_rt = RenderDepth(depthCam, depth_rt, shorelineAreaPos, areaSize, depthSize);
        if(depthParams == null) depthParams = new OrthoDepthParams();
        depthParams.SetData(areaSize, waterPos);
        UpdateShaderParameters(areaSize, shorelineAreaPos);
    }

    public void SavesWavesParamsToDataFolder(string GUID)
    {
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

        KW_Extensions.SerializeToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shoreLineWavesData), _shorelineWavesData);
    }

    public void SaveWavesToDataFolder(string GUID)
    {
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

        KW_Extensions.SerializeToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shoreLineWavesData), _shorelineWavesData);

        waves1_rt.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV1), TextureFormat.RGBAHalf);
        waves2_rt.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV2), TextureFormat.RGBAHalf);
        wavesData1_rt.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData1), TextureFormat.RGHalf);
        wavesData2_rt.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData2), TextureFormat.RGHalf);
    }

    public void SaveOrthoDepth(string GUID)
    {
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

        SaveDepthTextureToFile(depth_rt, Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineDepth));
        SaveDepthDataToFile(depthParams, Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineDepthData));
    }

    public async Task<bool> RenderShorelineWavesWithFoam(int shorelineAreaSize, Vector3 shorelineAreaPos, string GUID)
    {
        var shorelineWavesData = await GetShorelineWavesData(GUID);
        if (shorelineWavesData == null || shorelineWavesData.Count == 0) return false;

        if (!isInitializedShorelineResources) await InitializeShorelineResources(GUID);
        UpdateShaderParameters(shorelineAreaSize, shorelineAreaPos);
        RenderFoam(shorelineWavesData);
        return true;
    }

    void RenderFoam(List<ShorelineWaveInfo> shorelineWavesData)
    {
        //Debug.Log("RenderFoam");

        if (VAT_Mesh_Lod0 == null || vatMaterial == null) return;

        var lodArray = new[] { VAT_Mesh_Lod0, VAT_Mesh_Lod1, VAT_Mesh_Lod2, VAT_Mesh_Lod3, VAT_Mesh_Lod4, VAT_Mesh_Lod5, VAT_Mesh_Lod6, VAT_Mesh_Lod7 };
        var props = new MaterialPropertyBlock();
        var activeID = new List<int>();
        foreach (var wave in shorelineWavesData)
        {
            if (!foamGameObjects.ContainsKey(wave.ID))
            {
                var vatGO = new GameObject("FoamParticles");
                vatGO.transform.parent = transform;
                var customLod = new CustomLod();
                customLod.Parent = vatGO;
                customLod.LodObjects = new GameObject[lodArray.Length];
                customLod.ShadowObjects = new GameObject[lodArray.Length];

                for (int i = 0; i < lodArray.Length; i++)
                {
                    var lodGO = CreateLodWave("FoamParticles_Lod" + i, vatGO.transform, lodArray[i], props, wave.TimeOffset, LodFoamSize[i], false);
                    var shadowMesh = lodArray.Length > i + 3 ? lodArray[i + 3] : lodArray.Last();
                    var shadowLodGO = CreateLodWave("FoamParticlesShadow_Lod" + i, lodGO.transform, shadowMesh, props, wave.TimeOffset, LodFoamSize[i] + 0.15f, true);

                    customLod.LodObjects[i] = lodGO;
                    customLod.ShadowObjects[i] = shadowLodGO;
                }

                foamGameObjects.Add(wave.ID, customLod);
            }

            activeID.Add(wave.ID);
            var vatParticlesGO = foamGameObjects[wave.ID];
            var lessScaleFix = Mathf.Lerp(0.15f, 0, Mathf.Clamp01((wave.ScaleY / wave.DefaultScaleY)));
            vatParticlesGO.Parent.transform.position = new Vector3(wave.PositionX, transform.position.y + lessScaleFix, wave.PositionZ);
            vatParticlesGO.Parent.transform.rotation = Quaternion.Euler(0, wave.EulerRotation, 0);
            vatParticlesGO.Parent.transform.localScale = new Vector3(wave.ScaleX / wave.DefaultScaleX, wave.ScaleY / wave.DefaultScaleY, wave.ScaleZ / wave.DefaultScaleZ);
            vatParticlesGO.Position = vatParticlesGO.Parent.transform.position;
        }

        RemoveNotActualFoam(activeID);
    }

    private void RemoveNotActualFoam(List<int> activeID)
    {
        foreach (var foamGO in foamGameObjects.ToList())
        {
            if (!activeID.Contains(foamGO.Key))
            {
                KW_Extensions.SafeDestroy(foamGameObjects[foamGO.Key].Parent);
                foamGameObjects.Remove(foamGO.Key);
            }
        }
    }


    GameObject CreateLodWave(string lodName, Transform parent, Mesh mesh, MaterialPropertyBlock props, float timeOffset, float lodParticleSize, bool useShadows)
    {
        var lod = new GameObject(lodName);
        lod.SetActive(false);
        lod.transform.parent = parent;
        lod.layer = 4 << 0;
        lod.AddComponent<MeshFilter>().sharedMesh = mesh;

        var vatRend = lod.AddComponent<MeshRenderer>();
        vatRend.sharedMaterial = vatMaterial;
        vatRend.shadowCastingMode = useShadows ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.Off;
        vatRend.GetPropertyBlock(props);
        props.SetFloat("KW_WaveTimeOffset", timeOffset);
        props.SetFloat("KW_SizeAdditiveScale", lodParticleSize);
        vatRend.SetPropertyBlock(props);
        return lod;
    }

    public void ClearFoam()
    {
        if (wavesObjects != null)
        {
            foreach (var waveObj in wavesObjects) KW_Extensions.SafeDestroy(waveObj);
            wavesObjects.Clear();
        }

        if (foamGameObjects != null)
        {
            foreach (var vatParticles in foamGameObjects)
            {
                KW_Extensions.SafeDestroy(vatParticles.Value.Parent);
            }
            foamGameObjects.Clear();
        }
        foamLodsForLateDeactivation.Clear();
    }

    public void ClearShorelineWavesWithFoam(string GUID)
    {
        ClearFoam();
        if (_shorelineWavesData != null) _shorelineWavesData.Clear();

        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        var directory = Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID);
        if (Directory.Exists(directory)) Directory.Delete(directory, true);

        ReleaseEditorResources();
        isInitializedEditorResources = false;
    }

    public void Release()
    {
        OnDisable();
    }

    private float UpdateEachSeconds = 0.5f;
    private float lastLodUpdateLeftTime = 0;
    private bool updateLodTest = false;

    bool lastFoamShadowsStatus;

    public void UpdateLodLevels(Vector3 camPos, int qualityLevel, bool useFoamShadows)
    {
        if (!isInitializedShorelineResources || foamGameObjects.Count == 0) return;

        foreach (var foamRend in foamLodsForLateDeactivation)
        {
            if (foamRend != null) foamRend.SetActive(false);
        }

        if (lastLodUpdateLeftTime < 0) lastLodUpdateLeftTime = 0;
        lastLodUpdateLeftTime += KW_Extensions.DeltaTime();
        if (lastLodUpdateLeftTime < UpdateEachSeconds) return;
        lastLodUpdateLeftTime = 0;
        foamLodsForLateDeactivation.Clear();

        foreach (var vatLodGO in foamGameObjects)
        {
            if (vatLodGO.Value != null)
            {
                var customLod = vatLodGO.Value;
                var lods = customLod.LodObjects;
                var distance = Vector3.Distance(camPos, customLod.Position);
                int n = 0;
                for (var i = 0; i < lods.Length; i++)
                {
                    float lodDist = 10f;
                    if (qualityLevel == 0) lodDist = LodDistances_High[i];
                    else if (qualityLevel == 1) lodDist = LodDistances_Medium[i];
                    else if (qualityLevel == 2) lodDist = LodDistances_Low[i];

                    if (distance > lodDist) n = i;
                }
                if (n != customLod.ActiveLod)
                {
                    if(customLod.ActiveLod != -1) foamLodsForLateDeactivation.Add(lods[customLod.ActiveLod]);
                    customLod.ActiveLod = n;
                    lods[n].SetActive(true);
                }
            }
        }

        if (lastFoamShadowsStatus != useFoamShadows)
        {
            
            foreach (var vatLodGO in foamGameObjects)
            {
                if (vatLodGO.Value != null)
                {
                    var shadowsObjects = vatLodGO.Value.ShadowObjects;
                    foreach (var shadowGO in shadowsObjects)
                    {
                        shadowGO.SetActive(useFoamShadows);
                    }
                }
            }
            lastFoamShadowsStatus = useFoamShadows;
        }
    }

    void OnDisable ()
    {
       // print("ShorelineMap.Disabled");


        KW_Extensions.SafeDestroy(camGO);
		KW_Extensions.SafeDestroy(waveMaterial);
        KW_Extensions.SafeDestroy(quad);

        ClearFoam();

        if (_shorelineWavesData != null) _shorelineWavesData.Clear();

        ReleaseEditorResources();
        KW_Extensions.SafeDestroy(vatMaterial, shorelineWaveDisplacementTex, shorelineWaveNormalTex, 
            VAT_Mesh_Lod0, VAT_Mesh_Lod1, VAT_Mesh_Lod2, VAT_Mesh_Lod3, VAT_Mesh_Lod4, VAT_Mesh_Lod5, VAT_Mesh_Lod6, VAT_Mesh_Lod7, 
            VAT_Position, VAT_Alpha, VAT_RangeLookup, VAT_Offset);

        isInitializedShorelineResources = false;
        isInitializedEditorResources = false;

        lastLodUpdateLeftTime = 0;
        lastFoamShadowsStatus = false;
    }

    public void ReleaseEditorResources()
    {
        KW_Extensions.SafeDestroy(waves_tex1, waves_tex2, wavesData1_tex, wavesData2_tex, depth_tex, camGO, depthCamGO);
        KW_Extensions.ReleaseRenderTextures(waves1_rt, waves2_rt, wavesData1_rt, wavesData2_rt, depth_rt);

    }

    GameObject CreateTempGO()
    {
        var go = Instantiate(quad);
        go.transform.parent = transform;
        var rend = go.GetComponent<MeshRenderer>();
        rend.sharedMaterial = waveMaterial;
        go.SetActive(false);
        return go;
    }

    public async Task InitialiseShorelineEditorResources(string GUID)
    {

        await InitializeShorelineResources(GUID);
    }

    public async Task InitializeShorelineResources(string GUID)
    {
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

        if (shorelineWaveDisplacementTex == null) shorelineWaveDisplacementTex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_ToShorelineWaveTex));
        if (shorelineWaveNormalTex == null) shorelineWaveNormalTex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_ToShorelineWaveNormTex), true);

        if (VAT_Position == null) VAT_Position = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_PositionTex), true, FilterMode.Point, TextureWrapMode.Repeat);
        if (VAT_Alpha == null) VAT_Alpha = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_AlphaTex), true, FilterMode.Point, TextureWrapMode.Repeat);
        if (VAT_RangeLookup == null) VAT_RangeLookup = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_RangeLookupTex), true, FilterMode.Point, TextureWrapMode.Repeat);
        if (VAT_Offset == null) VAT_Offset = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_OffsetTex));
        if (VAT_Mesh_Lod0 == null)
        {
            VAT_Mesh_Lod0 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod0), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod1 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod1), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod2 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod2), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod3 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod3), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod4 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod4), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod5 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod5), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod6 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod6), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod7 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod7), new Vector3(14, 4.5f, 16) * 0.75f);
        }

        if (vatMaterial != null)
        {
            vatMaterial.SetTexture(ID_KW_ShorelineVAT_Position, VAT_Position);
            vatMaterial.SetTexture(ID_KW_ShorelineVAT_Alpha, VAT_Alpha);
            vatMaterial.SetTexture(ID_KW_ShorelineVAT_RangeLookup, VAT_RangeLookup);
            vatMaterial.SetTexture(ID_KW_ShorelineVAT_Offset, VAT_Offset);
        }

        if (vatMaterial != null && VAT_Mesh_Lod0 && VAT_Position != null) isInitializedShorelineResources = true;

        var pathToShorelineDirectory = Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID);
        if (File.Exists(Path.Combine(pathToShorelineDirectory, path_shorelineMapUV1 + ".gz")))
        {
            if (waves_tex1 == null) waves_tex1 = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToShorelineDirectory, path_shorelineMapUV1), true, FilterMode.Bilinear, TextureWrapMode.Clamp);
            if (waves_tex2 == null) waves_tex2 = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToShorelineDirectory, path_shorelineMapUV2), true, FilterMode.Bilinear, TextureWrapMode.Clamp);
            if (wavesData1_tex == null) wavesData1_tex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToShorelineDirectory, path_shorelineMapData1), true, FilterMode.Point, TextureWrapMode.Clamp);
            if (wavesData2_tex == null) wavesData2_tex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToShorelineDirectory, path_shorelineMapData2), true, FilterMode.Point, TextureWrapMode.Clamp);

            depthParams = await KW_Extensions.DeserializeFromFile<OrthoDepthParams>(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineDepthData));
            if (depth_tex == null) depth_tex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineDepth));
        }

      
        // Debug.Log("Initialized Shoreline Resources " + VAT_Position.width + "  " + VAT_Mesh_Lod0.triangles.Length);
    }

    void InitializeEditorTextures(int size)
    {
        waves1_rt = KW_Extensions.ReinitializeRenderTexture(waves1_rt, size, size, 0, RenderTextureFormat.ARGB32, null, false, false, TextureWrapMode.Clamp);
        waves2_rt = KW_Extensions.ReinitializeRenderTexture(waves2_rt, size, size, 0, RenderTextureFormat.ARGB32, null, false, false, TextureWrapMode.Clamp);
        wavesData1_rt = KW_Extensions.ReinitializeRenderTexture(wavesData1_rt, size, size, 0, RenderTextureFormat.ARGBHalf, null, false, false, TextureWrapMode.Clamp);
        wavesData2_rt = KW_Extensions.ReinitializeRenderTexture(wavesData2_rt, size, size, 0, RenderTextureFormat.ARGBHalf, null, false, false, TextureWrapMode.Clamp);
    }

    void InitializeEditorResources()
    {
        waveMaterial = KW_Extensions.CreateMaterial(shorelineWaveBakinng_shaderName);
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = transform;
        KW_Extensions.SafeDestroy(quad.GetComponent<Collider>());

        camGO = new GameObject("ShorelineCamera");
        camGO.transform.parent = transform;
        cam = camGO.AddComponent<Camera>();

        cam.transform.rotation = Quaternion.Euler(90, 0, 0);

        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.clearFlags = CameraClearFlags.Color;
        cam.allowHDR = false;
        cam.allowMSAA = false;
        cam.nearClipPlane = -2;
        cam.farClipPlane = 2;
        cam.depth = -1000;
        cam.orthographic = true;
        cam.enabled = false;

        depthCam = InitializeDepthCamera(nearPlaneDepth, farPlaneDepth, transform);
        depthCamGO = depthCam.gameObject;

        isInitializedEditorResources = true;
    }

    void UpdateShaderParameters(int shorelineAreaSize, Vector3 shorelinePos)
    {
        Shader.SetGlobalTexture("KW_BakedWaves1_UV_Angle_Alpha", waves1_rt != null ? waves1_rt as Texture : waves_tex1);
        Shader.SetGlobalTexture("KW_BakedWaves2_UV_Angle_Alpha", waves2_rt != null ? waves2_rt as Texture : waves_tex2);
        Shader.SetGlobalTexture("KW_BakedWaves1_TimeOffset_Scale", wavesData1_rt != null ? wavesData1_rt as Texture : wavesData1_tex);
        Shader.SetGlobalTexture("KW_BakedWaves2_TimeOffset_Scale", wavesData2_rt != null ? wavesData2_rt as Texture : wavesData2_tex);
        Shader.SetGlobalFloat("KW_GlobalTimeOffsetMultiplier", GlobalTimeOffsetMultiplier);
        Shader.SetGlobalFloat("KW_GlobalTimeSpeedMultiplier", GlobalTimeSpeedMultiplier);

        if (depthParams != null)
        {
            Shader.SetGlobalTexture(ID_KW_ShorelineDepth, depth_rt != null ? depth_rt as Texture : depth_tex);
            Shader.SetGlobalFloat(ID_KW_ShorelineDepthOrthoSize, depthParams.OtrhograpicSize);
            Shader.SetGlobalVector(ID_KW_ShorelineDepthNearFarDistance, new Vector3(nearPlaneDepth, farPlaneDepth, farPlaneDepth - nearPlaneDepth));
        }

        Shader.SetGlobalTexture(ID_KW_ShorelineWaveDisplacement, shorelineWaveDisplacementTex);
        Shader.SetGlobalTexture(ID_KW_ShorelineWaveNormal, shorelineWaveNormalTex);
        Shader.SetGlobalFloat(ID_KW_ShorelineMapSize, shorelineAreaSize);
        Shader.SetGlobalVector(ID_KW_ShorelineAreaPos, shorelinePos);
    }

}
