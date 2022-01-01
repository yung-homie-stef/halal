using UnityEngine;
using UnityEngine.Rendering.Universal;

public class KWS_WaterSystemRendererFeature : ScriptableRendererFeature
{
    KW_MaskDepthNormalRenderPass maskDepthPass;
    KW_DepthCopyRenderPass depthCopyPass;
    KW_VolumetricLightingRenderPass volumeLightingPass;
    KW_CausticDecalRenderPass causticDecalRenderPass;
    KW_ScreenSpaceReflectionRenderPass screenSpaceReflectionPass;
    KW_OffscreenRenderingRenderPass offscreenRenderingRenderPass;
    KW_UnderwaterRenderPass underwaterRenderPass;
    KW_DrawToDepthRenderPass drawToDepthRenderPass;

    public override void Create()
    {
        maskDepthPass = new KW_MaskDepthNormalRenderPass("Water.MaskDepthNormalPass", RenderPassEvent.BeforeRenderingSkybox);
        depthCopyPass = new KW_DepthCopyRenderPass("Water.DepthCopyPass", RenderPassEvent.BeforeRenderingSkybox);
        volumeLightingPass = new KW_VolumetricLightingRenderPass("Water.VolumetricLighting", RenderPassEvent.BeforeRenderingSkybox);
        causticDecalRenderPass = new KW_CausticDecalRenderPass("Water.Caustic", RenderPassEvent.BeforeRenderingSkybox);
        screenSpaceReflectionPass = new KW_ScreenSpaceReflectionRenderPass("Water.ScreenSpaceReflection", RenderPassEvent.BeforeRenderingTransparents);
        offscreenRenderingRenderPass = new KW_OffscreenRenderingRenderPass("Water.OffscreenMeshRendering", RenderPassEvent.AfterRenderingTransparents);
        underwaterRenderPass = new KW_UnderwaterRenderPass("Water.UnderwaterPass", RenderPassEvent.AfterRenderingTransparents);
        drawToDepthRenderPass = new KW_DrawToDepthRenderPass("Water.DrawToDepthPostFX", RenderPassEvent.BeforeRenderingPostProcessing);
    }

    // called every frame once per camera
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        var currentCamera = renderingData.cameraData.camera;
       
        if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection || currentCamera.name.Contains("Water") 
            || water == null || water.currentWaterMesh == null || water.waterMeshGO == null)
        {
            return;
        }

        if (water.UseVolumetricLight || water.UseCausticEffect || water.UseUnderwaterEffect)
        {
            maskDepthPass.UpdateParams(currentCamera);
            renderer.EnqueuePass(maskDepthPass);
        }
        
        depthCopyPass.UpdateParams();
        renderer.EnqueuePass(depthCopyPass);

        if (water.ReflectionMode == WaterSystem.ReflectionModeEnum.ScreenSpaceReflection)
        {
            screenSpaceReflectionPass.UpdateParams();
            renderer.EnqueuePass(screenSpaceReflectionPass);
        }

        if (water.UseVolumetricLight)
        {
            volumeLightingPass.UpdateParams();
            renderer.EnqueuePass(volumeLightingPass);
        }
        
        if(water.UseCausticEffect)
        {
            causticDecalRenderPass.UpdateParams();
            renderer.EnqueuePass(causticDecalRenderPass);
        }

        if(water.OffscreenRendering)
        {
            renderer.EnqueuePass(offscreenRenderingRenderPass);
        }

        if (water.UseUnderwaterEffect)
        {
            underwaterRenderPass.UpdateParams();
            renderer.EnqueuePass(underwaterRenderPass);
        }

        if(water.DrawToPosteffectsDepth && renderingData.cameraData.cameraType != CameraType.SceneView)
        {
            drawToDepthRenderPass.UpdateParams();
            renderer.EnqueuePass(drawToDepthRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        maskDepthPass.Release();
        depthCopyPass.Release();
        screenSpaceReflectionPass.Release();
        volumeLightingPass.Release();
        underwaterRenderPass.Release();
    }
}
