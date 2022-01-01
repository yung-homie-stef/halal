using System;
using System.Collections.Generic;
using UnityEngine;

public class KW_DynamicWaves : MonoBehaviour
{
    const string dynamicWavesShaderName = "Hidden/KriptoFX/Water/DynamicWaves";
    string keyword_UseRainEffect = "KW_USE_RAIN_EFFECT";

    WavesData[] wavesData = new WavesData[3];

    private int frameNumber;
    //List<DrawPointInfo> drawPoints = new List<DrawPointInfo>(100);
    private int currentDrawIdx;
    private float[] drawPointsX = new float[KW_WaterDynamicScripts.DefaultInteractWavesCapacity];
    private float[] drawPointsY = new float[KW_WaterDynamicScripts.DefaultInteractWavesCapacity];
    private float[] drawPointsSize = new float[KW_WaterDynamicScripts.DefaultInteractWavesCapacity];
    private float[] drawPointsForce = new float[KW_WaterDynamicScripts.DefaultInteractWavesCapacity];

    public Mesh GridVBO;
    private Material dynamicWavesMaterial;
    public RenderTexture dynamicObjectsRT;

    private Vector3[]     vertices;
    //  private Color[]       colors;
    private List<Vector2> forces;
    private List<Vector3> uv;
    private int[]         triangles;

    private int lastWidth;
    private int lastHeight;

    float rainLeftFramesBeforeSpawn;

    class WavesData
    {
        public RenderTexture DataRT;
        public RenderTexture NormalRT;
        public RenderBuffer[] MRT = new RenderBuffer[2];
        public Vector3 WorldOffset;
        public int ID;
    }

    void InitializeTextures(int width, int height)
    {
        for (int i = 0; i < 3; i++)
        {
            if (wavesData[i] == null) wavesData[i] = new WavesData();
            wavesData[i].DataRT = KW_Extensions.ReinitializeRenderTexture(wavesData[i].DataRT, width, height, 0, RenderTextureFormat.RFloat);
            wavesData[i].NormalRT = KW_Extensions.ReinitializeRenderTexture(wavesData[i].NormalRT, width, height, 0, RenderTextureFormat.RGHalf);
            wavesData[i].MRT[0] = wavesData[i].DataRT.colorBuffer;
            wavesData[i].MRT[1] = wavesData[i].NormalRT.colorBuffer;
            wavesData[i].ID = i + 1;
        }
        dynamicObjectsRT = KW_Extensions.ReinitializeRenderTexture(dynamicObjectsRT, width, height, 0, RenderTextureFormat.RHalf);
        lastWidth = width;
        lastHeight = height;
    }

  
    void OnDisable()
    {
        for (int i = 0; i < 3; i++)
        {
            if (wavesData[i] != null)
            {
                KW_Extensions.ReleaseRenderTextures(wavesData[i].DataRT, wavesData[i].NormalRT);
                wavesData[i].WorldOffset = Vector3.zero;
            }
        }
        KW_Extensions.SafeDestroy(dynamicWavesMaterial, GridVBO);
        KW_Extensions.ReleaseRenderTextures(dynamicObjectsRT);
        //lastPosition = null;
        lastWidth = 0;
        lastHeight = 0;
    }

    public void Release()
    {
        OnDisable();
    }


    void IncreaseDrawArray()
    {
        var newSize = (int) (drawPointsX.Length * 1.5f); //increase capacity on 50%
        Array.Resize(ref drawPointsX, newSize);
        Array.Resize(ref drawPointsY, newSize);
        Array.Resize(ref drawPointsSize, newSize);
        Array.Resize(ref drawPointsForce, newSize);
        
        KW_Extensions.SafeDestroy(GridVBO);
        CreateGridVBO(newSize);
    }

    public void AddPositionToDrawArray(Vector3 areaPos, Vector3 position, float size, float force, float areaSize)
    {
        if (currentDrawIdx >= drawPointsX.Length) IncreaseDrawArray();
      
        areaSize *= 0.5f;
        position -= areaPos;
        drawPointsX[currentDrawIdx] = (position.x / areaSize) * 0.5f + 0.5f;
        drawPointsY[currentDrawIdx] = (position.z / areaSize) * 0.5f + 0.5f;
        drawPointsSize[currentDrawIdx] = size;
        drawPointsForce[currentDrawIdx] = force;
        ++currentDrawIdx;
    }


