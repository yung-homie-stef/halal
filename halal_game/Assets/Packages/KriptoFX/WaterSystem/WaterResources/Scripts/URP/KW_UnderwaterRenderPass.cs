using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;

class KW_UnderwaterRenderPass : ScriptableRenderPass
{
    float resolutionScale;
    string profilerTag;

    const string UnderwaterShaderName = "KriptoFX/Water30/Underwater";

    private Material underwaterMaterial;
    KW_PyramidBlur pyramidBlur;
    private RenderTextureTemp underwaterRT;
    private RenderTextureTemp underwaterRT_Blured;

    public KW_UnderwaterRenderPass(string profilerTag,
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
        
        underwaterRT = new RenderTextureTemp(cmd, "KW_UnderwaterRenderPass._UnderwaterRT", width, height, 0, RenderTextureFormat.ARGBHalf);
        underwaterRT_Blured = new RenderTextureTemp(cmd, "KW_UnderwaterRenderPass._UnderwaterRT_Blured", width, height, 0, RenderTextureFormat.ARGBHalf);
    }
  

    public void UpdateParams()
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        resolutionScale = water.UnderwaterResolutionScale;
        if (underwaterMaterial == null) underwaterMaterial = KW_Extensions.CreateMaterial(UnderwaterShaderName);
        if (!water.waterSharedMaterials.Contains(underwaterMaterial)) water.waterSharedMaterials.Add(underwaterMaterial);
        if (pyramidBlur == null) pyramidBlur = new KW_PyramidBlur();
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();

        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();

        if (water.UseUnderwaterBlur)
        {
            ConfigureTarget(underwaterRT.identifier);
            cmd.Blit(null, underwaterRT.identifier, underwaterMaterial, 0);
            pyramidBlur.ComputeBlurPyramid(water.UnderwaterBlurRadius, underwaterRT, underwaterRT_Blured, cmd);
            cmd.SetGlobalTexture("KW_UnderwaterPostFX_Blured", underwaterRT_Blured.identifier);
            Blit(cmd, underwaterRT_Blured.identifier, renderingData.cameraData.renderer.cameraColorTarget, underwaterMaterial, 1);
        }
        else
        {
            cmd.Blit(null, renderingData.cameraData.renderer.cameraColorTarget, underwaterMaterial, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    // called after Execute, use it to clean up anything allocated in Configure
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(underwaterRT.id);
        cmd.ReleaseTemporaryRT(underwaterRT_Blured.id);
    }

    public void Release()
    {
        if (pyramidBlur != null) pyramidBlur.Release();
        KW_Extensions.SafeDestroy(underwaterMaterial);
    }
}
