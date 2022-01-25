Shader "Hidden/KriptoFX/Water/KW_WaterVolumetricLighting_URP"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

    }
        SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
                HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS 
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE 
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS 
            #pragma multi_compile _ _SHADOWS_SOFT 
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ KW_POINT_SHADOWS_SUPPORTED
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                 static const float ditherPattern[8][8] = {
                { 0.012f, 0.753f, 0.200f, 0.937f, 0.059f, 0.800f, 0.243f, 0.984f},
                { 0.506f, 0.259f, 0.690f, 0.443f, 0.553f, 0.306f, 0.737f, 0.490f},
                { 0.137f, 0.875f, 0.075f, 0.812f, 0.184f, 0.922f, 0.122f, 0.859f},
                { 0.627f, 0.384f, 0.569f, 0.322f, 0.675f, 0.427f, 0.612f, 0.369f},
                { 0.043f, 0.784f, 0.227f, 0.969f, 0.027f, 0.769f, 0.212f, 0.953f},
                { 0.537f, 0.290f, 0.722f, 0.475f, 0.522f, 0.275f, 0.706f, 0.459f},
                { 0.169f, 0.906f, 0.106f, 0.843f, 0.153f, 0.890f, 0.090f, 0.827f},
                { 0.659f, 0.412f, 0.600f, 0.353f, 0.643f, 0.400f, 0.584f, 0.337f},
                };

                sampler2D KW_WaterMaskScatterNormals_Blured;
                sampler2D KW_WaterDepth;

                uint KW_lightsCount;

                half KW_Transparent;
                half MaxDistance;
                half KW_RayMarchSteps;
                half KW_VolumeLightMaxDistance;			
                half KW_VolumeDepthFade;
                half4 KW_LightAnisotropy;

                float2 KW_DitherSceenScale;
               
                float3 KW_WaterPosition;

                float4 KW_WaterMaskScatterNormals_Blured_TexelSize;
                float4 KW_WaterDepth_TexelSize;
                float4 KW_Frustum[4];
                float4 KW_UV_World[4];

                struct appdata
                {
                    real4 vertex : POSITION;
                    real2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float2 ditherPos : TEXCOORD1;
                    float3 frustumWorldPos : TEXCOORD2;
                    float3 uvWorldPos : TEXCOORD3;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = TransformWorldToHClip(v.vertex);
                    o.uv = v.uv;
                    o.ditherPos = v.uv * KW_DitherSceenScale.xy;
                    o.frustumWorldPos = KW_Frustum[v.uv.x + v.uv.y * 2];
                    o.uvWorldPos = KW_UV_World[v.uv.x + v.uv.y * 2];
                    return o;
                }

                inline half MieScattering(float cosAngle)
                {
                    return KW_LightAnisotropy.w * (KW_LightAnisotropy.x / (pow(KW_LightAnisotropy.y - KW_LightAnisotropy.z * cosAngle, 1.5)));
                }

                inline float3 RayMarch(float2 ditherScreenPos, float3 rayStart, float3 rayDir, float rayLength, half isUnderwater)
                {
                    ditherScreenPos = ditherScreenPos % 8;
                    float offset = ditherPattern[ditherScreenPos.y][ditherScreenPos.x];

                    float stepSize = rayLength / KW_RayMarchSteps;
                    float3 step = rayDir * stepSize;
                    float3 currentPos = rayStart + step * offset;

                    float3 result = 0;
                    float cosAngle = 0;
                    float shadowDistance = saturate(distance(rayStart, _WorldSpaceCameraPos) - KW_Transparent);
                    float depthFade = 1 - exp(-((_WorldSpaceCameraPos.y - KW_WaterPosition.y) + KW_Transparent));


                    Light mainLight = GetMainLight();
                    [loop]
                    for (int i = 0; i < KW_RayMarchSteps; ++i)
                    {
                        float atten = MainLightRealtimeShadow(TransformWorldToShadowCoord(currentPos));
                        float3 scattering = stepSize;
#if defined (USE_CAUSTIC)
                        float underwaterStrength = lerp(saturate((KW_Transparent - 1) / 5) * 0.5, 1, isUnderwater);
                        scattering += scattering * ComputeCaustic(rayStart, currentPos) * underwaterStrength;

#endif
                        float3 light = atten * scattering * mainLight.color;
                        result.rgb += light;
                        currentPos += step;
                    }
                    cosAngle = dot(mainLight.direction.xyz, -rayDir);
                    result *= MieScattering(cosAngle);

                    

#ifdef _ADDITIONAL_LIGHTS
                    //uint pixelLightCount = GetAdditionalLightsCount(); //bug, unity does not update light count after removal 
                    uint pixelLightCount = KW_lightsCount; //bug, unity does not update light count after removal 
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        currentPos = rayStart + step * offset;
                        [loop]
                        for (int i = 0; i < KW_RayMarchSteps; ++i)
                        {

                            Light addLight = GetAdditionalPerObjectLight(lightIndex, currentPos);
#if KW_POINT_SHADOWS_SUPPORTED
                            float atten = AdditionalLightRealtimeShadow(lightIndex, currentPos, addLight.direction);
#else 
                            float atten = AdditionalLightRealtimeShadow(lightIndex, currentPos);
#endif
                           
                            float3 scattering = stepSize * addLight.color.rgb * 5;
                            float3 light = atten * scattering * addLight.distanceAttenuation;

                            cosAngle = dot(-rayDir, normalize(currentPos - addLight.direction.xyz));
                            light *= MieScattering(cosAngle);

                            result.rgb += light;
                            currentPos += step;

                        }
                        
                    }
#endif               

                    result /= KW_Transparent;
                    result *= KW_VolumeDepthFade;
                    //result *= 4;

                    return max(0, result);
                }


                float4 frag(v2f i) : SV_Target
                {
                    half mask = tex2D(KW_WaterMaskScatterNormals_Blured, i.uv - float2(0, 6 * KW_WaterMaskScatterNormals_Blured_TexelSize.y)).x;
                    if (mask < 0.45) discard;

                    float depthTop = tex2D(KW_WaterDepth, i.uv);
#if UNITY_REVERSED_Z
                    float depthBot = SampleSceneDepth(i.uv);
#else
                    float depthBot = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
#endif
                  
                    bool isUnderwater = mask < 0.75;
                    if (depthBot > depthTop && isUnderwater) discard;

                    float3 topPos = ComputeWorldSpacePosition(i.uv, depthTop, UNITY_MATRIX_I_VP);
                    float3 botPos = ComputeWorldSpacePosition(i.uv, depthBot, UNITY_MATRIX_I_VP);

                    float3 rayDir = botPos - topPos;
                    rayDir = normalize(rayDir);
                    float rayLength = KW_VolumeLightMaxDistance;

                    float3 rayStart;

                    if (isUnderwater) {
                        rayLength = min(length(topPos - botPos), rayLength);
                        rayStart = topPos;
                    }
                    else
                    {
                        rayDir = normalize(i.frustumWorldPos - _WorldSpaceCameraPos);
                        rayLength = min(length(i.uvWorldPos - botPos), rayLength);
                        rayLength = min(length(i.uvWorldPos - topPos), rayLength);
                        rayStart = i.uvWorldPos;
                    }
                    
                    half4 finalColor;
                    finalColor.rgb = RayMarch(i.ditherPos, rayStart, rayDir, rayLength, isUnderwater);
                    finalColor.a = MainLightRealtimeShadow(TransformWorldToShadowCoord(topPos));
                   
                    return finalColor;
           }
           ENDHLSL
       }
    }
}