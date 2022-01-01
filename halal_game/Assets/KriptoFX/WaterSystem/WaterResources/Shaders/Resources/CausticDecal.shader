Shader "Hidden/KriptoFX/Water/CausticDecal"
{
	Subshader
	{
		//Tags{"RenderType" = "Opaque" "Queue" = "Geometry+1" }

		Tags{ "Queue" = "AlphaTest"  "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite Off
		Cull Front
		ZTest Greater
		//ZTest Always
		Blend DstColor Zero

		//Blend SrcAlpha OneMinusSrcAlpha
		//Offset -1, -1

		Pass
		{


			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ USE_DISPERSION
			#pragma multi_compile _ USE_LOD1 USE_LOD2 USE_LOD3
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_DEPTH_SCALE

			#include "UnityCG.cginc"
			#include "KW_WaterVariables.cginc"

			sampler2D _MainTex;

			sampler2D KW_CausticLod0;
			sampler2D KW_CausticLod1;
			sampler2D KW_CausticLod2;
			sampler2D KW_CausticLod3;

			float KW_DecalScale;

			float4 KW_CausticLod0_TexelSize;
			float4 KW_CausticLod1_TexelSize;
			float KW_CausticDispersionStrength;
			float KW_CaustisStrength;

			float4 KW_CausticLodSettings;
			float3 KW_CausticLodOffset;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};


			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				float4 ray : TEXCOORD2;
				float3 rayCameraOffset : TEXCOORD3;
			};


			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xz / KW_FFTDomainSize;

				float3 viewRay = UnityObjectToViewPos(v.vertex).xyz;
				o.ray.w = viewRay.z;
				viewRay *= -1;
				o.ray.xyz = mul((float3x3)mul(unity_WorldToObject, UNITY_MATRIX_I_V), viewRay);
				o.rayCameraOffset = mul(mul(unity_WorldToObject, UNITY_MATRIX_I_V), float4(0, 0, 0, 1)).xyz;

				o.screenUV = ComputeScreenPos(o.vertex);

				return o;
			}

			half3 GetCausticLod(float2 decalUV, float lodDist, sampler2D tex, half3 lastLodCausticColor)
			{
				float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
				float caustic = tex2D(tex, uv);
				uv = 1 - min(1, abs(uv * 2 - 1));
				float lerpLod = uv.x * uv.y;
				lerpLod = min(1, lerpLod * 3);
				return lerp(lastLodCausticColor, caustic, lerpLod);
			}

			half3 GetCausticLodWithDynamicWaves(float2 decalUV, float lodDist, sampler2D tex, half3 lastLodCausticColor, float2 offsetUV1, float2 offsetUV2, float flowLerpMask)
			{
				float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
				float caustic1 = tex2D(tex, uv - offsetUV1);
				float caustic2 = tex2D(tex, uv - offsetUV2);
				float caustic = lerp(caustic1, caustic2, flowLerpMask);
				uv = 1 - min(1, abs(uv * 2 - 1));
				float lerpLod = uv.x * uv.y;
				lerpLod = min(1, lerpLod * 3);
				return lerp(lastLodCausticColor, caustic, lerpLod);
			}

			half3 GetCausticLodWithDispersion(float2 decalUV, float lodDist, sampler2D tex, half3 lastLodCausticColor, float texelSize, float dispersionStr)
			{
				float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
				float3 caustic;
				caustic.r = tex2D(tex, uv);
				caustic.g = tex2D(tex, uv + texelSize * dispersionStr * 2);
				caustic.b = tex2D(tex, uv + texelSize * dispersionStr * 4);

				uv = 1 - min(1, abs(uv * 2 - 1));
				float lerpLod = uv.x * uv.y;
				lerpLod = min(1, lerpLod * 3);
				return lerp(lastLodCausticColor, caustic, lerpLod);
			}

			inline float3 ScreenToWorld(float2 UV, float depth)
			{
				float2 uvClip = UV * 2.0 - 1.0;
				float4 clipPos = float4(uvClip, depth, 1.0);
				float4 viewPos = mul(KW_ProjToView, clipPos);
				viewPos /= viewPos.w;
				float3 worldPos = mul(KW_ViewToWorld, viewPos).xyz;
				return worldPos;
			}


			float KW_DynamicWavesAreaSize;
			sampler2D KW_DynamicWavesNormal;
			sampler2D KW_DynamicWaves;

			texture2D KW_CausticDepthTex;
			float KW_CausticDepthOrthoSize;
			float3 KW_CausticDepth_Near_Far_Dist;
			float3 KW_CausticDepthPos;

			float ComputeCausticOrthoDepth(float3 worldPos)
			{
				float2 depthUV = (worldPos.xz - KW_CausticDepthPos.xz - KW_WaterPosition.xz * 0) / KW_CausticDepthOrthoSize + 0.5;
				float terrainDepth = KW_CausticDepthTex.SampleLevel(sampler_linear_clamp, depthUV, 0).r * KW_CausticDepth_Near_Far_Dist.z - KW_CausticDepth_Near_Far_Dist.y;
				return terrainDepth;
			}


			half4 frag(v2f i) : SV_Target
			{
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (i.screenUV.xy / i.screenUV.w));
				float waterDepth = tex2Dproj(KW_WaterDepth, i.screenUV);
				bool isUnderwater = tex2Dproj(KW_WaterMaskScatterNormals_Blured, i.screenUV).x > 0.7;
				bool causticMask = isUnderwater ? waterDepth < depth : waterDepth > depth;

				if (causticMask < 0.0001) discard;

				i.ray /= i.ray.w;
				float zEye = LinearEyeDepth(depth);
				float3 localPos = i.rayCameraOffset + i.ray.xyz * zEye;

				float waterHeightOffset = ScreenToWorld((i.screenUV.xy / i.screenUV.w), waterDepth).y;
				float3 worldPos = mul(unity_ObjectToWorld, float4(localPos, 1));


				float dist = length(worldPos - _WorldSpaceCameraPos);

#if USE_DEPTH_SCALE
				float terrainDepth = ComputeCausticOrthoDepth(worldPos);
#else 
				float terrainDepth = 1;
				
#endif


				float depthTransparent = max(1, KW_Transparent * 2);
				terrainDepth = clamp(-terrainDepth, 0, depthTransparent) / (depthTransparent);
				

				float3 caustic = 0.1;
				
#if KW_DYNAMIC_WAVES
				float2 dynamicWavesUV = (worldPos.xz - KW_DynamicWavesWorldPos.xz) / KW_DynamicWavesAreaSize + 0.5;
				float2 dynamicWavesNormals = tex2D(KW_DynamicWavesNormal, dynamicWavesUV) * 2 - 1;
				

				half time1 = frac(_Time.x + 0.5);
				half time2 = frac(_Time.x);
				half flowLerpMask = abs((0.5 - time1) / 0.5);

				float2 uvOffset1 = 0.25 * dynamicWavesNormals * time1;
				float2 uvOffset2 = 0.25 * dynamicWavesNormals * time2;

	#if defined(USE_LOD3)
					caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.w, KW_CausticLod3, caustic, uvOffset1, uvOffset2, flowLerpMask);
	#endif
	#if defined(USE_LOD2) || defined(USE_LOD3)
					caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.z, KW_CausticLod2, caustic, uvOffset1, uvOffset2, flowLerpMask);
	#endif

	
	#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
					caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic, uvOffset1, uvOffset2, flowLerpMask);
	#endif
					
					caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic, uvOffset1, uvOffset2, flowLerpMask);
				
					float dynamicWaves = tex2D(KW_DynamicWaves, dynamicWavesUV) * 2 - 1;
					float3 dynWavesNormalized = normalize(float3(dynamicWavesNormals.x, 1, dynamicWavesNormals.y));
					float dynWavesCaustic = dot(dynWavesNormalized, float3(0.5, 1, 0.5));
					caustic *= 1 + clamp(dynamicWaves, -0.03, 1 )*20;
