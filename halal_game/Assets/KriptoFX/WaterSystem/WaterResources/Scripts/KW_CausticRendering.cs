using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//[ExecuteInEditMode]
public class KW_CausticRendering : MonoBehaviour
{
    public Texture2D depth_tex;
    public RenderTexture causticLod0;
    public RenderTexture causticLod1;
    public RenderTexture causticLod2;
    public RenderTexture causticLod3;
    public RenderTexture causticRT;

    private GameObject cameraGameObject;
    private GameObject causticBakeGameObject;
    private GameObject causticDecalGameObject;
    private Material causticBakeMaterial;
    private Material causticDecalMaterial;
    private Mesh causticMesh;
    private Mesh decalMesh;
    private ComputeShader causticComputeShader;

    private int causticKernel;
    private CommandBuffer cb;
    Camera causticCam;

    private const string path_causticFolder = "CausticMaps";
    private const string path_causticDepthTexture = "KW_CausticDepthTexture";
    private const string path_causticDepthData = "KW_CausticDepthData";
    private const string CausticBakeShaderName = "Hidden/KriptoFX/Water/ComputeCaustic";
    private const string CausticDecalShaderName = "Hidden/KriptoFX/Water/CausticDecal";

    private int ID_KW_CausticDepth = Shader.PropertyToID("KW_CausticDepthTex");
    private int ID_KW_CausticDepthOrthoSize = Shader.PropertyToID("KW_CausticDepthOrthoSize");
    private int ID_KW_CausticDepthNearFarDistance = Shader.PropertyToID("KW_CausticDepth_Near_Far_Dist");
    private int ID_KW_CausticDepthPos = Shader.PropertyToID("KW_CausticDepthPos");

    private const int CameraHeight = 7855;
    private const int nearPlaneDepth = -2;
    private const int farPlaneDepth = 100;

    private int currentMeshResolution;
    private bool isDepthTextureInitialized;
    public static Vector4 LodSettings = new Vector4(10, 20, 40, 80);
    
    public void Release()
    {
        OnDisable();
    }

    void OnDisable()
    {
        KW_Extensions.SafeDestroy(depth_tex, cameraGameObject, causticBakeGameObject, causticDecalGameObject, causticBakeMaterial, causticDecalMaterial, causticMesh, decalMesh, causticComputeShader);
        KW_Extensions.ReleaseRenderTextures(causticLod0, causticLod1, causticLod2, causticLod3, causticRT);

        currentMeshResolution = 0;
        isDepthTextureInitialized = false;
        Shader.DisableKeyword("USE_DEPTH_SCALE");
    }

    private void InitializeCausticTexture(int size)
    {
        causticLod0 = KW_Extensions.ReinitializeRenderTexture(causticLod0, size, size, 0, RenderTextureFormat.R8, null, false, true);
        causticLod1 = KW_Extensions.ReinitializeRenderTexture(causticLod1, size, size, 0, RenderTextureFormat.R8, null, false, true);
        causticLod2 = KW_Extensions.ReinitializeRenderTexture(causticLod2, size, size, 0, RenderTextureFormat.R8, null, false, true);
        causticLod3 = KW_Extensions.ReinitializeRenderTexture(causticLod3, size, size, 0, RenderTextureFormat.R8, null, false, true);
    }

    private async void LoadDepthTexture(string GUID)

