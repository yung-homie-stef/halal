using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;

public class KW_VolumetricLightingRenderPass : ScriptableRenderPass
{
    float resolutionScale;
    string profilerTag;

    private Vector4[] frustum = new Vector4[4];
    private Vector4[] uv_World = new Vector4[4];

    public RenderTextureTemp volumeLightRT;
    public RenderTextureTemp volumeLightRT_blured;

    private Material volumeLightMat;
    private KW_PyramidBlur pyramidBlur = new KW_PyramidBlur();
   
    private const string VolumeShaderName = "Hidden/KriptoFX/Water/KW_WaterVolumetricLighting_URP";
    private int VolumeLightRT_ID = Shader.PropertyToID("KW_VolumetricLight");


    bool IsSupportedPointLightsShadows()
    {
        var version = Application.unityVersion;
        if (version.Contains("2020.3")) return false;
        else return true;
    }

    Vector4 ComputeMieVector(float MieG)
    {
        return new Vector4(1 - (MieG * MieG), 1 + (MieG * MieG), 2 * MieG, 1.0f / (4.0f * Mathf.PI));
    }

    public KW_VolumetricLightingRenderPass(string profilerTag,
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

        volumeLightRT = new RenderTextureTemp(cmd, "KW_VolumetricLightingRenderPass._volumeLightRT", width, height, 0, RenderTextureFormat.ARGBHalf);
        volumeLightRT_blured = new RenderTextureTemp(cmd, "KW_VolumetricLightingRenderPass._volumeLightRT_blured", width, height, 0, RenderTextureFormat.ARGBHalf);

    }

    public void UpdateParams()
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        resolutionScale = water.VolumetricLightResolutionScale;

        if (volumeLightMat == null) volumeLightMat = KW_Extensions.CreateMaterial(VolumeShaderName);
        if (!water.waterSharedMaterials.Contains(volumeLightMat)) water.waterSharedMaterials.Add(volumeLightMat);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();

        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();
        

        UpdateMaterialParams(water, renderingData.cameraData.camera, volumeLightRT.descriptor.width, volumeLightRT.descriptor.height, renderingData.lightData.additionalLightsCount);
        ConfigureTarget(volumeLightRT.identifier);
        cmd.Blit(null, volumeLightRT.identifier, volumeLightMat);

        if (pyramidBlur == null) pyramidBlur = new KW_PyramidBlur();
        pyramidBlur.ComputeBlurPyramid(water.VolumetricLightBlurRadius, volumeLightRT, volumeLightRT_blured, cmd);
        cmd.SetGlobalTexture(VolumeLightRT_ID, volumeLightRT_blured.identifier);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    private void UpdateMaterialParams(WaterSystem water, Camera cam, int width, int height, int lightsCount)
    {
        var anisoMie = ComputeMieVector(0.05f);
       
        var volumeLightMaxDist = Mathf.Max(0.3f, water.Transparent * 3);
        volumeLightMaxDist = Mathf.Min(40, volumeLightMaxDist);
        float volumeLightFade = Mathf.Clamp01(Mathf.Exp(-1 * (water.transform.position.y - cam.transform.position.y) / water.Transparent));

        volumeLightMat.SetFloat("KW_lightsCount", lightsCount);
        volumeLightMat.SetVector("KW_LightAnisotropy", anisoMie);
        volumeLightMat.SetFloat("KW_VolumeDepthFade", volumeLightFade);
        volumeLightMat.SetFloat("KW_VolumeLightMaxDistance", volumeLightMaxDist);
        volumeLightMat.SetFloat("KW_RayMarchSteps", water.VolumetricLightIteration);
        volumeLightMat.SetVector("KW_DitherSceenScale", new Vector2(width, height));
        
        volumeLightMat.SetKeyword("KW_POINT_SHADOWS_SUPPORTED", IsSupportedPointLightsShadows());


        frustum[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.farClipPlane));
        frustum[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.farClipPlane));
        frustum[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.farClipPlane));
        frustum[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.farClipPlane));
        volumeLightMat.SetVectorArray("KW_Frustum", frustum);

        uv_World[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        uv_World[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        uv_World[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
        uv_World[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        volumeLightMat.SetVectorArray("KW_UV_World", uv_World);
    }

    // called after Execute, use it to clean up anything allocated in Configure
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(volumeLightRT.id);
        cmd.ReleaseTemporaryRT(volumeLightRT_blured.id);
    }

    public void Release()
    {
        if (pyramidBlur != null) pyramidBlur.Release();
        KW_Extensions.SafeDestroy(volumeLightMat);
    }
}