#else 


	#if defined(USE_LOD3)
					caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.w, KW_CausticLod3, caustic);
	#endif
	#if defined(USE_LOD2) || defined(USE_LOD3)
					caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.z, KW_CausticLod2, caustic);
	#endif

	#if USE_DISPERSION
		#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
						caustic = GetCausticLodWithDispersion(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic, KW_CausticLod0_TexelSize.x, KW_CausticDispersionStrength);
		#endif
					caustic = GetCausticLodWithDispersion(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic, KW_CausticLod0_TexelSize.x, KW_CausticDispersionStrength);

	#else
		#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
						caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic);
		#endif
					caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic);
	#endif
#endif
	
				


				//caustic = lerp(0.1, caustic, saturate(0.25 + KW_Transparent / 5));
				//caustic += pow(caustic.rrr, 5) * 10 * (1-terrainDepth);
				//caustic *= 10;
				caustic = lerp(1, caustic * 10, saturate(KW_CaustisStrength));
				caustic += caustic * caustic * caustic * saturate(KW_CaustisStrength - 1);
				float distFade = 1 - saturate(dist / KW_DecalScale * 2);
				caustic = lerp(1, caustic, distFade);

				float fade = saturate((waterHeightOffset - worldPos.y) * 2);

				if(!isUnderwater) caustic = lerp(1, caustic, fade);
				return float4(lerp(caustic, 1, terrainDepth), 1);

			}

			ENDCG
	}
	}
}
