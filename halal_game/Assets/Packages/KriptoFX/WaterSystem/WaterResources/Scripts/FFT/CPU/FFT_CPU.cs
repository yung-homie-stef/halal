using System;
using System.Threading;
using UnityEngine;

public class FFT_CPU : MonoBehaviour
{
    public bool IsDetailed;
    public SizeSetting Size = SizeSetting.Size_64;

    [Range(0, 1)] public float              WindDirection = 0.3f;
    [Range(.2f, 10)] public float           WindSpeed = 5;
    [Range(0.25f, 1.5f)] public float       Choppines = 0.85f;
    [Range(1, 100)] public float             WaterScale = 10;
    [Range(0, 2)] public float              TimeScale = 1;

    public Texture2D                        DisplaceTexture;
    public RenderTexture                    NormalTexture;
    public Material                         WaterMaterial;

    const float                             DomainSize = 20;

    float                                   prevWindDirection;
    float                                   prevWindSpeed;
    int                                     prevSize;


    FFT_CPU_Spectrum                        spectrumCpu;
    OutputFFTData                           outputFftData;
    FFT_CPU_Simulation                      fftCpu1, fftCpu2, fftCpu3;
    Vector2[]                               butterflyRawData;
    Material                                normalComputeMaterial;
    Thread                                  t0, t1, t2, t3;
    float                                   currentTime;
    bool                                    canUpdate = true;
    bool                                    isDoneComputeFFT_1, isDoneComputeFFT_2, isDoneComputeFFT_3;
    AutoResetEvent[]                        startHandle;
    
