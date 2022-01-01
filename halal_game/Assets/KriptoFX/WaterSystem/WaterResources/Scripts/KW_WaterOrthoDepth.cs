using System.IO;
using System.Threading.Tasks;

using UnityEngine;


public static class KW_WaterOrthoDepth
{
    private const int WaterLayer = (1 << 4);

    [System.Serializable]
    public class OrthoDepthParams
    {
        [SerializeField] public int OtrhograpicSize;
        [SerializeField] public float PositionX;
        [SerializeField] public float PositionY;
        [SerializeField] public float PositionZ;
        public void SetData(int orthoSize, Vector3 pos)
        {
            OtrhograpicSize = orthoSize;
            PositionX = pos.x;
            PositionY = pos.y;
            PositionZ = pos.z;
        }
    }

    public static RenderTexture ReinitializeDepthTexture(RenderTexture depth_rt, int size)
    {
        return KW_Extensions.ReinitializeRenderTexture(depth_rt, size, size, 32, RenderTextureFormat.Depth, null, false, false, TextureWrapMode.Clamp);
    }

    public static Camera InitializeDepthCamera(float nearPlane, float farPlane, Transform parent)
    {

        var cameraGO = new GameObject("TopViewDepthCamera");
        cameraGO.transform.parent = parent;
        var depthCam = cameraGO.AddComponent<Camera>();
       
        depthCam.renderingPath = RenderingPath.Forward;
        depthCam.orthographic = true;
        depthCam.allowMSAA = false;
        depthCam.allowHDR = false;
        depthCam.nearClipPlane = nearPlane;
        depthCam.farClipPlane = farPlane;
        depthCam.transform.rotation = Quaternion.Euler(90, 0, 0);
        depthCam.enabled = false;

        return depthCam;
    }     

    public static RenderTexture RenderDepth(Camera cam, RenderTexture depth_rt, Vector3 position, int depthAreaSize, int depthTextureSize)
    {
        depth_rt = ReinitializeDepthTexture(depth_rt, depthTextureSize);
        
        cam.orthographicSize = depthAreaSize * 0.5f;
        cam.transform.position = position;
        cam.cullingMask = ~WaterLayer;
        cam.targetTexture = depth_rt;
        cam.Render();
        //KW_Extensions.CameraRender(cam);

        return depth_rt;
    }

    public static void SaveDepthTextureToFile(RenderTexture depth_rt, string path)
    {
        if(depth_rt == null)
        {
            Debug.LogError("Can't save ortho depth");
            return;
        }
        var tempRT = new RenderTexture(depth_rt.width, depth_rt.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        Graphics.Blit(depth_rt, tempRT);
        tempRT.SaveRenderTextureToFile(path, TextureFormat.RFloat);
        tempRT.Release();
    }

    public static void SaveDepthDataToFile(OrthoDepthParams depthParams, string path)
    {
        KW_Extensions.SerializeToFile(path, depthParams);
    }

    public static void RenderAndSaveDepth(Transform parent, Vector3 position, int areaSize, int textureSize, float nearPlane, float farPlane, string pathToTexture, string pathToData)
    {
        var cam = InitializeDepthCamera(nearPlane, farPlane, parent);
        var depth_rt = ReinitializeDepthTexture(null, textureSize);
        RenderDepth(cam, depth_rt, position, areaSize, textureSize);

        var depthParams = new OrthoDepthParams();
        depthParams.SetData(areaSize, position);
        SaveDepthTextureToFile(depth_rt, pathToTexture);
        SaveDepthDataToFile(depthParams, pathToData);
        KW_Extensions.SafeDestroy(cam.gameObject);
        KW_Extensions.ReleaseRenderTextures(depth_rt);
    }


    //public async Task<Texture2D> ReadDepthFromFileAsync(string path)
    //{
    //    var depthTex = await KW_Extensions.ReadTextureFromFileAsync(path, true, FilterMode.Bilinear, TextureWrapMode.Clamp);
    //    return depthTex;
    //}

    //public async Task<OrthoDepthParams> ReadDepthDataFromFileAsync(string path)
    //{
    //    currentOrthoDepthParams = await KW_Extensions.DeserializeFromFile<OrthoDepthParams>(path);
    //    return currentOrthoDepthParams;
    //}
    
    //public RenderTexture GetCurrentDepthRT()
    //{
    //    return depth_rt;
    //}
}
