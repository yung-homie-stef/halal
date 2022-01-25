using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;

public class KW_OffscreenRenderingRenderPass : ScriptableRenderPass
{
    string profilerTag;

    public RenderTextureTemp waterRT;
    private Material sceneCombineMaterial;

    private const string WaterDepthShaderName = "Hidden/KriptoFX/Water/ScreenSpaceWaterMeshCombine";

    public KW_OffscreenRenderingRenderPass(string profilerTag,
      RenderPassEvent renderPassEvent)
    {
        this.profilerTag = profilerTag;
        this.renderPassEvent = renderPassEvent;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        var width = (int)(cameraTextureDescriptor.width * water.OffscreenRenderingResolution);
        var height = (int)(cameraTextureDescriptor.height * water.OffscreenRenderingResolution);

        waterRT = new RenderTextureTemp(cmd, "KW_OffscreenRenderingRenderPass._WaterOffscreenRT", width, height, 16, RenderTextureFormat.ARGBHalf, true, FilterMode.Bilinear, false, false, TextureDimension.Tex2D, (int)water.OffscreenRenderingAA);
       
        if (sceneCombineMaterial == null) sceneCombineMaterial = KW_Extensions.CreateMaterial(WaterDepthShaderName);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();

        var water = KW_WaterDynamicScripts.GetCurrentWater();

        CoreUtils.SetRenderTarget(cmd, waterRT.identifier, ClearFlag.All);

        cmd.DrawMesh(water.currentWaterMesh, water.waterMeshGO.transform.localToWorldMatrix, water.waterMaterial);

        cmd.SetGlobalTexture("KW_ScreenSpaceWater", waterRT.identifier);
        Blit(cmd, waterRT.identifier, renderingData.cameraData.renderer.cameraColorTarget, sceneCombineMaterial);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(waterRT.id);
    }


    public void Release()
    {
        KW_Extensions.SafeDestroy(sceneCombineMaterial);
    }
}