    {
        isDepthTextureInitialized = true;
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        var pathToDepthTex = Path.Combine(pathToBakedDataFolder, path_causticFolder, GUID, path_causticDepthTexture);
        var pathToDepthData = Path.Combine(pathToBakedDataFolder, path_causticFolder, GUID, path_causticDepthData);
        var depthParams = await KW_Extensions.DeserializeFromFile<KW_WaterOrthoDepth.OrthoDepthParams>(pathToDepthData);
        if (depthParams != null)
        {
            if (depth_tex == null) depth_tex = await KW_Extensions.ReadTextureFromFileAsync(pathToDepthTex);
            Shader.SetGlobalTexture(ID_KW_CausticDepth, depth_tex);
            Shader.SetGlobalFloat(ID_KW_CausticDepthOrthoSize, depthParams.OtrhograpicSize);
            Shader.SetGlobalVector(ID_KW_CausticDepthNearFarDistance, new Vector3(nearPlaneDepth, farPlaneDepth, farPlaneDepth - nearPlaneDepth));
            Shader.SetGlobalVector(ID_KW_CausticDepthPos, new Vector3(depthParams.PositionX, depthParams.PositionY, depthParams.PositionZ));
          
            Shader.EnableKeyword("USE_DEPTH_SCALE");
        }
    }

    void InitializeCamera()
    {
        cameraGameObject = new GameObject("WaterCausticCamera");
        cameraGameObject.transform.parent = transform;
        cameraGameObject.transform.localPosition += Vector3.up * CameraHeight;
        cameraGameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        cameraGameObject.hideFlags = HideFlags.HideAndDontSave;

        causticCam = cameraGameObject.AddComponent<Camera>();
        //causticCam.cameraType = CameraType.Reflection;
        causticCam.enabled = false;
        causticCam.allowMSAA = false;

        var cameraData = cameraGameObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;

        causticCam.orthographic = true;
        causticCam.allowMSAA = false;
        causticCam.allowHDR = false;
        causticCam.clearFlags = CameraClearFlags.Color;
        causticCam.backgroundColor = Color.black;
        causticCam.nearClipPlane = -1;
        causticCam.farClipPlane = 1;
        
    }

    void InitializeCausticBakeGO()
    {
        causticBakeGameObject = new GameObject("BakeCausticMesh");
        causticBakeGameObject.transform.parent = transform;
        causticBakeGameObject.transform.localPosition += Vector3.up * CameraHeight;


        causticBakeGameObject.AddComponent<MeshFilter>().sharedMesh = causticMesh;
        causticBakeGameObject.AddComponent<MeshRenderer>().sharedMaterial = causticBakeMaterial;

    }

    //void InitializeCausticDecalGO()
    //{
    //    causticDecalGameObject = new GameObject("DecalCausticMesh");
    //    causticDecalGameObject.transform.parent = transform;
    //    causticDecalGameObject.transform.localPosition += Vector3.up * CameraHeight;

    //    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    causticDecalGameObject.AddComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().mesh;
    //    causticDecalGameObject.AddComponent<MeshRenderer>().sharedMaterial = causticDecalMaterial;
    //    KW_Extensions.SafeDestroy(cube);
    //}

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (causticBakeMaterial == null) causticBakeMaterial = KW_Extensions.CreateMaterial(CausticBakeShaderName);
        if (!waterShaderMaterials.Contains(causticBakeMaterial)) waterShaderMaterials.Add(causticBakeMaterial);

