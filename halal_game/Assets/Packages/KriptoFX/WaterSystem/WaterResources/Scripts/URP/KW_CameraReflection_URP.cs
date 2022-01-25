using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KW_CameraReflection_URP : MonoBehaviour
{
    private GameObject reflCameraGO;
    private Camera reflectionCamera;
    public RenderTexture reflectionRT;
    public RenderTexture reflectionCubemapRT;
    public float m_ClipPlaneOffset = -0.05f;
    public float m_planeOffset = -0.05f;

    private float currentInterval = 0;
    private int sideIdx = 0;
    private int[] cubeMapSides = new int[]
    {
        0, 1, 2, 4, 5
    };

    bool requiredUpdateAllFaces = true;
    int waterCullingMask = ~(1 << 4);


    public void Release()
    {
        KW_Extensions.SafeDestroy(reflCameraGO);
        KW_Extensions.ReleaseRenderTextures(reflectionRT, reflectionCubemapRT);
        currentInterval = 0;
        requiredUpdateAllFaces = true;
        //Debug.Log("KW_PlanarReflection.Released ");
    }

    void OnDisable()
    {
        Release();
    }

    void InitializeReflectionTexture(int width, int height, bool useMip)
    {
        reflectionRT = KW_Extensions.ReinitializeRenderTexture(reflectionRT, width, height, 24, RenderTextureFormat.ARGBHalf, null, false, useMip);
    }

    void InitializeCubemapTexture(int width, int height)
    {
        reflectionCubemapRT = KW_Extensions.ReinitializeRenderTexture(reflectionCubemapRT, width, height, 0, RenderTextureFormat.ARGBHalf, null, false, false);
        reflectionCubemapRT.dimension = TextureDimension.Cube;
    }

    void CreateCamera()
    {
        reflCameraGO = new GameObject("WaterReflectionCamera");
        reflCameraGO.hideFlags = HideFlags.HideAndDontSave;
        reflCameraGO.transform.parent = transform;
        reflectionCamera = reflCameraGO.AddComponent<Camera>();
        reflectionCamera.cameraType = CameraType.Reflection; 
        reflectionCamera.enabled = false;
        reflectionCamera.allowMSAA = false;
        reflectionCamera.useOcclusionCulling = false;
      

        var cameraData = reflCameraGO.AddComponent<UniversalAdditionalCameraData>();
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
        
    }

    void CopyCameraParams(Camera currentCamera, int cullingMask)
    {
        reflectionCamera.transform.position = currentCamera.transform.position;
        reflectionCamera.transform.rotation = currentCamera.transform.rotation;
        reflectionCamera.fieldOfView = currentCamera.fieldOfView;
        reflectionCamera.nearClipPlane = currentCamera.nearClipPlane;
        reflectionCamera.farClipPlane = currentCamera.farClipPlane;
        reflectionCamera.rect = currentCamera.rect;
        reflectionCamera.aspect = currentCamera.aspect;
        reflectionCamera.orthographicSize = currentCamera.orthographicSize;
        reflectionCamera.renderingPath = RenderingPath.Forward;
        reflectionCamera.allowDynamicResolution = currentCamera.allowDynamicResolution;
        reflectionCamera.cullingMask = cullingMask;
        reflectionCamera.clearFlags = currentCamera.clearFlags;
        reflectionCamera.backgroundColor = currentCamera.backgroundColor;

        if (currentCamera.usePhysicalProperties)
        {
            reflectionCamera.usePhysicalProperties = true;
            reflectionCamera.focalLength = currentCamera.focalLength;
            reflectionCamera.sensorSize = currentCamera.sensorSize;
            reflectionCamera.lensShift = currentCamera.lensShift;
            reflectionCamera.gateFit = currentCamera.gateFit;
        }
    }

    void RenderCamera(ScriptableRenderContext context, Camera currentCamera, Vector3 waterPosition, Matrix4x4 cameraMatrix)
    {

        Vector3 pos = waterPosition + Vector3.up * m_planeOffset;
        var normal = Vector3.up;

        var d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
        var reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        var reflection = Matrix4x4.identity;
        reflection *= Matrix4x4.Scale(new Vector3(1, -1, 1));

        CalculateReflectionMatrix(ref reflection, reflectionPlane);

        reflectionCamera.transform.forward = Vector3.Scale(currentCamera.transform.forward, new Vector3(1, -1, 1));
        reflectionCamera.worldToCameraMatrix = cameraMatrix * reflection;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        var clipPlane = CameraSpacePlane(reflectionCamera, pos - Vector3.up * 0.1f, normal, 1.0f);
        var projection = currentCamera.CalculateObliqueMatrix(clipPlane);
        reflectionCamera.projectionMatrix = projection;


        var data = new PlanarReflectionSettingData();
        data.Set();
        try
        {
            reflectionCamera.targetTexture = reflectionRT;
            KW_Extensions.CameraRender(reflectionCamera);
        }
        finally
        {
            data.Restore();
        }
    }

    public void RenderPlanar(ScriptableRenderContext context, Camera currentCamera, Vector3 waterPosition, float resolutionScale, List<Material> waterShaderMaterials)
    {
        if (reflCameraGO == null)
        {
            CreateCamera();
        }

     
        CopyCameraParams(currentCamera, waterCullingMask); //currentCamera.copyFrom doesn't work correctly 

        var width = (int)(currentCamera.pixelWidth * resolutionScale);
        var height = (int)(currentCamera.pixelHeight * resolutionScale);
        InitializeReflectionTexture(width, height, true);

        RenderCamera(context, currentCamera, waterPosition, currentCamera.worldToCameraMatrix);

      
        foreach (var mat in waterShaderMaterials)
        {
            if (mat == null) continue;
            mat.SetTexture("KW_PlanarReflection", reflectionRT);
        }
    }

    public void RenderCubemap(ScriptableRenderContext context, Camera currentCamera, Vector3 waterPosition, float interval, int cullingMask, int texSize, bool useSkyFix, List<Material> waterShaderMaterials)
    {
        currentInterval += KW_Extensions.DeltaTime();
   
        if (!requiredUpdateAllFaces && currentInterval < interval / 6.0f) return; //6 faces x 6 time faster

        currentInterval = 0;

        if (reflCameraGO == null)
        {
            CreateCamera();
        }

      
        CopyCameraParams(currentCamera, cullingMask); //currentCamera.copyFrom doesn't work correctly 
        if(useSkyFix)
        {
            reflectionCamera.clearFlags = CameraClearFlags.SolidColor;
            reflectionCamera.backgroundColor = Color.black;
        }

        InitializeReflectionTexture(texSize, texSize, false);
        InitializeCubemapTexture(texSize, texSize);

        var currentFov = currentCamera.fieldOfView;
        var currentAspect = currentCamera.aspect;
        currentCamera.fieldOfView = 90;
        currentCamera.aspect = 1;
        reflectionCamera.fieldOfView = 90;
        reflectionCamera.aspect = 1;

        m_planeOffset = 0;
        m_ClipPlaneOffset = 0;
       

        if (requiredUpdateAllFaces || interval < 0.0001f)
        {
            RenderToCubemapFace(context, currentCamera, waterPosition, CubemapFace.NegativeX);
            //RenderToCubemapFace(context, currentCamera, waterPosition, CubemapFace.NegativeY);
            RenderToCubemapFace(context, currentCamera, waterPosition, CubemapFace.NegativeZ);
            RenderToCubemapFace(context, currentCamera, waterPosition, CubemapFace.PositiveX);
            RenderToCubemapFace(context, currentCamera, waterPosition, CubemapFace.PositiveY);
            RenderToCubemapFace(context, currentCamera, waterPosition, CubemapFace.PositiveZ);
        }
        else
        {
            var currentSide = cubeMapSides[sideIdx];
            sideIdx = (sideIdx < 4) ? sideIdx += 1 : 0;
            RenderToCubemapFace(context, currentCamera, waterPosition, (CubemapFace)currentSide);
            
        }


        requiredUpdateAllFaces = false;
        currentCamera.fieldOfView = currentFov;
        currentCamera.aspect = currentAspect;
       
        foreach (var mat in waterShaderMaterials)
        {
            if (mat == null) continue;
            mat.SetTexture("KW_ReflectionCube", reflectionCubemapRT);
        }
    }

    void RenderToCubemapFace(ScriptableRenderContext context, Camera currentCamera, Vector3 waterPosition, CubemapFace face)
    {
        var camPos = currentCamera.transform.position;
        var viewMatrix = Matrix4x4.Inverse(Matrix4x4.TRS(camPos, GetRotationByCubeFace(face), new Vector3(1, 1, -1)));
        RenderCamera(context, currentCamera, waterPosition, viewMatrix);
        Graphics.CopyTexture(reflectionRT, 0, reflectionCubemapRT, (int)face);
        //Graphics.SetRenderTarget(reflectionCubemapRT, 0, CubemapFace.PositiveY);
        //Graphics.Blit(reflectionRT, reflectionCubemapRT);
    }

    Quaternion GetRotationByCubeFace(CubemapFace face)
    {
        switch (face)
        {
            case CubemapFace.NegativeX: return Quaternion.Euler(0, -90, 0);
            case CubemapFace.PositiveX: return Quaternion.Euler(0, 90, 0);
            case CubemapFace.PositiveY: return Quaternion.Euler(90, 0, 0);
            case CubemapFace.NegativeY: return Quaternion.Euler(-90, 0, 0);
            case CubemapFace.PositiveZ: return Quaternion.Euler(0, 0, 0);
            case CubemapFace.NegativeZ: return Quaternion.Euler(0, -180, 0);
        }
        return Quaternion.identity;
    }

    private static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }
    private static Vector3 ReflectPosition(Vector3 pos)
    {
        var newPos = new Vector3(pos.x, -pos.y, pos.z);
        return newPos;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        var offsetPos = pos + normal * m_ClipPlaneOffset;
        var m = cam.worldToCameraMatrix;
        var cameraPosition = m.MultiplyPoint(offsetPos);
        var cameraNormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    class PlanarReflectionSettingData
    {
        private readonly bool _fog;
        private readonly int _maxLod;
        private readonly float _lodBias;

        public PlanarReflectionSettingData()
        {
            _fog = RenderSettings.fog;
            _maxLod = QualitySettings.maximumLODLevel;
            _lodBias = QualitySettings.lodBias;
        }

        public void Set()
        {
            GL.invertCulling = true;
            RenderSettings.fog = false; 
            QualitySettings.maximumLODLevel += 1;
            QualitySettings.lodBias = _lodBias * 0.5f;
        }

        public void Restore()
        {
            GL.invertCulling = false;
            RenderSettings.fog = _fog;
            QualitySettings.maximumLODLevel = _maxLod;
            QualitySettings.lodBias = _lodBias;
        }
    }
}
