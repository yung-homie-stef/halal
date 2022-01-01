Shader "Hidden/KriptoFX/Water/VolumetricLighting"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ SHADOWS_DEPTH
			#pragma multi_compile _ DIRECTIONAL POINT SPOT
			#pragma multi_compile _ USE_MULTIPLE_SIMULATIONS
			#pragma multi_compile _ USE_CAUSTIC
			#pragma multi_compile _ USE_LOD1 USE_LOD2 USE_LOD3


			#if defined(SHADOWS_DEPTH) || defined(SHADOWS_CUBE)
				#define SHADOWS_NATIVE
			#endif

			#include "UnityCG.cginc"
			#include "UnityDeferredLibrary.cginc"

			UNITY_DECLARE_SHADOWMAP(_DirShadowMapTexture);
#if defined(SHADOWS_CUBE_IN_DEPTH_TEX)
			UNITY_DECLARE_TEXCUBE_SHADOWMAP(_PointShadowMapTexture);
#else
			UNITY_DECLARE_TEXCUBE(_PointShadowMapTexture);
#endif
			UNITY_DECLARE_SHADOWMAP(_SpotShadowMapTexture);

			float4 KW_LightDir;
			float4 KW_LightPos;
			float4 KW_LightColor;


			int KW_DirLightCount;
			float4 KW_DirLightPositions[3];
			float4 KW_DirLightColors[3];

			int KW_PointLightCount;
			float4 KW_PointLightPositions[100];
			float4 KW_PointLightColors[100];

			int KW_SpotLightCount;
			float4 KW_SpotLightPositions[100];
			float4 KW_SpotLightColors[100];
			float4x4 KW_SpotLightWorldToShadows[100];
			int KW_SpotLightShadowIndex;

			sampler2D KW_PointLightAttenuation;
			sampler2D _LightVolume;

			sampler2D _MainTex;
			sampler2D KW_DitherTexture;
			sampler2D KW_SpotLightTex;
sampler2D					KW_DispTex;
sampler2D					KW_DispTex_LOD1;
sampler2D					KW_DispTex_LOD2;
sampler2D KW_NormTex;
sampler2D KW_NormTex_LOD1;
sampler2D KW_NormTex_LOD2;

			sampler2D KW_WaterDepth;
			sampler2D KW_WaterDepthWithFoam;
			float4 KW_WaterDepth_TexelSize;
			sampler2D KW_WaterMaskScatterNormals;
			sampler2D KW_WaterMaskScatterNormals_Blured;
			float4 KW_WaterMaskScatterNormals_Blured_TexelSize;
			//sampler2D _CameraDepthTextureAfterWaterZWrite;
			//sampler2D _CameraDepthTextureBeforeWaterZWrite;;
			sampler2D KW_WaterScreenPosTex;

			float4 KW_Frustum[4];
			float4 KW_UV_World[4];

			float2 KW_DitherSceenScale;
			float4x4 KW_ProjToView;
			float4x4 KW_ViewToWorld;
			float4x4 KW_SpotWorldToShadow;

			half KW_Transparent;
			half MaxDistance;
			half KW_RayMarchSteps;
			half _FogDensity;
			half _Extinction;
			half4 KW_LightAnisotropy;
			half _MieScattering;
			half _RayleighScattering;