        if (causticDecalMaterial == null) causticDecalMaterial = KW_Extensions.CreateMaterial(CausticDecalShaderName);
        if (!waterShaderMaterials.Contains(causticDecalMaterial)) waterShaderMaterials.Add(causticDecalMaterial);
    }

    public void AddComputeShadersToWaterRendering(Dictionary<ComputeShader, List<int>> waterSharedComputeShaders)
    {
        if (causticComputeShader == null)
        {
            causticComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("KW_Caustic_GPU"));
            causticKernel = causticComputeShader.FindKernel("CausticUpdate");
        }
        if (!waterSharedComputeShaders.ContainsKey(causticComputeShader)) waterSharedComputeShaders.Add(causticComputeShader, new List<int>(){ causticKernel });
    }

    //public RenderTexture RenderCausticDX11(int size)
    //{
    //    causticRT = KW_Extensions.ReinitializeRenderTexture(causticRT, size, size, 0, RenderTextureFormat.ARGBFloat, Color.black, true);
    //    Debug.Log("Dispatch");
    //    causticComputeShader.SetTexture(causticKernel, "causticRT", causticRT);
    //    causticComputeShader.Dispatch(causticKernel, 64, 64, 1);
    //    return causticRT;
    //}

   
    //public void RenderScreenSpaceCaustic(Camera currentCamera, Transform anchor, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers, int causticTextureSize, int meshResolution, bool useFiltering, bool useDisperstion, float dispersionStrength)
    //{
    //    if (cb != null && waterSharedBuffers.ContainsKey(cb)) return;
    //    print("ScreenSpacePlanarReflection.CreatedCommand");

    //    if (cb == null) cb = new CommandBuffer() { name = "ScreenSpaceCaustic" };
    //    else cb.Clear();

    //    InitializeCausticTexture(causticTextureSize);
    //    GeneratePlane(meshResolution, 1.0f, false);

    //    if (useFiltering) causticBakeMaterial.EnableKeyword("USE_FILTERING");
    //    else causticBakeMaterial.DisableKeyword("USE_FILTERING");

    //    if (useDisperstion && dispersionStrength > 0.5f)
    //    {
    //        causticDecalMaterial.EnableKeyword("USE_DISPERSION");
    //        dispersionStrength = Mathf.Lerp(dispersionStrength * 0.25f, dispersionStrength, causticTextureSize / 1024f);
    //        causticDecalMaterial.SetFloat("KW_CausticDispersionStrength", dispersionStrength);
    //    }
    //    else
    //    {
    //        causticDecalMaterial.DisableKeyword("USE_DISPERSION");
    //    }


    //    float h = Mathf.Tan(currentCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * 1 * 2f;
    //    var fullScreenSizeMatrix = Matrix4x4.TRS(currentCamera.transform.position + currentCamera.transform.forward, currentCamera.transform.rotation, new Vector3(h * currentCamera.aspect, h, 1));

    //    //cb.SetRenderTarget(screenSpaceCaustic);
    //    //cb.ClearRenderTarget(false, true, Color.black);
    //    cb.DrawMesh(causticMesh, fullScreenSizeMatrix, causticBakeMaterial);
    //    //causticDecalMaterial.SetTexture("KW_ScreenSpaceCaustic", screenSpaceCaustic);

    //    waterSharedBuffers.Add(cb, CameraEvent.BeforeForwardAlpha);

    //    //if (causticDecalGameObject == null) InitializeCausticDecalGO();
    //    //var decalPos = currentCamera.transform.position;
    //    //decalPos.y = transform.position.y - 15;
    //    //causticDecalGameObject.transform.position = decalPos;
    //    //causticDecalGameObject.transform.localScale = new Vector3(100, 40, 100);
    //}

    void RenderLod(ScriptableRenderContext context, Vector3 camPos, Vector3 camDir, float lodDistance, RenderTexture target, float causticStr, float causticDepthScale, bool useFiltering = false)
    {
        var bakeCamPos = camPos + camDir * lodDistance * 0.5f;
        bakeCamPos.y = CameraHeight;
        causticCam.transform.position = bakeCamPos;
        causticCam.orthographicSize = lodDistance * 0.5f;
        causticBakeGameObject.transform.position = bakeCamPos;
        causticBakeGameObject.transform.localScale = Vector3.one * lodDistance;

        if (useFiltering) causticBakeMaterial.EnableKeyword("USE_CAUSTIC_FILTERING");
        else causticBakeMaterial.DisableKeyword("USE_CAUSTIC_FILTERING");
        

        causticBakeMaterial.SetFloat("KW_MeshScale", lodDistance);
        causticBakeMaterial.SetFloat("KW_CaustisStrength", causticStr);
        causticBakeMaterial.SetFloat("KW_CausticDepthScale", causticDepthScale);

        causticCam.targetTexture = target;
        KW_Extensions.CameraRender(causticCam);
    }

    void GenerateDecalMesh()
    {
        Vector3[] vertices = {
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, -0.5f, 0.5f),
            new Vector3 (-0.5f, -0.5f, 0.5f),
        };

        int[] triangles = {
            0, 2, 1, //face front
            0, 3, 2,
            2, 3, 4, //face top
            2, 4, 5,
            1, 2, 5, //face right
            1, 5, 6,
            0, 7, 4, //face left
            0, 4, 3,
            5, 4, 7, //face back
            5, 7, 6,
            0, 6, 7, //face bottom
            0, 1, 6
        };

        if (decalMesh == null)
        {
            decalMesh = new Mesh();
        }
        decalMesh.Clear();
        decalMesh.vertices = vertices;
        decalMesh.triangles = triangles;
        decalMesh.RecalculateNormals();
    }

    public void Render(ScriptableRenderContext context, Camera currentCamera, float causticStr, float causticDepthScale, int causticTextureSize, int activeLodCounts, int meshResolution, bool useFiltering,
        bool useDisperstion, bool useDepthScale, float dispersionStrength, List<Material> waterSharedMaterials, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers, string GUID)
    {

        
        if (causticLod0 == null || causticTextureSize != causticLod0.width) InitializeCausticTexture(causticTextureSize);
        if (cameraGameObject == null) InitializeCamera();
        if (currentMeshResolution != meshResolution) GeneratePlane(meshResolution, 1.1f);
        if (causticBakeGameObject == null) InitializeCausticBakeGO();
        if (decalMesh == null) GenerateDecalMesh();
        if (useDepthScale && !isDepthTextureInitialized) LoadDepthTexture(GUID);
        

        var camPos = currentCamera.transform.position;
        var camDir = currentCamera.transform.forward;
        var decalScale = LodSettings[activeLodCounts - 1] * 2;

        RenderLod(context, camPos, camDir, LodSettings.x, causticLod0, causticStr, causticDepthScale, useFiltering);
        if (activeLodCounts > 1) RenderLod(context, camPos, camDir, LodSettings.y, causticLod1, causticStr, causticDepthScale, useFiltering);
        if (activeLodCounts > 2) RenderLod(context, camPos, camDir, LodSettings.z, causticLod2, causticStr, causticDepthScale);
        if (activeLodCounts > 3) RenderLod(context, camPos, camDir, LodSettings.w, causticLod3, causticStr, causticDepthScale);

        //if (causticDecalGameObject == null) InitializeCausticDecalGO();
        var decalPos = currentCamera.transform.position;
        decalPos.y = transform.position.y - 15;
        //causticDecalGameObject.transform.position = decalPos;
       // causticDecalGameObject.transform.localScale = new Vector3(decalScale, 40, decalScale);

      //  var lodParams = new Vector4(decalScale / lodSettings.x, decalScale / lodSettings.y, decalScale / lodSettings.z, decalScale / lodSettings.w);
        var lodDir = camDir * 0.5f;
        UpdateMaterialParams(causticDecalMaterial, lodDir, camPos, decalScale);

        foreach (var waterSharedMaterial in waterSharedMaterials)
        {
            UpdateMaterialParams(waterSharedMaterial, lodDir, camPos, decalScale);
        }

        causticDecalMaterial.SetFloat("KW_CaustisStrength", causticStr);
        if (useDisperstion && dispersionStrength > 0.1f)
        {
            causticDecalMaterial.EnableKeyword("USE_DISPERSION");
            dispersionStrength = Mathf.Lerp(dispersionStrength * 0.25f, dispersionStrength, causticTextureSize / 1024f);
            causticDecalMaterial.SetFloat("KW_CausticDispersionStrength", dispersionStrength);
        }
        else causticDecalMaterial.DisableKeyword("USE_DISPERSION");

        if (!useDepthScale)
        {
            isDepthTextureInitialized = false;
            Shader.DisableKeyword("USE_DEPTH_SCALE");
        }

        Shader.DisableKeyword("USE_LOD1");
        Shader.DisableKeyword("USE_LOD2");
        Shader.DisableKeyword("USE_LOD3");
        switch (activeLodCounts)
        {
            case 2:
                Shader.EnableKeyword("USE_LOD1");
                break;
            case 3:
                Shader.EnableKeyword("USE_LOD2");
                break;
            case 4:
                Shader.EnableKeyword("USE_LOD3");
                break;
        }
        

#if !UNITY_PIPELINE_URP && !UNITY_PIPELINE_HDRP
        RenderDecal(decalPos, decalScale, waterSharedBuffers);
#endif
    }

    void RenderDecal(Vector3 decalPos, float decalScale, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cb == null) cb = new CommandBuffer() { name = "CausticDecal" };
        else cb.Clear();

        var decalTRS = Matrix4x4.TRS(decalPos, Quaternion.identity, new Vector3(decalScale, 40, decalScale));
        cb.DrawMesh(decalMesh, decalTRS, causticDecalMaterial);

        if (!waterSharedBuffers.ContainsKey(cb)) waterSharedBuffers.Add(cb, CameraEvent.BeforeForwardAlpha);
    }

    public void SaveOrthoDepth(string GUID, Vector3 position, int areaSize, int texSize)
    {
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        var pathToDepthTex = Path.Combine(pathToBakedDataFolder, path_causticFolder, GUID, path_causticDepthTexture);
        var pathToDepthData = Path.Combine(pathToBakedDataFolder, path_causticFolder, GUID, path_causticDepthData);
        KW_WaterOrthoDepth.RenderAndSaveDepth(transform, position, areaSize, texSize, nearPlaneDepth, farPlaneDepth, pathToDepthTex, pathToDepthData);
        Release();
    }

    void UpdateMaterialParams(Material mat, Vector3 lodDir, Vector3 lodPos, float decalScale)
    {
        if (mat == null) return;
        mat.SetTexture("KW_CausticLod0", causticLod0);
        mat.SetTexture("KW_CausticLod1", causticLod1);
        mat.SetTexture("KW_CausticLod2", causticLod2);
        mat.SetTexture("KW_CausticLod3", causticLod3);
        mat.SetVector("KW_CausticLodSettings", LodSettings);
        mat.SetVector("KW_CausticLodOffset", lodDir);
        mat.SetVector("KW_CausticLodPosition", lodPos);
        mat.SetFloat("KW_DecalScale", decalScale);
    }

    private void GeneratePlane(int meshResolution, float scale, bool useXZplane = true)
    {
        currentMeshResolution = meshResolution;
        if (causticMesh == null)
        {
            causticMesh = new Mesh();
            causticMesh.indexFormat = IndexFormat.UInt32;
        }

        var vertices = new Vector3[(meshResolution + 1) * (meshResolution + 1)];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[meshResolution * meshResolution * 6];

        for (int i = 0, y = 0; y <= meshResolution; y++)
        for (var x = 0; x <= meshResolution; x++, i++)
        {
            if (useXZplane) vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, 0, y * scale / meshResolution - 0.5f * scale);
            else vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, y * scale / meshResolution - 0.5f * scale, 0);
                uv[i] = new Vector2(x * scale / meshResolution, y * scale / meshResolution);
        }

        for (int ti = 0, vi = 0, y = 0; y < meshResolution; y++, vi++)
        for (var x = 0; x < meshResolution; x++, ti += 6, vi++)
        {
            triangles[ti] = vi;
            triangles[ti + 3] = triangles[ti + 2] = vi + 1;
            triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution + 1;
            triangles[ti + 5] = vi + meshResolution + 2;
        }

        causticMesh.Clear();
        causticMesh.vertices = vertices;
        causticMesh.uv = uv;
        causticMesh.triangles = triangles;
    }

}
