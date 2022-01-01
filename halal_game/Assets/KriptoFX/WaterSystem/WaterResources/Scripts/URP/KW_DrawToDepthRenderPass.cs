using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;

class KW_DrawToDepthRenderPass : ScriptableRenderPass
{
    float resolutionScale;

    string profilerTag;


   //RenderTextureTemp depthRT;
    //RenderTextureTemp depthRT_blured;

    private Material blitToDepthMaterial;

    private const string BlitToDepthShaderName = "Hidden/KriptoFX/Water/KW_BlitToDepthTexture_URP";

    public KW_DrawToDepthRenderPass(string profilerTag,
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

        //depthRT = new RenderTextureTemp(cmd, "_CameraDepthTextureBeforeWaterRendering", width, height, 0, RenderTextureFormat.RFloat);
    }
  

    public void UpdateParams()
    {
        resolutionScale = 0.5f;
        if (blitToDepthMaterial == null) blitToDepthMaterial = KW_Extensions.CreateMaterial(BlitToDepthShaderName);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();

        var depthRT_source = new RenderTargetIdentifier("_CameraDepthAttachment");
        var depthRT_Target = new RenderTargetIdentifier("_CameraDepthTexture");
      
        Blit(cmd, depthRT_source, depthRT_Target, blitToDepthMaterial);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    // called after Execute, use it to clean up anything allocated in Configure
    public override void FrameCleanup(CommandBuffer cmd)
    {
       // cmd.ReleaseTemporaryRT(depthRT.id);
        //cmd.ReleaseTemporaryRT(depthRT_blured.id);
    }

    public void Release()
    {
        KW_Extensions.SafeDestroy(blitToDepthMaterial);
    }
}
