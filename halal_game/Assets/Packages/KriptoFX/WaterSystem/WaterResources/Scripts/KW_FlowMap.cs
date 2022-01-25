using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class KW_FlowMap : MonoBehaviour
{
   // public int CurrentAreaSize;

   // public Vector3  LastDrawFlowMapPosition;
   private FlowMapData currentFlowmapData;
   private FlowMapData LoadedFlowmapData;

    string FlowMapShaderName = "Hidden/KriptoFX/Water/FlowMapEdit";
    //string flowMapPath     = "/KriptoFX/WaterSystem/FlowMaps/WaterFlowMap.png";
    //string flowMapPathInfo = "/KriptoFX/WaterSystem/FlowMaps/WaterFlowMapInfo.txt";
    private const string path_flowmapFolder = "FlowMaps";
    private const string path_flowmapTexture = "FlowMapTexture";
    private const string path_flowmapData = "FlowMapData";

    public RenderTexture flowmapRT;
    public Texture2D grayTex;
    public Texture2D flowMapTex2D;
    Material _flowMaterial;


    [System.Serializable]
    public class FlowMapData
    {
        [SerializeField] public int AreaSize;
        [SerializeField] public int TextureSize;
    }

    Material flowMaterial
    {
        get
        {
            if(_flowMaterial == null) _flowMaterial = KW_Extensions.CreateMaterial(FlowMapShaderName);
            return _flowMaterial;
        }
    }

    public void Release()
    {
        KW_Extensions.ReleaseRenderTextures(flowmapRT);
        KW_Extensions.SafeDestroy(_flowMaterial, flowMapTex2D, grayTex);
      
    }

    void OnDisable()
    {
        //print("FlowMap.Disabled");
        Release();
    }

    public void ClearFlowMap(string GUID)
    {
        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        var pathToTexture = Path.Combine(pathToDepthDataFolder, path_flowmapFolder, GUID, path_flowmapTexture + ".gz");
        var pathToData = Path.Combine(pathToDepthDataFolder, path_flowmapFolder, GUID, path_flowmapData + ".gz");
        if (File.Exists(pathToTexture)) File.Delete(pathToTexture);
        if (File.Exists(pathToData))  File.Delete(pathToData);

        KW_Extensions.ClearRenderTexture(flowmapRT, Color.gray);
        Shader.SetGlobalTexture("KW_FlowMapTex", flowmapRT);
    }

    public void InitializeFlowMapEditorResources(int size, int areaSize)
    {
        if (flowmapRT == null)
        {
            flowmapRT = KW_Extensions.ReinitializeRenderTexture(flowmapRT, size, size, 0, RenderTextureFormat.ARGBHalf, Color.gray, false, false, TextureWrapMode.Clamp);
        }

        if (flowMapTex2D != null)
        {
            var activeRT = RenderTexture.active;
            Graphics.Blit(flowMapTex2D, flowmapRT);
            RenderTexture.active = activeRT;
            KW_Extensions.SafeDestroy(flowMapTex2D);
        }

        if (currentFlowmapData == null) currentFlowmapData = new FlowMapData();
        currentFlowmapData.AreaSize = areaSize;
        currentFlowmapData.TextureSize = size;
        Shader.SetGlobalTexture("KW_FlowMapTex", flowmapRT);
    }

    private Vector3 flowMapLastClickedPos;
    public void DrawOnFlowMap(Vector3 brushPosition, Vector3 brushMoveDirection, float circleRadius, float brushStrength, bool eraseMode = false)
    {
        float brushSize = currentFlowmapData.AreaSize / circleRadius;
     
        var uv = new Vector2(brushPosition.x / currentFlowmapData.AreaSize + 0.5f, brushPosition.z / currentFlowmapData.AreaSize + 0.5f);
        if (brushMoveDirection.magnitude < 0.001f) brushMoveDirection = Vector3.zero;

        var tempRT = RenderTexture.GetTemporary(flowmapRT.width, flowmapRT.height, 0, flowmapRT.format, RenderTextureReadWrite.Linear);
        tempRT.filterMode = FilterMode.Bilinear;


        flowMaterial.SetVector("_MousePos", uv);
        flowMaterial.SetVector("_Direction", new Vector2(brushMoveDirection.x, brushMoveDirection.z));
        flowMaterial.SetFloat("_Size", brushSize * 0.75f);
        flowMaterial.SetFloat("_BrushStrength", brushStrength / (circleRadius * 3));
        flowMaterial.SetFloat("isErase", eraseMode ? 1 : 0);

        var activeRT = RenderTexture.active;
        Graphics.Blit(flowmapRT, tempRT, flowMaterial, 0);
        Graphics.Blit(tempRT, flowmapRT);
        RenderTexture.active = activeRT;
        RenderTexture.ReleaseTemporary(tempRT);
    }

    public void SaveFlowMap(int areaSize, string GUID)
    {
        if (currentFlowmapData == null) currentFlowmapData = new FlowMapData() {AreaSize =  areaSize};

        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        flowmapRT.SaveRenderTextureToFile(Path.Combine(pathToDepthDataFolder, path_flowmapFolder, GUID, path_flowmapTexture), TextureFormat.RGBAHalf);
        KW_Extensions.SerializeToFile(Path.Combine(pathToDepthDataFolder, path_flowmapFolder, GUID, path_flowmapData), currentFlowmapData);
    }

    public void RedrawFlowMap(int resolution, int newAreaSize)
    {
        var tempRT = RenderTexture.GetTemporary(resolution, resolution, 0, flowmapRT.format, RenderTextureReadWrite.Linear);
        tempRT.filterMode = FilterMode.Bilinear;

        var uvScale = (float)newAreaSize / currentFlowmapData.AreaSize;
        currentFlowmapData.AreaSize = newAreaSize;
        flowMaterial.SetFloat("_UvScale", uvScale);
       // Debug.Log("        flow.RedrawFlowMapArea ");
        var activeRT = RenderTexture.active;
        Graphics.Blit(flowmapRT, tempRT, flowMaterial, 1);
        Graphics.Blit(tempRT, flowmapRT);

        RenderTexture.active = activeRT;
        RenderTexture.ReleaseTemporary(tempRT);
    }

    public async Task<bool> ReadFlowMap(List<Material> sharedMaterials, string GUID)
    {
        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

        LoadedFlowmapData = await KW_Extensions.DeserializeFromFile<FlowMapData>(Path.Combine(pathToDepthDataFolder, path_flowmapFolder, GUID, path_flowmapData));
        if (LoadedFlowmapData == null || LoadedFlowmapData.AreaSize == 0)
        {
            Shader.SetGlobalTexture("KW_FlowMapTex", GrayScaleTexture());
            return false;
        }

        currentFlowmapData = LoadedFlowmapData;
        
        if(flowMapTex2D != null) KW_Extensions.SafeDestroy(flowMapTex2D);
        flowMapTex2D = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToDepthDataFolder, path_flowmapFolder, GUID, path_flowmapTexture), true, FilterMode.Bilinear, TextureWrapMode.Clamp);
        if (flowMapTex2D == null)
        {
            return false;
        }
       
        Shader.SetGlobalTexture("KW_FlowMapTex", flowMapTex2D);
        return true;
    }

    Texture2D GrayScaleTexture()
    {
        if (grayTex == null)
        {
            grayTex = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
            var arr = new Color[4];
            arr[0] = arr[1] = arr[2] = arr[3] = Color.gray;
            grayTex.SetPixels(arr);
            grayTex.Apply();
        }
        return grayTex;
    }

    public FlowMapData GetFlowMapDataFromFile()
    {
        return LoadedFlowmapData;
    }
}