half						KW_FFTDomainSize;
half						KW_FFTDomainSize_LOD1;
half						KW_FFTDomainSize_LOD2;
			float3 KW_WaterPosition;
			float KW_WindSpeed;
			half KW_Choppines;

			sampler2D KW_CausticLod0;
			sampler2D KW_CausticLod1;
			sampler2D KW_CausticLod2;
			sampler2D KW_CausticLod3;

			float4 KW_CausticLodSettings;
			float3 KW_CausticLodOffset;

			sampler2D KW_CausticTex;
			half KW_CausticDomainSize;
			float2 KW_CausticTex_TexelSize;
			float4	 KW_DispTex_TexelSize;
			float KW_ShadowDistance;
			float KW_VolumeLightMaxDistance;
			float _MyTest;
			float KW_VolumeLightBlurRadius;

			sampler2D _CameraOpaqueTexture;
			float4 KW_TurbidityColor;

			inline half getRayleighPhase(half cosTheta) {
				return 0.05968310365f * (1 + (cosTheta * cosTheta));
			}

			inline half MieScattering(float cosAngle)
			{
				return KW_LightAnisotropy.w * (KW_LightAnisotropy.x / (pow(KW_LightAnisotropy.y - KW_LightAnisotropy.z * cosAngle, 1.5)));
			}


			/*
			inline half getSchlickScattering(float costheta) {
				float sqr = 1 + KW_LightAnisotropy * costheta;
				sqr *= sqr;
				return 0.4959 / 12.5663706144 * sqr;
			}*/

			inline float3 ScreenToWorld(float2 UV, float depth)
			{
				float2 uvClip = UV * 2.0 - 1.0;
				float4 clipPos = float4(uvClip, depth, 1.0);
				float4 viewPos = mul(KW_ProjToView, clipPos);
				viewPos /= viewPos.w;
				float3 worldPos = mul(KW_ViewToWorld, viewPos).xyz;
				return worldPos;
			}

			//-------------------------------shadow helpers----------------------------------------------

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

			inline half UnitySamplePointShadowmap(float3 vec, float range)
			{
#if defined(SHADOWS_CUBE_IN_DEPTH_TEX)
				float3 absVec = abs(vec);
				float dominantAxis = max(max(absVec.x, absVec.y), absVec.z); // TODO use max3() instead
				dominantAxis = max(0.00001, dominantAxis - _LightProjectionParams.z); // shadow bias from point light is apllied here.
				dominantAxis *= _LightProjectionParams.w; // bias
				float mydist = -_LightProjectionParams.x + _LightProjectionParams.y / dominantAxis; // project to shadow map clip space [0; 1]

#if defined(UNITY_REVERSED_Z)
				mydist = 1.0 - mydist; // depth buffers are reversed! Additionally we can move this to CPP code!
#endif
#else
				float mydist = length(vec) * range;
#endif

#if defined (SHADOWS_CUBE_IN_DEPTH_TEX)
				half shadow = UNITY_SAMPLE_TEXCUBE_SHADOW(_PointShadowMapTexture, float4(vec, mydist));
				return lerp(_LightShadowData.r, 1.0, shadow);
#else
				half shadowVal = UnityDecodeCubeShadowDepth(UNITY_SAMPLE_TEXCUBE(_PointShadowMapTexture, vec));

				half shadow = shadowVal < mydist ? _LightShadowData.r : 1.0;
				return shadow;
#endif
			}


			float GetLightAttenuation(float3 wpos)
			{
				float atten = 0;

#if defined (DIRECTIONAL)
				atten = 1;
	#if defined (SHADOWS_DEPTH)
					float4 cascadeWeights = GetCascadeWeights_SplitSpheres(wpos);
					float4 samplePos = getShadowCoord(float4(wpos, 1), cascadeWeights);

					half inside = dot(cascadeWeights, float4(1, 1, 1, 1));
					atten = inside > 0 ? UNITY_SAMPLE_SHADOW(_DirShadowMapTexture, samplePos.xyz) : 1.0f;
					atten = _LightShadowData.r + atten * (1 - _LightShadowData.r);
	#endif
#endif
#if defined (SPOT)
					float4 lightPos = mul(KW_SpotWorldToShadow, float4(wpos, 1));
					float3 tolight = KW_LightPos.xyz - wpos;
					half3 lightDir = normalize(tolight);

					atten = tex2Dbias(KW_SpotLightTex, float4(lightPos.xy / lightPos.w * 0.5 + 0.25, 0, -8)).r;
					atten *= lightPos.w > 1;
					atten *= 1 - length(tolight) / KW_LightPos.w;

	#if defined(SHADOWS_DEPTH)
					atten *= UNITY_SAMPLE_SHADOW(_SpotShadowMapTexture, lightPos.xyz / lightPos.w);
	#endif
#endif

#if defined (POINT)
				float3 tolight = wpos - KW_LightPos.xyz;

				half3 lightDir = -normalize(tolight);

				float lightRange = 1.0f / KW_LightPos.w;
				float lightRangeDouble = 1.0f / (KW_LightPos.w * KW_LightPos.w);
				float att = dot(tolight, tolight) * lightRangeDouble;
				atten = tex2Dlod(_LightTextureB0, float4(att.rr, 0, 0));
	#if defined(SHADOWS_DEPTH)
				atten *= UnitySamplePointShadowmap(tolight, lightRange);
	#endif
#endif
				return atten;
			}
			float4 Test4;
			float3 KW_CausticLodPosition;
			float KW_DecalScale;

			half3 GetCausticLod(float3 currentPos, float offsetLength, float lodDist, sampler2D tex, half lastLodCausticColor, float lodScale)
			{
				float2 uv = ((currentPos.xz - KW_CausticLodPosition.xz) - offsetLength * KW_LightDir.xz) / lodDist + 0.5 - KW_CausticLodOffset.xz;
				half caustic = tex2Dlod(tex, float4(uv, 0, KW_VolumeLightBlurRadius * 0.5 + 1)).r;
				uv = 1 - min(1, abs(uv * 2 - 1));
				float lerpLod = uv.x * uv.y;
				lerpLod = min(1, lerpLod * 3);
				return lerp(lastLodCausticColor, caustic, lerpLod);
			}

			half ComputeCaustic(float3 rayStart, float3 currentPos)
			{
				//half deepFade = 1 - saturate((KW_WaterYPos - currentPos.y) / KW_Transparent * 0.5);
				//half topFade = saturate(KW_WaterYPos - currentPos.y);
				half angle = dot(float3(0, -0.999, 0), KW_LightDir);
				half offsetLength = (rayStart.y - currentPos.y) / angle;

				float3 caustic = 0.1;
#if defined(USE_LOD3)
				caustic = GetCausticLod(currentPos, offsetLength, KW_CausticLodSettings.w, KW_CausticLod3, caustic, 2);
#endif
#if defined(USE_LOD2) || defined(USE_LOD3)
				caustic = GetCausticLod(currentPos, offsetLength, KW_CausticLodSettings.z, KW_CausticLod2, caustic, 2);
#endif
#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
				caustic = GetCausticLod(currentPos, offsetLength, KW_CausticLodSettings.y, KW_CausticLod1, caustic, 2);
#endif
				caustic = GetCausticLod(currentPos, offsetLength, KW_CausticLodSettings.x, KW_CausticLod0, caustic, 2);

				float distToCamera = length(currentPos - _WorldSpaceCameraPos);
				float distFade = saturate(distToCamera / KW_DecalScale * 2);
				caustic = lerp(caustic, 0, distFade);

				return 0.1 * caustic;
			}

			static const float ditherPattern[4][4] = { { 0.1f, 0.5f, 0.125f, 0.625f},
			{ 0.75f, 0.22f, 0.875f, 0.375f},
			{ 0.1875f, 0.6875f, 0.0625f, 0.5625},
			{ 0.9375f, 0.4375f, 0.8125f, 0.3125} };

			inline float4 RayMarch(float2 ditherScreenPos, float3 rayStart, float3 rayDir, float rayLength, float3 topPos, half isUnderwater)
			{
				//ditherScreenPos = ditherScreenPos % 4;
				//float offset = ditherPattern[ditherScreenPos.x][ditherScreenPos.y];

				float offset = tex2D(KW_DitherTexture, ditherScreenPos).w;
				float stepSize = rayLength / KW_RayMarchSteps;
				float3 step = rayDir * stepSize;
				float3 currentPos = rayStart + step * offset;

				float4 result = 0;
				float cosAngle = 0;
				float scattering = 0;
				float shadowDistance = saturate(distance(rayStart, _WorldSpaceCameraPos) - KW_ShadowDistance);

#if defined (DIRECTIONAL)
				cosAngle = dot(KW_LightDir.xyz, -rayDir);
				half cosAngleRayleigh = dot(KW_LightDir.xyz, half3(0, -1, 0));
				half cosAngleVertical = saturate(dot(KW_LightDir.xyz, float3(0, -5, 0)));
#endif

				[loop]
				for (int i = 0; i < KW_RayMarchSteps; ++i)
				{
					float atten = GetLightAttenuation(currentPos);

#if defined (DIRECTIONAL)

					scattering = getRayleighPhase(cosAngleRayleigh) * 0.5;
					//scattering += MieScattering(cosAngle) * (1.0/8.0);
#if USE_CAUSTIC
					float underwaterStrength = lerp(saturate((KW_Transparent - 1) / 5) * 0.75, 1, isUnderwater);
					scattering += atten * ComputeCaustic(rayStart, currentPos) * underwaterStrength;
#endif
					//scattering = ComputeCaustic(rayStart, currentPos);
					scattering *= cosAngleVertical;

					float turbidityShadowMul = 1 - saturate(KW_Transparent / 10);
					atten += turbidityShadowMul * 0.125;

					//half shadowDeepFade = length(currentPos - rayStart) / rayLength;
					//shadowDeepFade = shadowDeepFade * shadowDeepFade * shadowDeepFade;
					//atten = lerp(atten, 1, shadowDeepFade);
					//half shadowDeepFade = saturate(exp((_WorldSpaceCameraPos.y - KW_WaterYPos + KW_Transparent) / KW_Transparent));

					//scattering *= shadowDeepFade;

#endif
#if defined (POINT)

					cosAngle = dot(rayDir, normalize(currentPos - KW_LightPos.xyz));
					scattering = getRayleighPhase(cosAngle) * 0.75;
					scattering += MieScattering(cosAngle) * 0.25;

#endif
#if defined (SPOT)
					cosAngle = dot(rayDir, normalize(currentPos - KW_LightPos.xyz));
					scattering = getRayleighPhase(cosAngle) * 2.75;
					scattering += MieScattering(cosAngle) * 0.25;
#endif

					half shadowDeepFade = length(currentPos - rayStart) / rayLength;
					shadowDeepFade = shadowDeepFade * shadowDeepFade * shadowDeepFade;
					atten = lerp(atten, 0, shadowDeepFade);

					result += atten * scattering * KW_LightColor;
					currentPos += step;
				}
				result.rgb = saturate((8.0 * result.rgb) / KW_RayMarchSteps) * 2;


				return result;
			}


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 ditherPos : TEXCOORD1;
				float3 frustumWorldPos : TEXCOORD2;
				float3 uvWorldPos : TEXCOORD3;
			};


			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.ditherPos = v.uv * KW_DitherSceenScale.xy / 8;
				o.frustumWorldPos = KW_Frustum[v.uv.x + v.uv.y * 2];
				o.uvWorldPos = KW_UV_World[v.uv.x + v.uv.y * 2];
				return o;
			}


			half4 frag (v2f i) : SV_Target
			{
				half mask = tex2D(KW_WaterMaskScatterNormals_Blured, i.uv - float2(0, 6 * KW_WaterMaskScatterNormals_Blured_TexelSize.y)).x;
				if (mask < 0.45) return float4(0, 0, 0, 1);
				//half2 waterNormals = tex2D(KW_WaterMaskScatterNormals, i.uv).zw * 2 - 1;
				//waterNormals *=0;


				float4 prevVolumeColor = tex2D(_MainTex, i.uv);
				float depthTop = tex2D(KW_WaterDepth, i.uv - float2(0, 6 * KW_WaterDepth_TexelSize.y));
				float depthBot = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			
				if (depthBot > depthTop && mask < 0.75) return float4(0, 0, 0, 1);

				float3 topPos = ScreenToWorld(i.uv, depthTop);
				float3 botPos = ScreenToWorld(i.uv, depthBot);

				float3 rayDir = botPos - topPos;
				rayDir = normalize(rayDir);
				float rayLength = KW_VolumeLightMaxDistance;

				half4 finalColor = 0;
				half bilateralBlurMask = 1;
				float3 rayStart;

				

				if (mask < 0.75) {
					float3 worldView = normalize(_WorldSpaceCameraPos.xyz - topPos);
					//rayDir = normalize(refract(-worldView, float3(waterNormals.x, 1, waterNormals.y), 1.0 / 1.33));
					rayLength = min(length(topPos - botPos), rayLength);
					bilateralBlurMask = depthTop >= depthBot ? 1 : -1;
					rayStart = topPos;
					finalColor = RayMarch(i.ditherPos, topPos, rayDir, rayLength, topPos, 0);
					
					//half3 sceneColor = tex2D(_CameraOpaqueTexture, i.uv);
					//half fadeExp = saturate((1 - exp(-length(topPos - botPos) / KW_Transparent)));
					//finalColor.rgb = lerp(sceneColor, finalColor.rgb, fadeExp);
				}
				else
				{

					rayDir = normalize(i.frustumWorldPos - _WorldSpaceCameraPos);
					rayLength = min(max(length(i.uvWorldPos - botPos), 1), rayLength);
					rayLength = min(max(length(i.uvWorldPos - topPos), 1), rayLength);
				
					rayStart = i.uvWorldPos;
					finalColor = RayMarch(i.ditherPos, rayStart, rayDir, rayLength, topPos, 1);
				
					//half3 sceneColor = tex2D(_CameraOpaqueTexture, i.uv);
					//half fadeExp = saturate((1 - exp(-length(i.uvWorldPos - botPos) / KW_Transparent)));
					//finalColor.rgb = lerp(sceneColor, finalColor.rgb, fadeExp);
				}



#if defined (DIRECTIONAL)
				finalColor.a = GetLightAttenuation(topPos);
#else
				finalColor.a = 0;
#endif
				finalColor.a = max(finalColor.a, prevVolumeColor.a);
				//finalColor.a = (finalColor.a * 0.5 + 0.5) * bilateralBlurMask;

				finalColor.rgb += prevVolumeColor.rgb;

				return finalColor;
			}
			ENDCG
		}
	}
}
