using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;

class KW_MaskDepthNormalRenderPass : ScriptableRenderPass
{
    string profilerTag;
    private float resolutionScale;

    KW_PyramidBlur pyramidBlurMask;
    private Material maskDepthNormalMaterial;

    RenderTextureTemp waterMaskRT;
    RenderTextureTemp waterMaskRT_Blured;
    RenderTextureTemp waterDepthRT;

    private const string maskDepthNormal_ShaderName = "Hidden/KriptoFX/Water/KW_MaskDepthNormalPass";
    private const int DepthMaskTextureHeightLimit = 540; //fullHD * 0.5 enough even for 4k

    private int KW_WaterMaskScatterNormals_ID = Shader.PropertyToID("KW_WaterMaskScatterNormals");
    private int KW_WaterDepth_ID = Shader.PropertyToID("KW_WaterDepth");
    private int KW_WaterMaskScatterNormals_Blured_ID = Shader.PropertyToID("KW_WaterMaskScatterNormals_Blured");


    public KW_MaskDepthNormalRenderPass(string profilerTag,
      RenderPassEvent renderPassEvent)
    {
        this.profilerTag = profilerTag;
        this.renderPassEvent = renderPassEvent;
    }

    // called each frame before Execute, use it to set up things the pass will need
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var width = (int)(cameraTextureDescriptor.width * resolutionScale);
        var height = (int)(cameraTextureDescriptor.height * resolutionScale);
       
        waterMaskRT = new RenderTextureTemp(cmd, "KW_MaskDepthNormalRenderPass._waterMaskRT", width, height, 0, RenderTextureFormat.ARGBHalf);
        waterMaskRT_Blured = new RenderTextureTemp(cmd, "KW_MaskDepthNormalRenderPass._waterMaskRT_Blured", width, height, 0, RenderTextureFormat.ARGBHalf);
        waterDepthRT = new RenderTextureTemp(cmd, "KW_MaskDepthNormalRenderPass._waterDepthRT", width, height, 24, RenderTextureFormat.Depth);
      
        if (pyramidBlurMask == null) pyramidBlurMask = new KW_PyramidBlur();
    }
   
    public void UpdateParams(Camera currentCamera)
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        resolutionScale = (water.UseUnderwaterEffect && water.UseHighQualityUnderwater) ? 0.5f : 0.25f;
        if (currentCamera.scaledPixelHeight  * resolutionScale > DepthMaskTextureHeightLimit)
        {
            var newRelativeScale = DepthMaskTextureHeightLimit / (currentCamera.scaledPixelHeight * resolutionScale);
            resolutionScale *= newRelativeScale;
        }
       

        if (maskDepthNormalMaterial == null) maskDepthNormalMaterial = KW_Extensions.CreateMaterial(maskDepthNormal_ShaderName);
        if (!water.waterSharedMaterials.Contains(maskDepthNormalMaterial)) water.waterSharedMaterials.Add(maskDepthNormalMaterial);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();
       
        ConfigureTarget(waterMaskRT.identifier, waterDepthRT.identifier);
        ConfigureClear(ClearFlag.All, Color.black);

        var water = KW_WaterDynamicScripts.GetCurrentWater();

        var shaderPass = water.UseTesselation && SystemInfo.graphicsShaderLevel >= 46 ? 0 : 1;
        cmd.DrawMesh(water.currentWaterMesh, water.waterMeshGO.transform.localToWorldMatrix, maskDepthNormalMaterial, 0, shaderPass);

        cmd.SetGlobalTexture(KW_WaterMaskScatterNormals_ID, waterMaskRT.identifier);
        cmd.SetGlobalTexture(KW_WaterDepth_ID, waterDepthRT.identifier);

        pyramidBlurMask.ComputeBlurPyramid(3.0f, waterMaskRT, waterMaskRT_Blured, cmd);
        cmd.SetGlobalTexture(KW_WaterMaskScatterNormals_Blured_ID, waterMaskRT_Blured.identifier);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    // called after Execute, use it to clean up anything allocated in Configure
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(waterMaskRT.id);
        cmd.ReleaseTemporaryRT(waterMaskRT_Blured.id);
        cmd.ReleaseTemporaryRT(waterDepthRT.id);
    }

    public void Release()
    {
        if (pyramidBlurMask != null) pyramidBlurMask.Release();
        KW_Extensions.SafeDestroy(maskDepthNormalMaterial);
    }
}
