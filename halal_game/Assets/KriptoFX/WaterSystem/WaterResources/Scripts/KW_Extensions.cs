using System;
using System.Collections.Generic;

using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;


public static class KW_Extensions {

    static float prevRealTime;
    private static float lastDeltaTime;

    public static void SetKeyword(this Material mat, string keyword, bool state)
    {
        if (state)
            mat.EnableKeyword(keyword);
        else
            mat.DisableKeyword(keyword);
    }

    public static void SetKeyword(this CommandBuffer buffer, string keyword, bool state)
    {
        if (state)
            buffer.EnableShaderKeyword(keyword);
        else
            buffer.EnableShaderKeyword(keyword);
    }

    public static RenderTextureFormat GetRenderTextureFormatHDR()
    {
        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float)) return RenderTextureFormat.RGB111110Float;
        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB2101010)) return RenderTextureFormat.ARGB2101010;
        return RenderTextureFormat.DefaultHDR;
    }

    public static RenderTexture ReinitializeRenderTexture(RenderTexture rt, int width, int height, int depth, RenderTextureFormat format, 
        Color? clearColor = null, bool useRandomWrite = false, bool useMipMap = false, TextureWrapMode wrapMode = TextureWrapMode.Repeat, FilterMode filterMode = FilterMode.Bilinear)
    {
        if (rt == null || rt.width != width || rt.height != height)
        {
            if (rt != null)
            {
                rt.Release();
                rt = null;
            }
            
            rt = new RenderTexture(width, height, depth, format, RenderTextureReadWrite.Linear);
            if (useRandomWrite) rt.enableRandomWrite = useRandomWrite;
            if(wrapMode != TextureWrapMode.Repeat) rt.wrapMode = wrapMode;
            if(filterMode != FilterMode.Bilinear) rt.filterMode = filterMode;
            if (clearColor != null) ClearRenderTexture(rt, (Color)clearColor);
            if (useMipMap) rt.useMipMap = true;
            //Debug.Log("ReinitializeRenderTexture " + rt);
        }

        return rt;
    }

    public static void ReleaseRenderTextures(params RenderTexture[] renderTextures)
    {
        for (var i = 0; i < renderTextures.Length; i++)
        {
            if(renderTextures[i] == null) continue;
            renderTextures[i].Release();
            renderTextures[i] = null;
        }
    }

    public static void ClearRenderTexture(RenderTexture rt, Color color)
    {
        var activeRT = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, color);
        RenderTexture.active = activeRT;
    }

    public static Vector3 GetRelativeToCameraAreaPos(Camera cam, float areaSize, float waterLevel)
    {
        var pos                      = cam.transform.position;
        var bottomCornerViewWorldPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0, cam.nearClipPlane));
        var cornerDir                = (bottomCornerViewWorldPos - pos).normalized;
        var cornerDotX               = Vector3.Dot(cornerDir, Vector3.right)   / 2.0f + 0.5f;
        var cornerDotZ               = Vector3.Dot(cornerDir, Vector3.forward) / 2.0f + 0.5f;

        float offsetX = Mathf.Lerp(-areaSize, areaSize, cornerDotX) * 0.5f;
        float offsetZ = Mathf.Lerp(-areaSize, areaSize, cornerDotZ) * 0.5f;
        return new Vector3(pos.x + offsetX, waterLevel, pos.z + offsetZ);
    }

    public static CameraEvent GetEventBeforeEverything(Camera currentCamera)
    {
        if (currentCamera.actualRenderingPath == RenderingPath.DeferredLighting || currentCamera.actualRenderingPath == RenderingPath.DeferredShading)
            return CameraEvent.BeforeGBuffer;
        else
            return CameraEvent.BeforeDepthTexture;
    }

    public static bool IsCameraContainsBuffer(Camera cam, string name, params CameraEvent[] events)
    {
        if (cam == null) return false;

        foreach (var e in events)
        {
            var eventBuffers = cam.GetCommandBuffers(e);
            foreach (var currentBuffer in eventBuffers)
            {
                if (currentBuffer.name == name) return true;
            }
        }

        return false;
    }

    public static void RemoveCommandBuffersByName(Camera cam, string name, params CameraEvent[] camEvents)
    {
        if (cam == null) return;
        foreach (var camEvent in camEvents)
        {
            var allBuffers = cam.GetCommandBuffers(camEvent);
            foreach (var currentBuffer in allBuffers)
            {
                if (currentBuffer.name == name) cam.RemoveCommandBuffer(camEvent, currentBuffer);
            }
        }

    }

    public static void RemoveCommandBuffersByName(Light light, string name, LightEvent lightEvent)
    {
        if (light == null) return;

        var allBuffers = light.GetCommandBuffers(lightEvent);
        foreach (var currentBuffer in allBuffers)
        {
            if (currentBuffer.name == name) light.RemoveCommandBuffer(lightEvent, currentBuffer);
        }
    }

    public static Material CreateMaterial(string shaderName)
    {
        var waterShader = Shader.Find(shaderName);
        if (waterShader == null)
            Debug.LogError("Can't find the shader '" + shaderName +  "' in the resources folder. Try to reimport the package.");

        var waterMaterial = new Material(waterShader);
        waterMaterial.hideFlags = HideFlags.DontSave;
        return waterMaterial;
    }

    public static void SafeDestroy(params UnityEngine.Object[] components)
    {

        if (!Application.isPlaying)
        {
            foreach (var component in components)
            {
                if(component != null) UnityEngine.Object.DestroyImmediate(component);
            }

        }
        else
        {
            foreach (var component in components)
            {
                if (component != null) UnityEngine.Object.Destroy(component);
            }
        }
    }

    public static float Time()
    {

        //return 0;
        return (Application.isPlaying ? UnityEngine.Time.time : UnityEngine.Time.realtimeSinceStartup);
    }

    public static float DeltaTime()
    {
        if (Application.isPlaying)
        {
            return UnityEngine.Time.deltaTime;
        }
        else
        {
            return lastDeltaTime;
        }
    }

    public static void UpdateDeltaTime()
    {
        if (Application.isPlaying) return;

        lastDeltaTime = UnityEngine.Time.realtimeSinceStartup - prevRealTime;
        prevRealTime = UnityEngine.Time.realtimeSinceStartup;
    }

    public static void Blit(RenderTexture destination, Material material, int materialPass = 0)
    {
        if (material.SetPass(materialPass))
        {
            // This is always reached.
            Graphics.SetRenderTarget(destination);

            GL.PushMatrix();
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            {
                Vector3 coords = new Vector3(0, 0, 0);
                GL.TexCoord(coords);
                GL.Vertex(coords);

                coords = new Vector3(1, 0, 0);
                GL.TexCoord(coords);
                GL.Vertex(coords);

                coords = new Vector3(1, 1, 0);
                GL.TexCoord(coords);
                GL.Vertex(coords);

                coords = new Vector3(0, 1, 0);
                GL.TexCoord(coords);
                GL.Vertex(coords);
            }
            GL.End();

            GL.PopMatrix();
        }
    }

    public static string GetPathToStreamingAssetsFolder()
    {
        
        var streamingAssetData = Path.Combine(Application.streamingAssetsPath, "WaterSystemData");
        if (Directory.Exists(streamingAssetData)) return streamingAssetData;

        var dirs = Directory.GetDirectories(Application.dataPath, "WaterSystemData", SearchOption.AllDirectories);
        if (dirs.Length != 0) 
        {
            if (Directory.Exists(dirs[0])) return dirs[0];
        }

        Debug.LogError("Can't find 'Assets/StreamingAssets/WaterSystemData' data folder");
        return string.Empty;
    }


    public static void SerializeToFile(string pathToFileWithoutExtenstion, object data)
    {
        var directory = Path.GetDirectoryName(pathToFileWithoutExtenstion);
        if (directory == null || data == null)
        {
            Debug.LogError("Can't find directory: " + pathToFileWithoutExtenstion);
            return;
        }
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        //var fs = new FileStream(pathToFileWithoutExtenstion + ".dat", FileMode.Create);

        using (var fileToCompress = File.Create(pathToFileWithoutExtenstion + ".gz"))
        {
            using (GZipStream compressionStream = new GZipStream(fileToCompress, CompressionMode.Compress))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(compressionStream, data);
                compressionStream.Close();
            }
        }

    }

    public static async Task<T> DeserializeFromFile<T>(string fileNameWithoutExtenstion)
    {
        fileNameWithoutExtenstion += ".gz";
        var directory = Path.GetDirectoryName(fileNameWithoutExtenstion);
        if (!Directory.Exists(directory) || !File.Exists(fileNameWithoutExtenstion)) return default;
        try
        {

            using (var fileStream = File.Open(fileNameWithoutExtenstion, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            {
                using (GZipStream decompressionStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await decompressionStream.CopyToAsync(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        var bformatter = new BinaryFormatter();
                        var data = (T)bformatter.Deserialize(stream);

                        return data;
                    }

                }

            }
        }
        catch (Exception e)
        {
            Debug.Log("Error Deserialize From File " + Path.GetFileName(fileNameWithoutExtenstion) +
                      Environment.NewLine + e.Message);
            return default;
        }
    }



    public static byte[] Combine(byte[] first, byte[] second)
    {
        byte[] bytes = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
        Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
        return bytes;
    }

    [System.Serializable]
    class SerializableMeshInfo
    {
        [SerializeField] public float[] verticesX;
        [SerializeField] public float[] verticesY;
        [SerializeField] public float[] verticesZ;

        [SerializeField] public int[] tris;

        [SerializeField] public float[] uv_x;
        [SerializeField] public float[] uv_y;

        [SerializeField] public float[] uv2_x;
        [SerializeField] public float[] uv2_y;
        [SerializeField] public float[] uv2_z;
        [SerializeField] public float[] uv2_w;
    }

    public static void SerializeMeshToFile(Vector3[] vertices, int[] tris, Vector2[] uv, Vector4[] uv2, string pathToFileWithoutExtension)
    {
        var meshInfo = new SerializableMeshInfo();
        meshInfo.verticesX = new float[vertices.Length];
        meshInfo.verticesY = new float[vertices.Length];
        meshInfo.verticesZ = new float[vertices.Length];
        meshInfo.tris = new int[tris.Length];
        meshInfo.uv_x = new float[uv.Length];
        meshInfo.uv_y = new float[uv.Length];
        meshInfo.uv2_x = new float[uv2.Length];
        meshInfo.uv2_y = new float[uv2.Length];
        meshInfo.uv2_z = new float[uv2.Length];
        meshInfo.uv2_w = new float[uv2.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            meshInfo.verticesX[i] = vertices[i].x;
            meshInfo.verticesY[i] = vertices[i].y;
            meshInfo.verticesZ[i] = vertices[i].z;
        }

        for (int i = 0; i < tris.Length; i++)
        {
            meshInfo.tris[i] = tris[i];
        }

        for (int i = 0; i < uv.Length; i++)
        {
            meshInfo.uv_x[i] = uv[i].x;
            meshInfo.uv_y[i] = uv[i].y;
        }

        for (int i = 0; i < uv2.Length; i++)
        {
            meshInfo.uv2_x[i] = uv2[i].x;
            meshInfo.uv2_y[i] = uv2[i].y;
            meshInfo.uv2_z[i] = uv2[i].z;
            meshInfo.uv2_w[i] = uv2[i].w;
        }

        SerializeToFile(pathToFileWithoutExtension, meshInfo);
    }

    public class MeshData
    {
        public Vector3[] vertices;
        public int[] tris;
        public Vector2[] uv;
        public Vector4[] uv2;
    }

    public static async Task<Mesh> DeserializeMeshFromFile(string filenameWithoutExtension, Vector3 bounds)
    {
        var meshDataResult = await Task.Run(async () =>
        {
            var sMeshData = await DeserializeFromFile<SerializableMeshInfo>(filenameWithoutExtension);
            if (sMeshData == null || sMeshData.verticesX == null) return null;

            Vector3[] vertices = new Vector3[sMeshData.verticesX.Length];
            int[] tris = new int[sMeshData.tris.Length];
            Vector2[] uv = new Vector2[sMeshData.uv_x.Length];
            Vector4[] uv2 = new Vector4[sMeshData.uv2_x.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(sMeshData.verticesX[i], sMeshData.verticesY[i], sMeshData.verticesZ[i]);
            }

            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] = sMeshData.tris[i];
            }

            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = new Vector2(sMeshData.uv_x[i], sMeshData.uv_y[i]);
            }

            for (int i = 0; i < uv2.Length; i++)
            {
                uv2[i] = new Vector4(sMeshData.uv2_x[i], sMeshData.uv2_y[i], sMeshData.uv2_z[i], sMeshData.uv2_w[i]);
            }

            var meshData = new MeshData();
            meshData.vertices = vertices;
            meshData.tris = tris;
            meshData.uv = uv;
            meshData.uv2 = uv2;

            return meshData;
        });
        if (meshDataResult == null) return null;

        var mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = meshDataResult.vertices;
        mesh.triangles = meshDataResult.tris;
        mesh.uv = meshDataResult.uv;
        mesh.SetUVs(1, meshDataResult.uv2.ToList());
        mesh.bounds = new Bounds(Vector3.zero, bounds);
        return mesh;
    }

    public static async Task<Texture2D> ReadTextureFromFileAsync(string pathToFileWithoutExtension, bool linear = true, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Repeat)
    {
        Texture2D tex;

        if (!File.Exists(pathToFileWithoutExtension + ".gz"))
        {
            Debug.LogError("Can't find the file: " + pathToFileWithoutExtension);
            return null;
        }
        try
        {
            //Debug.Log("read " + pathToFileWithoutExtension);
            using (var fileStream = File.Open(pathToFileWithoutExtension + ".gz", FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            {
                using (GZipStream gzip = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await gzip.CopyToAsync(stream);

                        var rawTextureDataWithInfo = new byte[stream.Length];
                        stream.Seek(0, SeekOrigin.Begin);
                        await stream.ReadAsync(rawTextureDataWithInfo, 0, rawTextureDataWithInfo.Length);
                        {
                            var format = (TextureFormat)BitConverter.ToInt32(rawTextureDataWithInfo, 0);
                            int width = BitConverter.ToInt32(rawTextureDataWithInfo, 4);
                            int height = BitConverter.ToInt32(rawTextureDataWithInfo, 8);

                            var rawTextureData = new byte[rawTextureDataWithInfo.Length - 12];
                            Array.Copy(rawTextureDataWithInfo, 12, rawTextureData, 0, rawTextureData.Length);

                            //var gFormat = GraphicsFormatUtility.GetGraphicsFormat(format, false);
                            tex = new Texture2D(width, height, format, false, true);

                            tex.filterMode = filterMode;
                            tex.wrapMode = wrapMode;
                            tex.LoadRawTextureData(rawTextureData);
                            tex.Apply();
                        }
                        stream.Close();
                        gzip.Close();
                    }
                }
                fileStream.Close();
            }
            return tex;
        }
        catch (Exception e)
        {
            Debug.LogError("ReadTextureFromFileAsync error: " + e.Message);
            return null;
        }
    }

    public static void SaveTextureToFile(this Texture2D tex, string pathToFileWithoutExtension)
    {
        var directory = Path.GetDirectoryName(pathToFileWithoutExtension);
        if (directory == null)
        {
            Debug.LogError("Can't find directory: " + pathToFileWithoutExtension);
            return;
        }
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        var fullPath = pathToFileWithoutExtension + ".gz";
        if (File.Exists(fullPath)) File.Delete(fullPath);
        try
        {
            //Debug.Log("save " + pathToFileWithoutExtension);
            using(var fileToCompress = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.Delete))
            {
                int w = tex.width;
                int h = tex.height;
                var rawTextureData = tex.GetRawTextureData();
                var textureInfo = new List<byte>();

                textureInfo.AddRange(BitConverter.GetBytes((uint)tex.format));
                textureInfo.AddRange(BitConverter.GetBytes(w));
                textureInfo.AddRange(BitConverter.GetBytes(h));
                rawTextureData = Combine(textureInfo.ToArray(), rawTextureData);

                using (GZipStream compressionStream = new GZipStream(fileToCompress, CompressionMode.Compress))
                {
                    compressionStream.Write(rawTextureData, 0, rawTextureData.Length);
                    compressionStream.Close();
                }
                fileToCompress.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SaveTextureToFile error: " + e.Message);
            return;
        }
    }

    public static void SaveRenderTextureToFile(this RenderTexture rt, string pathToFileWithoutExtension, TextureFormat targetFormat)
    {
#if UNITY_EDITOR
        if (rt == null) return;
        var currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false, true);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        UnityEditor.EditorUtility.CompressTexture(tex, targetFormat, UnityEditor.TextureCompressionQuality.Best);
        tex.SaveTextureToFile(pathToFileWithoutExtension);
        RenderTexture.active = currentRT;
        SafeDestroy(tex);
        //SafeDestroy(tex2);
#endif
    }

    public class RenderTextureTemp
    {
        public int id { get; set; }
        public RenderTargetIdentifier identifier { get; }

        public RenderTextureDescriptor descriptor;

        public RenderTextureTemp(CommandBuffer cmd, string name, int width, int height, int depth, RenderTextureFormat format, bool isLinear = true, FilterMode filterMode = FilterMode.Bilinear)
        {
            id = Shader.PropertyToID(name);
            identifier = new RenderTargetIdentifier(name);
            descriptor = new RenderTextureDescriptor(width, height, format, depth);
            descriptor.sRGB = !isLinear;
            descriptor.useMipMap = false;


            cmd.GetTemporaryRT(id, descriptor, filterMode);
        }
        public RenderTextureTemp(CommandBuffer cmd, string name, int width, int height, int depth, RenderTextureFormat format, bool isLinear, FilterMode filterMode = FilterMode.Bilinear, bool enableRandomWrite = false, bool useMipMap = false, TextureDimension dimension = TextureDimension.Tex2D, int MSAA = 1)
        {
            id = Shader.PropertyToID(name);
            identifier = new RenderTargetIdentifier(name);
            descriptor = new RenderTextureDescriptor(width, height, format, depth);
            descriptor.sRGB = !isLinear;
            descriptor.useMipMap = useMipMap;
            descriptor.dimension = dimension;
            descriptor.enableRandomWrite = enableRandomWrite;
            descriptor.msaaSamples = MSAA;
            cmd.GetTemporaryRT(id, descriptor, filterMode);
        }
    }

    public static void CameraRender(Camera cam)
    {
#if UNITY_PIPELINE_URP
        var context = KW_WaterDynamicScripts.GetCurrentWaterContext();
        if(context != null) UnityEngine.Rendering.Universal.UniversalRenderPipeline.RenderSingleCamera(context, cam);
#elif UNITY_PIPELINE_HDRP

#else
        cam.Render();
#endif

    }

    static PipelineType GetPipeline()
    {
#if UNITY_2019_1_OR_NEWER
        if (GraphicsSettings.renderPipelineAsset != null)
        {
            var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
            if (srpType.Contains("HDRenderPipelineAsset"))
            {
                return PipelineType.HDPipeline;
            }
            else if (srpType.Contains("UniversalRenderPipelineAsset"))
            {
                return PipelineType.UniversalPipeline;
            }
            else return PipelineType.Unsupported;
        }
#endif
       
        return PipelineType.BuiltInPipeline;
    }

    enum PipelineType
    {
        Unsupported,
        BuiltInPipeline,
        UniversalPipeline,
        HDPipeline
    }
}