    void InitializeResources()
    {
        isDoneComputeFFT_1 = false;
        isDoneComputeFFT_2 = false;
        isDoneComputeFFT_3 = false;
        canUpdate = true;
        outputFftData = new OutputFFTData();
      
        fftCpu1 = new FFT_CPU_Simulation((int)Size, outputFftData);
        fftCpu2 = new FFT_CPU_Simulation((int)Size, outputFftData);
        fftCpu3 = new FFT_CPU_Simulation((int)Size, outputFftData);

        prevSize = (int) Size;
        prevWindDirection = WindDirection;
        prevWindSpeed = WindSpeed;

        normalComputeMaterial = new Material(Shader.Find("KriptoFX/Water/ComputeNormal"));
        if (NormalTexture != null) NormalTexture.Release();
        NormalTexture = new RenderTexture((int)Size, (int)Size, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        NormalTexture.filterMode = FilterMode.Bilinear;
        NormalTexture.wrapMode = TextureWrapMode.Repeat;

        if (DisplaceTexture != null) DestroyImmediate(DisplaceTexture);
        DisplaceTexture = new Texture2D((int)Size, (int)Size, TextureFormat.RGBAHalf, false, true);

        InitializeButterfly((int)Size);

        spectrumCpu = new FFT_CPU_Spectrum((int)Size);
        spectrumCpu.InitializeSpectrum(DomainSize, WindSpeed, WindDirection, (int)Size);

        if (startHandle == null)
        {
            startHandle = new AutoResetEvent[4];
            startHandle[0] = new AutoResetEvent(true);
            startHandle[1] = new AutoResetEvent(false);
            startHandle[2] = new AutoResetEvent(false);
            startHandle[3] = new AutoResetEvent(false);
        }
    }

    void InitializeButterfly(int size)
    {
        var log2Size = Mathf.RoundToInt(Mathf.Log(size, 2));
        butterflyRawData = new Vector2[size * log2Size];

        int offset = 1, numIterations = size >> 1;
        for (int rowIndex = 0; rowIndex < log2Size; rowIndex++)
        {
            int rowOffset = rowIndex * size;
            {
                int start = 0, end = 2 * offset;
                for (int iteration = 0; iteration < numIterations; iteration++)
                {
                    var bigK = 0.0f;
                    for (int K = start; K < end; K += 2)
                    {
                        var phase = 2.0f * Mathf.PI * bigK * numIterations / size;
                        var cos = Mathf.Cos(phase);
                        var sin = Mathf.Sin(phase);

                        butterflyRawData[rowOffset + K / 2] = new Vector2(cos, -sin);
                        butterflyRawData[rowOffset + K / 2 + offset] = new Vector2(-cos, sin);

                        bigK += 1.0f;
                    }
                    start += 4 * offset;
                    end = start + 2 * offset;
                }
            }
            numIterations >>= 1;
            offset <<= 1;
        }
    }

    void StartThreads()
    {
        if (startHandle != null)
        {
            startHandle[0].Set();
        }

        if (t0 == null)
        {
            t0 = new Thread(thread0);
            t0.Start();
        }

        if (t1 == null)
        {
            t1 = new Thread(thread1);
            t1.Start();
        }

        if (t2 == null)
        {
            t2 = new Thread(thread2);
            t2.Start();
        }

        if (t3 == null)
        {
            t3 = new Thread(thread3);
            t3.Start();
        }
    }

    void RestartCompute()
    {
        ReleaseAll();
        InitializeResources();
        StartThreads();
    }

    void ReleaseAll()
    {
        canUpdate = false;

        if (t0 != null) t0.Abort();
        t0 = null;
        if (t1 != null) t1.Abort();
        t1 = null;
        if (t2 != null) t2.Abort();
        t2 = null;
        if (t3 != null) t3.Abort();
        t3 = null;

        if (DisplaceTexture != null) DestroyImmediate(DisplaceTexture);
        if (NormalTexture != null) NormalTexture.Release();
        if (IsDetailed) Shader.DisableKeyword("KW_DETAIL_FFT");

    }

    #region Threads

    void thread0()
    {
        while (canUpdate)
        {
            startHandle[0].WaitOne();
            isDoneComputeFFT_1 = false;
            isDoneComputeFFT_2 = false;
            isDoneComputeFFT_3 = false;

            
            spectrumCpu.GetUpdatedSpectrum(currentTime, (int)Size);

            startHandle[1].Set();
            startHandle[2].Set();
            startHandle[3].Set();
        }
    }

    void thread1()
    {
        while (canUpdate)
        {
            startHandle[1].WaitOne();

            fftCpu1.Compute(spectrumCpu.ResultDisplaceZ, butterflyRawData, 0, (int)Size);
            isDoneComputeFFT_1 = true;
        }
    }

    void thread2()
    {
        while (canUpdate)
        {
            startHandle[2].WaitOne();

            fftCpu2.Compute(spectrumCpu.ResultHeight, butterflyRawData, 1, (int)Size);
            isDoneComputeFFT_2 = true;
        }
    }

    void thread3()
    {
        while (canUpdate)
        {
            startHandle[3].WaitOne();

            fftCpu3.Compute(spectrumCpu.ResultDisplaceX, butterflyRawData, 2, (int)Size);
            isDoneComputeFFT_3 = true;
        }
    }

    #endregion

    void ReadThreadsData()
    {
        if (isDoneComputeFFT_1 && isDoneComputeFFT_2 && isDoneComputeFFT_3)
        {
            DisplaceTexture.SetPixels(outputFftData.OutputPixels);
            DisplaceTexture.Apply();

            normalComputeMaterial.SetTexture("_DispTex", DisplaceTexture);
            var sizeLog = Mathf.RoundToInt(Mathf.Log((int)Size, 2)) - 4;
            normalComputeMaterial.SetFloat("_SizeLog", sizeLog);
            normalComputeMaterial.SetFloat("_Choppines", Choppines);
            normalComputeMaterial.SetFloat("_WindSpeed", WindSpeed);
            NormalTexture.DiscardContents();
            var temp = RenderTexture.active;
            if (!Application.isPlaying) RenderTexture.active = null;
            Graphics.Blit(null, NormalTexture, normalComputeMaterial);
            if (!Application.isPlaying) RenderTexture.active = temp;

            startHandle[0].Set();
        }
    }

    void UpdateFFT()
    {
        if (prevSize != (int)Size)
        {
            prevSize = (int)Size;
            RestartCompute();
        }
        else if (Mathf.Abs(prevWindDirection - WindDirection) > 0.001f || Mathf.Abs(prevWindSpeed - WindSpeed) > 0.01f)
        {
            prevWindDirection = WindDirection;
            prevWindSpeed = WindSpeed;
            spectrumCpu.InitializeSpectrum(DomainSize, WindSpeed, WindDirection, (int)Size);
        }

        currentTime += Time.deltaTime * Mathf.Lerp(TimeScale, TimeScale * 0.5f, WaterScale / 100);
        ReadThreadsData();

        Vector3 fftDisplaceScale = new Vector3((WaterScale * Choppines) / 20, 1, (WaterScale * Choppines) / 20);
        if (!IsDetailed)
        {
            if (WaterMaterial == null) Shader.SetGlobalTexture("KW_DispTex", DisplaceTexture); else WaterMaterial.SetTexture("KW_DispTex", DisplaceTexture);
            if (WaterMaterial == null) Shader.SetGlobalTexture("KW_NormTex", NormalTexture); else WaterMaterial.SetTexture("KW_NormTex", NormalTexture);
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_WindSpeed", WindSpeed); else WaterMaterial.SetFloat("KW_WindSpeed", WindSpeed);
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_Choppines", Choppines); else WaterMaterial.SetFloat("KW_Choppines", Choppines);
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_FFTScale", WaterScale); else WaterMaterial.SetFloat("KW_FFTScale", WaterScale);
            if (WaterMaterial == null) Shader.SetGlobalVector("KW_DisplaceScale", fftDisplaceScale); else WaterMaterial.SetVector("KW_DisplaceScale", fftDisplaceScale);
            var lodSize = Mathf.RoundToInt(Mathf.Log((int)Size, 2)) - 5;
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_NormalLod", lodSize); else WaterMaterial.SetFloat("KW_NormalLod", lodSize);
        }
        else
        {
            if (WaterMaterial == null) Shader.SetGlobalTexture("KW_DispTexDetail", DisplaceTexture); else WaterMaterial.SetTexture("KW_DispTexDetail", DisplaceTexture);
            if (WaterMaterial == null) Shader.SetGlobalTexture("KW_NormTexDetail", NormalTexture); else WaterMaterial.SetTexture("KW_NormTexDetail", NormalTexture);
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_WindSpeedDetail", WindSpeed); else WaterMaterial.SetFloat("KW_WindSpeedDetail", WindSpeed);
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_ChoppinesDetail", Choppines); else WaterMaterial.SetFloat("KW_ChoppinesDetail", Choppines);
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_FFTScaleDetail", WaterScale); else WaterMaterial.SetFloat("KW_FFTScaleDetail", WaterScale);
            if (WaterMaterial == null) Shader.SetGlobalVector("KW_DisplaceScaleDetail", fftDisplaceScale); else WaterMaterial.SetVector("KW_DisplaceScaleDetail", fftDisplaceScale);
            var lodSize = Mathf.RoundToInt(Mathf.Log((int)Size, 2)) - 5;
            if (WaterMaterial == null) Shader.SetGlobalFloat("KW_NormalLodDetail", lodSize); else WaterMaterial.SetFloat("KW_NormalLodDetail", lodSize);
            Shader.EnableKeyword("KW_DETAIL_FFT");
        }
    }

    public enum SizeSetting
    {
        Size_32 = 32,
        Size_64 = 64,
        Size_128 = 128,
    }

    public class OutputFFTData
    {
        public Color[] OutputPixels;
    }

    //-------------------------------------- Unity Methods -------------------------------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        RestartCompute();
    }

    void OnDisable()
    {
        ReleaseAll();
    }

    void OnDestroy()
    {
        ReleaseAll();
    }

    void Update()
    {
        UpdateFFT();
    }
}