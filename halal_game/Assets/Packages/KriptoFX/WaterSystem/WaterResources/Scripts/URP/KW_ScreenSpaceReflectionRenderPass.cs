using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;


public class KW_ScreenSpaceReflectionRenderPass : ScriptableRenderPass
{
    string profilerTag;
    float resolutionScale;
    bool debugNonDx11Features = false;
   

    int currentWidth;
    int currentHeight;

    private const string hashProjectionShaderName = "Hidden/KriptoFX/Water/SSPR_Projection";
    private const string hashReflectionShaderName = "Hidden/KriptoFX/Water/SSPR_Reflection";

    private Material hashProjectionMat;
    private Material hashReflectionMat;
    private RenderTextureTemp reflectionRT;
    private RenderTextureTemp reflectionHash;
   // private RenderTextureTemp reflectionHashMobile;

    ComputeShader cs;


    const int SHADER_NUMTHREAD_X = 8; //must match compute shader's [numthread(x)]
    const int SHADER_NUMTHREAD_Y = 8; //must match compute shader's [numthread(y)]

    public KW_ScreenSpaceReflectionRenderPass(string profilerTag,
      RenderPassEvent renderPassEvent)
    {
        this.profilerTag = profilerTag;
        this.renderPassEvent = renderPassEvent;
    }

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (hashProjectionMat == null) hashProjectionMat = KW_Extensions.CreateMaterial(hashProjectionShaderName);
        if (!waterShaderMaterials.Contains(hashProjectionMat)) waterShaderMaterials.Add(hashProjectionMat);

        if (hashReflectionMat == null) hashReflectionMat = KW_Extensions.CreateMaterial(hashReflectionShaderName);
        if (!waterShaderMaterials.Contains(hashReflectionMat)) waterShaderMaterials.Add(hashReflectionMat);
    }

    // called each frame before Execute, use it to set up things the pass will need
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        currentWidth = GetRTWidth(cameraTextureDescriptor.width, cameraTextureDescriptor.height);
        currentHeight = GetRTHeight(cameraTextureDescriptor.height);

        var canUseMipMap = Application.isPlaying;
        reflectionRT = new RenderTextureTemp(cmd, "KW_ScreenSpaceReflectionRenderPass._reflectionRT", currentWidth, currentHeight, 0, RenderTextureFormat.ARGBHalf, true, FilterMode.Bilinear, true, canUseMipMap); //bug, mipmaping is flickering in editor
        reflectionHash = new RenderTextureTemp(cmd, "KW_ScreenSpaceReflectionRenderPass._reflectionHash", currentWidth, currentHeight, 0, IsSupportDx11Features() ? RenderTextureFormat.RInt : RenderTextureFormat.RFloat, true, FilterMode.Bilinear, true);
        
        //reflectionHashMobile = new RenderTextureTemp(cmd, "_reflectionHashMobile", width, height, 0, RenderTextureFormat.RFloat, true, true);

        if (cs == null) cs = (ComputeShader)Resources.Load(@"URP/SSPR_Projection_URP");
    }


    public void UpdateParams()
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        resolutionScale = water.ReflectionTextureScale;
    }

    bool IsSupportDx11Features()
    {
        if (debugNonDx11Features) return false;

        if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt))
            return false;

        //tested Metal(even on a Mac) can't use InterlockedMin().
        //so if metal, use mobile path
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
            return false;
#if UNITY_EDITOR
        //PC(DirectX) can use RenderTextureFormat.RInt + InterlockedMin() without any problem, use Non-Mobile path.
        //Non-Mobile path will NOT produce any flickering
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12)
            return true;
#elif UNITY_ANDROID
               return false;
