using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class KW_FFT_HeightData : MonoBehaviour
{
    Material dataHeightMaterial;
    public RenderTexture HeighDataTexture;
    public Mesh heightDataMesh;
    GameObject camGO;
    Camera cam;
    GameObject meshGO;

    Unity.Collections.NativeArray<byte> rawHeightData;
    float currentHeightDataDomainScale;
    int lastSize;
    float waterHeight;
    float cameraHeight = 21231;

    bool isHeightDataUpdated;

    public delegate void HeightDataHandler();
    public event HeightDataHandler IsDataReadCompleted;

    public void Release()
    {
        KW_Extensions.SafeDestroy(dataHeightMaterial, heightDataMesh, camGO, meshGO);
        KW_Extensions.ReleaseRenderTextures(HeighDataTexture);
        lastSize = 0;

        if (cubes != null)
        {
            foreach (var cube in cubes)
            {
                KW_Extensions.SafeDestroy(cube);
            }
        }
        isHeightDataUpdated = false;
    }

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if(dataHeightMaterial ==null) dataHeightMaterial = KW_Extensions.CreateMaterial("Hidden/KriptoFX/Water/KW_FFT_Height");
        if (!waterShaderMaterials.Contains(dataHeightMaterial)) waterShaderMaterials.Add(dataHeightMaterial);
    }

    void InitializeResources(int size, float domainScale)
    {
        currentHeightDataDomainScale = domainScale;
        HeighDataTexture = KW_Extensions.ReinitializeRenderTexture(HeighDataTexture, size, size, 0, RenderTextureFormat.R8, null, false, true);
       
        GeneratePlane(size, currentHeightDataDomainScale * 1.1f, true);
        if (meshGO == null)
        {
            meshGO = new GameObject("FFT_HeightDataMesh");
            meshGO.AddComponent<MeshRenderer>().sharedMaterial = dataHeightMaterial;
            meshGO.AddComponent<MeshFilter>();
            meshGO.transform.parent = transform;
            meshGO.transform.localPosition += Vector3.up * cameraHeight;
            meshGO.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        meshGO.GetComponent<MeshFilter>().sharedMesh = heightDataMesh;

        if (camGO == null) InitializeCamera();
        cam.orthographicSize = currentHeightDataDomainScale * 0.5f;
        cam.targetTexture = HeighDataTexture;

        lastSize = size;
        isHeightDataUpdated = false;
    }

    void InitializeCamera()
    {
        camGO = new GameObject("FFT_HeightDataCamera");
        camGO.transform.parent = transform;
        camGO.transform.localPosition += Vector3.up * cameraHeight;
        camGO.transform.rotation = Quaternion.Euler(90, 0, 0);

        cam = camGO.AddComponent<Camera>();
        cam.renderingPath = RenderingPath.Forward;
        cam.depthTextureMode = DepthTextureMode.None;

        cam.orthographic = true;
        cam.allowMSAA = false;
        cam.allowHDR = false;
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = -10;
        cam.farClipPlane = 10;
        cam.enabled = false;
    }

    public Vector3? GetWaterSurfaceHeight(Vector3 worldPosition)
    {
        if (!isHeightDataUpdated) return null;

        var domainScale = currentHeightDataDomainScale;
        var x = (worldPosition.x + domainScale * 0.5f) % domainScale;
        var z = (worldPosition.z + domainScale * 0.5f) % domainScale;

        if (x < 0) x = domainScale + x;
        if (z < 0) z = domainScale + z;

        x = HeighDataTexture.width * (x / domainScale);
        z = HeighDataTexture.height * (z / domainScale);

        var pixelIdx = (int)x + HeighDataTexture.height * (int)z;
        if (!rawHeightData.IsCreated || pixelIdx > rawHeightData.Length - 1) return worldPosition;

        var waterPos = new Vector3(worldPosition.x, (rawHeightData[pixelIdx] / 255.0f) * 20.0f - 10.0f + waterHeight, worldPosition.z);
      //  Debug.Log(waterPos);
        return waterPos;
    }

   // private NativeArray<float> _tempBuffer;
    public void UpdateHeightData(int size, float domainScale, float _waterHeight)
    {
        waterHeight = _waterHeight;
        if (size != lastSize || currentHeightDataDomainScale != domainScale) InitializeResources(size, domainScale);
        KW_Extensions.CameraRender(cam);
        //  ReadIsComplete = false;
        isHeightDataUpdated = false;
#if UNITY_EDITOR //memory leak fix in pause mode
        if(UnityEditor.EditorApplication.isPlaying) AsyncGPUReadback.Request(HeighDataTexture, 0, OnCompleteGPUReadback);
#else
        AsyncGPUReadback.Request(HeighDataTexture, 0, OnCompleteGPUReadback);
#endif

        //  TestDataHeight();
    }

    private void OnCompleteGPUReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }
        if (request.done)
        {
            rawHeightData = request.GetData<byte>();
            isHeightDataUpdated = true;
            IsDataReadCompleted?.Invoke();
        }
        
      
        //  TestDataHeight();
    }

    private void GeneratePlane(int meshResolution, float scale, bool useXZplane = true)
    {
        var trisCount = meshResolution * meshResolution * 6;
        if (heightDataMesh == null)
        {
            heightDataMesh = new Mesh();
            heightDataMesh.indexFormat = IndexFormat.UInt32;
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

        heightDataMesh.Clear();
        heightDataMesh.vertices = vertices;
        heightDataMesh.uv = uv;
        heightDataMesh.triangles = triangles;
    }


    List<GameObject> cubes;

}