    private Vector3 lastInteractPos;
    public Vector3 InteractPos; //relative to camera frustrum area position center
  

    Vector3 ComputeAreaSimulationJitter(float offset)
    {
        var randTime = Time.time * UnityEngine.Random.Range(20, 60);
        var jitterSin = Mathf.Sin(randTime);
        var jitterCos = Mathf.Cos(randTime);
        var jitter = new Vector3(offset * jitterSin, 0, offset * jitterCos);
       
        return jitter;
    }

    public void RenderWaves(Camera currentCamera, int fps, int areaSize, int pixelsPerMeter, float propagationSpeed, float rainStrength)
    {
      
        var bufferSize = pixelsPerMeter * areaSize;
        

        bufferSize = Mathf.Min(bufferSize, 2048);
        if (lastWidth != bufferSize || lastHeight != bufferSize) InitializeTextures(bufferSize, bufferSize);
        if (GridVBO == null) CreateGridVBO(KW_WaterDynamicScripts.DefaultInteractWavesCapacity);

        if (dynamicWavesMaterial == null) dynamicWavesMaterial = KW_Extensions.CreateMaterial(dynamicWavesShaderName);

        int endIndex;
        var interactScripts = KW_WaterDynamicScripts.GetInteractScriptsInArea(InteractPos, areaSize, out endIndex);

        InteractPos = KW_Extensions.GetRelativeToCameraAreaPos(currentCamera, areaSize, transform.position.y);
        InteractPos += ComputeAreaSimulationJitter(5f / bufferSize);

        for (var i = 0; i < endIndex; i++)
        {
            var script = interactScripts[i];
            var force = script.GetForce(InteractPos.y);
            var intersectedSize = script.GetIntersectionSize();
            AddPositionToDrawArray(InteractPos, script.t.position + script.Offset, intersectedSize, force, areaSize);
        }
        UpdateVBO(areaSize);
        DrawInstancedArrayToTexture(dynamicObjectsRT, dynamicWavesMaterial, areaSize);
      
        Shader.SetGlobalVector("KW_DynamicWavesWorldPos", InteractPos);
        Shader.SetGlobalFloat("KW_DynamicWavesAreaSize", areaSize);
        Shader.SetGlobalTexture("KW_DynamicObjectsMap", dynamicObjectsRT);

        var offset = (InteractPos - lastInteractPos) / (areaSize);

     

        if(rainStrength > 0.001)
        {
            var rainThreshold = Mathf.Lerp(0.9999999f, 0.9992f, rainStrength);
            dynamicWavesMaterial.SetFloat("KW_DynamicWavesRainThreshold", rainThreshold);

            var scaledRainStrength = Mathf.Lerp(0.05f, 0.25f, Mathf.Pow(rainStrength, 5));
            dynamicWavesMaterial.SetFloat("KW_DynamicWavesRainStrength", scaledRainStrength);
            dynamicWavesMaterial.EnableKeyword(keyword_UseRainEffect);
        }
        else dynamicWavesMaterial.DisableKeyword(keyword_UseRainEffect);

       

        UpdateDynamicWavesLod(wavesData[0], wavesData[1], wavesData[2], areaSize, propagationSpeed, offset);
      
        //if (quality == WaterSystem.QualityEnum.High)
        //{
        //    UpdateDynamicWavesLod(wavesData[0], wavesData[1], wavesData[2], areaSize, propagationSpeed, Vector3.zero);
        //}
        lastInteractPos = InteractPos;
    }