#endif
        return false;
    }

    int GetRTHeight(int height)
    {
        return Mathf.CeilToInt((resolutionScale * height) / (float)SHADER_NUMTHREAD_Y) * SHADER_NUMTHREAD_Y;
    }
    int GetRTWidth(int width, int height)
    {
        float aspect = (float)width / height;
        return Mathf.CeilToInt(GetRTHeight(height) * aspect / (float)SHADER_NUMTHREAD_X) * SHADER_NUMTHREAD_X;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();

        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();

        int dispatchThreadGroupXCount = currentWidth / SHADER_NUMTHREAD_X; //divide by shader's numthreads.x
        int dispatchThreadGroupYCount = currentHeight / SHADER_NUMTHREAD_Y; //divide by shader's numthreads.y
        int dispatchThreadGroupZCount = 1; //divide by shader's numthreads.z
      
        cmd.SetComputeVectorParam(cs, Shader.PropertyToID("_RTSize"), new Vector2(currentWidth, currentHeight));
        cmd.SetComputeFloatParam(cs, Shader.PropertyToID("_HorizontalPlaneHeightWS"), water.transform.position.y);
        cmd.SetComputeFloatParam(cs, Shader.PropertyToID("_DepthHolesFillDistance"), water.SSR_DepthHolesFillDistance);
      
        var camera = renderingData.cameraData.camera;
       
        Matrix4x4 VP = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix;
        cmd.SetComputeMatrixParam(cs, "_VPMatrix", VP);
        
        if (IsSupportDx11Features())
        {
           
            ConfigureTarget(reflectionRT.identifier);
           
            int kernel_NonMobilePathClear = cs.FindKernel("NonMobilePathClear");
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathClear, "HashRT", reflectionHash.identifier);
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathClear, "ColorRT", reflectionRT.identifier);
            cmd.DispatchCompute(cs, kernel_NonMobilePathClear, dispatchThreadGroupXCount, dispatchThreadGroupYCount, dispatchThreadGroupZCount);

            int kernel_NonMobilePathRenderHashRT = cs.FindKernel("NonMobilePathRenderHashRT");
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathRenderHashRT, "HashRT", reflectionHash.identifier);
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathRenderHashRT, "ColorRT", reflectionRT.identifier);
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathRenderHashRT, "_CameraDepthTexture", new RenderTargetIdentifier("_CameraDepthTexture"));

            cmd.DispatchCompute(cs, kernel_NonMobilePathRenderHashRT, dispatchThreadGroupXCount, dispatchThreadGroupYCount, dispatchThreadGroupZCount);

            int kernel_NonMobilePathResolveColorRT = cs.FindKernel("NonMobilePathResolveColorRT");
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathResolveColorRT, "_CameraOpaqueTexture", new RenderTargetIdentifier("_CameraOpaqueTexture"));
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathResolveColorRT, "ColorRT", reflectionRT.identifier);
            cmd.SetComputeTextureParam(cs, kernel_NonMobilePathResolveColorRT, "HashRT", reflectionHash.identifier);
            cmd.DispatchCompute(cs, kernel_NonMobilePathResolveColorRT, dispatchThreadGroupXCount, dispatchThreadGroupYCount, dispatchThreadGroupZCount);
           
        }
        else
        {
            ConfigureTarget(reflectionRT.identifier);
            int kernel_MobilePathSinglePassColorRTDirectResolve = cs.FindKernel("MobilePathSinglePassColorRTDirectResolve");
            cmd.SetComputeTextureParam(cs, kernel_MobilePathSinglePassColorRTDirectResolve, "ColorRT", reflectionRT.identifier);
            cmd.SetComputeTextureParam(cs, kernel_MobilePathSinglePassColorRTDirectResolve, "PosWSyRT", reflectionHash.identifier);
            cmd.SetComputeTextureParam(cs, kernel_MobilePathSinglePassColorRTDirectResolve, "_CameraOpaqueTexture", new RenderTargetIdentifier("_CameraOpaqueTexture"));
            cmd.SetComputeTextureParam(cs, kernel_MobilePathSinglePassColorRTDirectResolve, "_CameraDepthTexture", new RenderTargetIdentifier("_CameraDepthTexture"));
            cmd.DispatchCompute(cs, kernel_MobilePathSinglePassColorRTDirectResolve, dispatchThreadGroupXCount, dispatchThreadGroupYCount, dispatchThreadGroupZCount);

            int kernel_FillHoles = cs.FindKernel("FillHoles");
            cmd.SetComputeTextureParam(cs, kernel_FillHoles, "ColorRT", reflectionRT.identifier);
            cmd.SetComputeTextureParam(cs, kernel_FillHoles, "PackedDataRT", reflectionHash.identifier);
            cmd.DispatchCompute(cs, kernel_FillHoles, Mathf.CeilToInt(dispatchThreadGroupXCount / 2f), Mathf.CeilToInt(dispatchThreadGroupYCount / 2f), dispatchThreadGroupZCount);
           
        }
        cmd.SetGlobalTexture("KW_ScreenSpaceReflectionTex", reflectionRT.identifier);
        
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(reflectionRT.id);
        cmd.ReleaseTemporaryRT(reflectionHash.id);
    }

    public void Release()
    {
        KW_Extensions.SafeDestroy(hashProjectionMat, hashReflectionMat);
    }
}
