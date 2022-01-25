using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;

class KW_DepthCopyRenderPass : ScriptableRenderPass
{
    float resolutionScale;

    string profilerTag;

    KW_PyramidBlur pyramidBlurMask;

    RenderTextureTemp depthRT;
    RenderTextureTemp depthRT_blured;

    private int KW_CameraDepthBeforeZWriteBlured_ID = Shader.PropertyToID("_CameraDepthTextureBeforeWaterZWrite_Blured");

    public KW_DepthCopyRenderPass(string profilerTag,
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

        depthRT = new RenderTextureTemp(cmd, "KW_DepthCopyRenderPass.depthRT", width, height, 0, RenderTextureFormat.RFloat);
        depthRT_blured = new RenderTextureTemp(cmd, "KW_DepthCopyRenderPass.depthRT_blured", width, height, 0, RenderTextureFormat.RFloat);

        if (pyramidBlurMask == null) pyramidBlurMask = new KW_PyramidBlur();
    }
  

    public void UpdateParams()
    {
        resolutionScale = 0.25f;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();

        ConfigureTarget(depthRT.identifier);
        var cameraDepthRT = new RenderTargetIdentifier("_CameraDepthTexture");
        cmd.Blit(cameraDepthRT, depthRT.identifier);
        
        pyramidBlurMask.ComputeBlurPyramid(3.0f, depthRT, depthRT_blured, cmd);
        cmd.SetGlobalTexture(KW_CameraDepthBeforeZWriteBlured_ID, depthRT_blured.identifier);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    // called after Execute, use it to clean up anything allocated in Configure
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(depthRT.id);
        cmd.ReleaseTemporaryRT(depthRT_blured.id);
    }

    public void Release()
    {
        if (pyramidBlurMask != null) pyramidBlurMask.Release();
    }
}
