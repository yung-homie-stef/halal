using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static KW_Extensions;

public class KW_CausticDecalRenderPass : ScriptableRenderPass
{
    string profilerTag;
    Mesh decalMesh;
    Material causticDecalMaterial;

    private const string CausticDecalShaderName = "Hidden/KriptoFX/Water/CausticDecal";

    public KW_CausticDecalRenderPass(string profilerTag,
      RenderPassEvent renderPassEvent)
    {
        this.profilerTag = profilerTag;
        this.renderPassEvent = renderPassEvent;
    }

    public void UpdateParams()
    {
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        if (causticDecalMaterial == null) causticDecalMaterial = KW_Extensions.CreateMaterial(CausticDecalShaderName);
        if (!water.waterSharedMaterials.Contains(causticDecalMaterial)) water.waterSharedMaterials.Add(causticDecalMaterial);
    }
   
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();
        
        var water = KW_WaterDynamicScripts.GetCurrentWater();
        var decalPos = renderingData.cameraData.camera.transform.position;
        decalPos.y = water.transform.position.y - 10; 
        
        var decalScale = KW_CausticRendering.LodSettings[water.CausticActiveLods - 1] * 2;
        var decalTRS = Matrix4x4.TRS(decalPos, Quaternion.identity, new Vector3(decalScale, 40, decalScale));
        UpdateMaterialParams(water);
        if (decalMesh == null) decalMesh = CoreUtils.CreateCubeMesh(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
        //if (decalMesh == null) GenerateDecalMesh();
        cmd.DrawMesh(decalMesh, decalTRS, causticDecalMaterial);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    void UpdateMaterialParams(WaterSystem water)
    {
        causticDecalMaterial.SetFloat("KW_CaustisStrength", water.CausticStrength); 
        var dispersionStrength = 1 - (Mathf.RoundToInt(Mathf.Log((int)water.FFT_SimulationSize, 2)) - 5) / 4.0f; // 0 - 4 => 1-0
        if (water.UseCausticDispersion && dispersionStrength > 0.1f)
        {
            causticDecalMaterial.EnableKeyword("USE_DISPERSION");
            dispersionStrength = Mathf.Lerp(dispersionStrength * 0.25f, dispersionStrength, water.CausticTextureSize / 1024f);
            causticDecalMaterial.SetFloat("KW_CausticDispersionStrength", dispersionStrength);
        }
        else causticDecalMaterial.DisableKeyword("USE_DISPERSION");
    }

    public void Release()
    {
        KW_Extensions.SafeDestroy(causticDecalMaterial);
    }
    
}