    void UpdateDynamicWavesLod(WavesData data1, WavesData data2, WavesData data3, int areaSize, float pixelSpeed, Vector3 offset)
    {

        WavesData lastSource, source, target;
        if(frameNumber == 0)
        {
            lastSource = data1;
            source = data2;
            target = data3; 
        }
        else if (frameNumber == 1)
        {
            lastSource = data2;
            source = data3;
            target = data1;
        }
        else
        {
            lastSource = data3;
            source = data1;
            target = data2;
        }
        frameNumber++;
        if (frameNumber > 2) frameNumber = 0;
        target.WorldOffset = offset;
       

        dynamicWavesMaterial.SetFloat("KW_InteractiveWavesPixelSpeed", pixelSpeed);
        dynamicWavesMaterial.SetTexture("_PrevTex", lastSource.DataRT);
        dynamicWavesMaterial.SetTexture("_PrevTex", lastSource.DataRT);
        dynamicWavesMaterial.SetVector("KW_AreaOffset", offset);
        dynamicWavesMaterial.SetVector("KW_LastAreaOffset", source.WorldOffset + offset);

        Graphics.SetRenderTarget(target.MRT, target.DataRT.depthBuffer);
        Graphics.Blit(source.DataRT, dynamicWavesMaterial, 1);
    
        dynamicWavesMaterial.SetFloat("KW_InteractiveWavesAreaSize", areaSize);
       
        Shader.SetGlobalTexture("KW_DynamicWaves", target.DataRT);
        Shader.SetGlobalTexture("KW_DynamicWavesPrev", source.DataRT);
        Shader.SetGlobalTexture("KW_DynamicWavesNormal", target.NormalRT);
        Shader.SetGlobalTexture("KW_DynamicWavesNormalPrev", source.NormalRT);
        Shader.SetGlobalFloat("KW_DynamicWavesAreaSize", areaSize);

    }


    void DrawInstancedArrayToTexture(RenderTexture rt, Material mat, int areaSize)
    {
        if (mat == null || rt == null) return;
        Graphics.SetRenderTarget(rt);
        RenderTexture.active = rt;
        mat.SetPass(0);
        GL.PushMatrix();
        GL.GetGPUProjectionMatrix(Matrix4x4.identity, true);

        GL.Clear(false, true, Color.black);

        Graphics.DrawMeshNow(GridVBO, Matrix4x4.identity);

        Graphics.SetRenderTarget(null);
        
        GL.PopMatrix();
    }

    void UpdateVBO(int areaSize)
    {
        int currentVertex = 0;
        for (int i = 0; i < currentDrawIdx; i++)
        {
            var size = (2 * drawPointsSize[i]) / areaSize;
            var halfSize = size / 2;

            vertices[currentVertex].x = drawPointsX[i] - halfSize;
            vertices[currentVertex].y = drawPointsY[i] - size * 0.2886751345948129f;

            vertices[currentVertex + 1].x = drawPointsX[i];
            vertices[currentVertex + 1].y = drawPointsY[i] + size * 0.5773502691896258f;

            vertices[currentVertex + 2].x = drawPointsX[i] + halfSize;
            vertices[currentVertex + 2].y = drawPointsY[i] - size * 0.2886751345948129f;

            //colors[currentVertex].r = colors[currentVertex + 1].r = colors[currentVertex + 2].r = drawPointsForce[i] * 0.5f + 0.5f;
            forces[currentVertex] = forces[currentVertex + 1] = forces[currentVertex + 2] = drawPointsForce[i] * Vector2.one;
            
            currentVertex += 3;
        }

        var count = vertices.Length;
        var zero = Vector2.zero;
        for (int i = currentVertex; i < count; i++)
        {
            vertices[i] = zero;
        }

        GridVBO.vertices = vertices;
        //GridVBO.colors = colors;
        GridVBO.SetUVs(1, forces);
        currentDrawIdx = 0;
    }


    void CreateGridVBO(int trisCount)
    {
        GridVBO = new Mesh();
        vertices = new Vector3[trisCount * 3];
        forces = new List<Vector2>();
        //colors = new Color[vertices.Length];
        uv = new List<Vector3>();
        triangles = new int[vertices.Length];

        for (int i = 0; i < vertices.Length; i += 3)
        {
            var offset = (float)i / vertices.Length;

            vertices[i] = new Vector3(offset, offset);
            vertices[i + 1] = new Vector3(1.0f / trisCount + offset, offset);
            vertices[i + 2] = new Vector3(offset, 1.0f / trisCount + offset);

            uv.Add(new Vector3(1, 0, 0));
            uv.Add(new Vector3(0, 1, 0));
            uv.Add(new Vector3(0, 0, 1));

            forces.Add(Vector2.zero);
            forces.Add(Vector2.zero);
            forces.Add(Vector2.zero);

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;
        }

        GridVBO.vertices = vertices;
        //GridVBO.colors = colors;
        GridVBO.triangles = triangles;
        GridVBO.SetUVs(0, uv);

        GridVBO.MarkDynamic();
    }

}
