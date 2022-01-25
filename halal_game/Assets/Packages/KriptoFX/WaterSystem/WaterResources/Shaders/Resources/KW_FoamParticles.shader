Shader "Hidden/KriptoFX/Water/KW_FoamParticles"
{
    Properties
    {   _Color("Color", Color) = (0.95, 0.95, 0.95, 0.12)
        _MainTex("Texture", 2D) = "white" {}
        KW_VAT_Position("Position texture", 2D) = "white" {}
        KW_VAT_Alpha("Alpha texture", 2D) = "white" {}
        KW_VAT_Offset("Height Offset", 2D) = "black" {}
        KW_VAT_RangeLookup("Range Lookup texture", 2D) = "white" {}


        _FPS("FPS", Float) = 6.66666
         //_FPS("FPS", Float) = 6.7

        _Size("Size", Float) = 0.09
        //_Scale("AABB Scale", Vector) = (26.3, 4.5, 31.16)
        _Scale("AABB Scale", Vector) = (26.3, 4.8, 30.5)
        _NoiseOffset("Noise Offset", Vector) = (0, 0, 0)
        // _Offset("Offset", Vector) = (-9.5, -1.85, -15.3, 0)
        _Offset("Offset", Vector) = (-9.35, -2.025, -15.6, 0)

        _Test("Test", Float) = 0.1
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1"}
            Pass
            {

            //Tags { "LightMode" = "ForwardBase" }
             //Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1"}
             Blend SrcAlpha OneMinusSrcAlpha
             ZWrite Off
             Cull Off
             //ZTest Always



                CGPROGRAM
                #pragma vertex vert_foam
                #pragma fragment frag_foam
                // make fog work
                #pragma multi_compile_fog
                #pragma multi_compile_fwdbase
                #pragma multi_compile_fwdadd_fullshadows
            #define FORWARD_BASE_PASS

                #pragma multi_compile _ KW_INTERACTIVE_WAVES
                #pragma shader_feature  KW_FLOW_MAP_EDIT_MODE
                #pragma multi_compile _ KW_FLOW_MAP
                #pragma shader_feature  KW_SHORELINE_EDIT_MODE
                #pragma multi_compile _ KW_SHORELINE
                #pragma multi_compile _ KW_FOAM
                #pragma multi_compile _ USE_MULTIPLE_SIMULATIONS

                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "KW_WaterVariables.cginc"
                #include "KW_WaterHelpers.cginc"
                #include "WaterVertFrag.cginc"
                #include "AutoLight.cginc"

                struct appdata_foam
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 uv2 : TEXCOORD1;
                };

                struct v2f_foam
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD1;
                    UNITY_FOG_COORDS(2)

                    float4 color : TEXCOORD3;
                    float3 worldPos : TEXCOORD5;
                    float4 screenPos : TEXCOORD6;
                    SHADOW_COORDS(9)

                };

                UNITY_DECLARE_SHADOWMAP(_DirShadowMapTexture);
                sampler2D KW_VAT_Position;
                float4 KW_VAT_Position_TexelSize;
                sampler2D KW_VAT_Alpha;
                sampler2D KW_VAT_RangeLookup;
                float4 KW_VAT_RangeLookup_TexelSize;
                sampler2D KW_VAT_Offset;

                sampler2D KW_BluredFoamShadow;
                float4 _MainTex_ST;
                float _Size;
                float4 _Offset;
                float3 _NoiseOffset;
                float _FPS;

                float4 _Color;

                float3 _Scale;
                half _Test;
                float3 KW_LightDir;


                float KW_SizeAdditiveScale;

                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, KW_WaveTimeOffset)
                UNITY_INSTANCING_BUFFER_END(Props)

                inline float3 ScreenToWorld(float2 UV, float depth)
                {
                    float2 uvClip = UV * 2.0 - 1.0;
                    float4 clipPos = float4(uvClip, depth, 1.0);
                    float4 viewPos = mul(KW_ProjToView, clipPos);
                    viewPos /= viewPos.w;
                    float3 worldPos = mul(KW_ViewToWorld, viewPos).xyz;
                    return worldPos;
                }


                inline fixed4 GetCascadeWeights_SplitSpheres(float3 wpos)
                {
                    float3 fromCenter0 = wpos.xyz - unity_ShadowSplitSpheres[0].xyz;
                    float3 fromCenter1 = wpos.xyz - unity_ShadowSplitSpheres[1].xyz;
                    float3 fromCenter2 = wpos.xyz - unity_ShadowSplitSpheres[2].xyz;
                    float3 fromCenter3 = wpos.xyz - unity_ShadowSplitSpheres[3].xyz;
                    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

                    fixed4 weights = float4(distances2 < unity_ShadowSplitSqRadii);
                    weights.yzw = saturate(weights.yzw - weights.xyz);
                    return weights;
                }

                inline float4 getShadowCoord(float4 worldPos, fixed4 weights) {

                    float3 shadowCoord = float3(0, 0, 0);

                    if (weights[0] == 1) shadowCoord += mul(unity_WorldToShadow[0], worldPos).xyz;
                    if (weights[1] == 1) shadowCoord += mul(unity_WorldToShadow[1], worldPos).xyz;
                    if (weights[2] == 1) shadowCoord += mul(unity_WorldToShadow[2], worldPos).xyz;
                    if (weights[3] == 1) shadowCoord += mul(unity_WorldToShadow[3], worldPos).xyz;

                    return float4(shadowCoord, 1);
                }


                float4 computeVertexData(float idx)
                {
                    float timeOffset = UNITY_ACCESS_INSTANCED_PROP(Props, KW_WaveTimeOffset);
                    float timeLimit = (14.0 * 15.0) / 20.0; //(frameX * frameY) / fps

                    //KW_Time = Test4.x * 0.01;
                    float time = frac((KW_GlobalTimeSpeedMultiplier * KW_Time) / timeLimit) * timeLimit;
                    time += timeOffset * KW_GlobalTimeOffsetMultiplier;
                    time = frac(time * KW_VAT_RangeLookup_TexelSize.x) * KW_VAT_RangeLookup_TexelSize.z;

                    float height = frac(idx / KW_VAT_Position_TexelSize.w);
                    float offset = (floor(idx / KW_VAT_Position_TexelSize.z)) * KW_VAT_Position_TexelSize.x; //todo check w instead of z

                    float4 lookup = tex2Dlod(KW_VAT_RangeLookup, float4((time * _FPS) * KW_VAT_RangeLookup_TexelSize.x, 0, 0, 0));

                    float offsetMin = min(lookup.y, offset);
                    float4 uv1 = float4((float2(offsetMin + lookup.x - KW_VAT_Position_TexelSize.x * 0.75, height)), 0, 0);
                    float4 texturePos1 = tex2Dlod(KW_VAT_Position, uv1);
                   // texturePos1.xyz = GammaToLinearSpace(texturePos1.xyz);
                    //texturePos1.a = tex2Dlod(KW_VAT_Alpha, uv1);


                    float offsetMin2 = min(lookup.w , offset);
                    float4 uv2 = float4((float2(offsetMin2 + lookup.z - KW_VAT_Position_TexelSize.x * 0.75, height)), 0, 0);
                    float4 texturePos2 = tex2Dlod(KW_VAT_Position, uv2);
                  //  texturePos2.xyz = GammaToLinearSpace(texturePos2.xyz);
                    //texturePos2.a = tex2Dlod(KW_VAT_Alpha, uv2);

                   // float interpolationMask = abs(texturePos1.z - texturePos2.z) > 0.15 ? 0 : 1;
                    //if (length(texturePos1.rgb) > 0.0001 && length(texturePos2.rgb) > 0.0001)
                    {
                        texturePos1 = lerp(texturePos1, texturePos2, frac(time * _FPS));
                    }


                    texturePos1.z = 1 - texturePos1.z;
                    return texturePos1;
                }

                v2f_foam vert_foam(appdata_foam v)
                {
                    v2f_foam o;

                    float3 cameraF = float3(v.uv.x - 0.5, v.uv.y - 0.5, 0);
                    _Size += KW_SizeAdditiveScale;
                    _Size /= length(unity_ObjectToWorld._m01_m11_m21);
                    cameraF *= float3(_Size, _Size, 1);
                    cameraF = mul(cameraF, UNITY_MATRIX_MV);

                  
                    //_Time.y = TEST;


                    float4 texturePos1 = computeVertexData(v.uv2.x);

                    float heightOffset = tex2Dlod(KW_VAT_Offset, float4(texturePos1.xz, 0, 0));

                    texturePos1.xyz *= _Scale;
                    texturePos1.xyz += v.uv2.yzw * _NoiseOffset + _Offset;

                    float3 localPos = texturePos1.xyz;

                    float3 waterOffset = 0;
                    float3 waterWorldPos = 0;
                    float4 shorelineUVAnim1;
                    float4 shorelineUVAnim2;
                    float4 shorelineWaveData1;
                    float4 shorelineWaveData2;

                    float3 worldPos = mul(unity_ObjectToWorld, float4(localPos, 1));

                    waterOffset += ComputeWaterOffset(worldPos);
                    ShorelineData shorelineData;
                  //  waterOffset += ComputeBeachWaveOffset(worldPos, shorelineData);
                    float3 scale = float3(length(unity_ObjectToWorld._m00_m10_m20), length(unity_ObjectToWorld._m01_m11_m21), length(unity_ObjectToWorld._m02_m12_m22));
                    waterOffset /= scale;
                    localPos += waterOffset;



                   // float2 depthUV = (worldPos.xz - KW_DepthPos.xz) / KW_DepthOrthographicSize + 0.5;
                   // float terrainDepth = tex2Dlod(KW_OrthoDepth, float4(depthUV, 0, 0)).r * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y + KW_DepthPos.y;
					float terrainDepth = ComputeWaterOrthoDepth(worldPos);


                    worldPos.y -= heightOffset;
                    worldPos.y = max(worldPos.y, (terrainDepth + 0.05));
                    localPos.y = mul(unity_WorldToObject, float4(worldPos, 1)).y;

                    v.vertex.xyz = cameraF;
                    v.vertex.xyz += localPos.xyz;

                    o.color.a = texturePos1.a;
                    half3 lightColor = _MainLightColor.rgb;
                    o.color.rgb = clamp(lightColor + KW_AmbientColor.xyz, 0, 0.95);
                    //o.color.rgb = dot(o.color.rgb, 0.333) * 0.5 + o.color.rgb * 0.5;
                    //o.color.rgb = lightColor;

                    o.uv = v.uv * float2(3, 4) - float2(2, 1);

                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.screenPos = ComputeScreenPos(o.pos);

                    TRANSFER_SHADOW(o);
                    return o;
                }

                half4 frag_foam(v2f_foam i) : SV_Target
                {

                    half atten = 1;
                   /* float4 cascadeWeights = GetCascadeWeights_SplitSpheres(i.worldPos);
                    float4 samplePos = getShadowCoord(float4(i.worldPos, 1), cascadeWeights);

                    half inside = dot(cascadeWeights, float4(1, 1, 1, 1));
                    atten = inside > 0 ? UNITY_SAMPLE_SHADOW(_DirShadowMapTexture, samplePos.xyz) : 1.0f;
                    atten = _LightShadowData.r + atten * (1 - _LightShadowData.r);*/

                    //float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

                    //float3 reflVec = reflect(viewDir, float3(0, 1, 0));
                    //half3 reflection = UNITY_SAMPLE_TEXCUBE_LOD(KW_ReflectionCube, reflVec, Test4.x);
                    //reflection = dot(reflection, 0.333) * 0.6 + reflection * 0.25;

                    half alphaMask = max(0, 1 - length(i.uv));

                    half4 result = _Color;

                    result.rgb *= i.color.rgb;
                   // result.rgb *= lerp(0.45, 1, atten);

                    result.a *= 1 + KW_SizeAdditiveScale*5;

                    result.a *= i.color.a;
                    result.a *= alphaMask;

                    return result;
                }
                ENDCG
            }

           Pass
            {
             Tags {"LightMode" = "ShadowCaster"}
                             Blend SrcAlpha OneMinusSrcAlpha
             ZWrite On
             Cull Off

                CGPROGRAM
                #pragma vertex vert_foam
                #pragma fragment frag_foam
                #pragma multi_compile_shadowcaster
                #pragma multi_compile_instancing

                #include "UnityCG.cginc"
                #include "KW_WaterVariables.cginc"
                #include "KW_WaterHelpers.cginc"
                #include "WaterVertFrag.cginc"

                sampler2D KW_VAT_Position;
                float4 KW_VAT_Position_TexelSize;
                sampler2D KW_VAT_Alpha;
                sampler2D KW_VAT_RangeLookup;
                float4 KW_VAT_RangeLookup_TexelSize;

                sampler2D KW_VAT_Offset;

                float _Size;
                float3 _Offset;
                float3 _NoiseOffset;
                float _FPS;

                float4 _Color;

                float3 _Scale;
                sampler2D _HeightTex;
                half _Test;


                float KW_SizeAdditiveScale;

                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, KW_WaveTimeOffset)
                    UNITY_INSTANCING_BUFFER_END(Props)


                struct appdata_foam
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 uv2 : TEXCOORD1;
                    float3 normal : NORMAL;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f_foam {
                    V2F_SHADOW_CASTER;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                        float2 uv : TEXCOORD0;
                    float alpha : TEXCOORD2;
                };

 float4 computeVertexData(float idx)
                {
                    float timeOffset = UNITY_ACCESS_INSTANCED_PROP(Props, KW_WaveTimeOffset);
                    float timeLimit = (14.0 * 15.0) / 20.0; //(frameX * frameY) / fps

                    //KW_Time = Test4.x * 0.01;
                    float time = frac((KW_GlobalTimeSpeedMultiplier * KW_Time) / timeLimit) * timeLimit;
                    time += timeOffset * KW_GlobalTimeOffsetMultiplier;
                    time = frac(time * KW_VAT_RangeLookup_TexelSize.x) * KW_VAT_RangeLookup_TexelSize.z;

                    float height = frac(idx / KW_VAT_Position_TexelSize.w);
                    float offset = (floor(idx / KW_VAT_Position_TexelSize.z)) * KW_VAT_Position_TexelSize.x; //todo check w instead of z

                    float4 lookup = tex2Dlod(KW_VAT_RangeLookup, float4((time * _FPS) * KW_VAT_RangeLookup_TexelSize.x, 0, 0, 0));

                    float offsetMin = min(lookup.y, offset);
                    float4 uv1 = float4((float2(offsetMin + lookup.x - KW_VAT_Position_TexelSize.x * 0.75, height)), 0, 0);
                    float4 texturePos1 = tex2Dlod(KW_VAT_Position, uv1);
                   // texturePos1.xyz = GammaToLinearSpace(texturePos1.xyz);
                    //texturePos1.a = tex2Dlod(KW_VAT_Alpha, uv1);


                    float offsetMin2 = min(lookup.w , offset);
                    float4 uv2 = float4((float2(offsetMin2 + lookup.z - KW_VAT_Position_TexelSize.x * 0.75, height)), 0, 0);
                    float4 texturePos2 = tex2Dlod(KW_VAT_Position, uv2);
                  //  texturePos2.xyz = GammaToLinearSpace(texturePos2.xyz);
                    //texturePos2.a = tex2Dlod(KW_VAT_Alpha, uv2);

                   // float interpolationMask = abs(texturePos1.z - texturePos2.z) > 0.15 ? 0 : 1;
                    //if (length(texturePos1.rgb) > 0.0001 && length(texturePos2.rgb) > 0.0001)
                    {
                        texturePos1 = lerp(texturePos1, texturePos2, frac(time * _FPS));
                    }


                    texturePos1.z = 1 - texturePos1.z;
                    return texturePos1;
                }

                v2f_foam vert_foam(appdata_foam v)
                {
                    v2f_foam o;

                    UNITY_INITIALIZE_OUTPUT(v2f_foam, o);

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    float3 cameraF = float3(v.uv.x - 0.5, v.uv.y - 0.5, 0);
                    _Size += KW_SizeAdditiveScale;
                    _Size /= length(unity_ObjectToWorld._m01_m11_m21);
                    //_Size *= 2;
                    cameraF *= float3(_Size, _Size, 1);
                    cameraF = mul(cameraF, UNITY_MATRIX_MV);

                      float4 texturePos1 = computeVertexData(v.uv2.x);
                    float heightOffset = tex2Dlod(KW_VAT_Offset, float4(texturePos1.xz, 0, 0));

                    texturePos1.xyz *= _Scale;
                    texturePos1.xyz += v.uv2.yzw * _NoiseOffset + _Offset;

                    float3 localPos = texturePos1.xyz;

                    float3 waterOffset = 0;
                    float2 uv;
                    float3 waterWorldPos = 0;
                    float4 shorelineUVAnim1;
                    float4 shorelineUVAnim2;
                    float4 shorelineWaveData1;
                    float4 shorelineWaveData2;

                    float3 worldPos = mul(unity_ObjectToWorld, float4(localPos, 1));

                    //waterOffset += ComputeWaterOffset(worldPos);

                    //ComputeBeachWaveOffset(waterWorldPos, waterOffset, shorelineUVAnim1, shorelineUVAnim2, shorelineWaveData1, shorelineWaveData2);
                    float3 scale = float3(length(unity_ObjectToWorld._m00_m10_m20), length(unity_ObjectToWorld._m01_m11_m21), length(unity_ObjectToWorld._m02_m12_m22));
                    waterOffset /= scale;
                    localPos += waterOffset;

                    worldPos = mul(unity_ObjectToWorld, float4(localPos, 1));

                   // float2 depthUV = (worldPos.xz - KW_DepthPos.xz) / KW_DepthOrthographicSize + 0.5;
                    //float terrainDepth = tex2Dlod(KW_OrthoDepth, float4(depthUV, 0, 0)).r * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y + KW_DepthPos.y;
					float terrainDepth = ComputeWaterOrthoDepth(worldPos);


                    worldPos.y -= heightOffset * 1;
                    worldPos.y = max(worldPos.y, (terrainDepth - 0.15));
                    localPos.y = mul(unity_WorldToObject, float4(worldPos, 1)).y  - 0.1;

                    v.vertex.xyz = cameraF;
                    v.vertex.xyz += localPos.xyz;
                    o.uv = v.uv * float2(3, 4) - float2(2, 1);

                    o.alpha = texturePos1.a;

                    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                    return o;
                }

                float4 frag_foam(v2f_foam i) : SV_Target
                {
                    half alphaUVMask = max(0, 1 - length(i.uv));
                    half alpha = _Color.a;
                    alpha *= alphaUVMask;
                    alpha *= 1 + KW_SizeAdditiveScale;
                    alpha *= i.alpha;

                    if (alpha < 0.06) discard;

                    UNITY_SETUP_INSTANCE_ID(i);
                    SHADOW_CASTER_FRAGMENT(i)
                }
                ENDCG
            }
        }
}
