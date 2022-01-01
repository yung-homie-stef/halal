Shader "KriptoFX/Water30/Underwater"
{
	CGINCLUDE

#include "UnityCG.cginc"
#include "AutoLight.cginc"
//#include "Lighting.cginc "
#include "KW_WaterHelpers.cginc"

	sampler2D					KW_VolumetricLight;
	sampler2D					KW_DispTex;
	sampler2D					KW_DispTex_LOD1;
	sampler2D					KW_DispTex_LOD2;
	float4						KW_DispTex_TexelSize;
	half						KW_FFTDomainSize;
	half						KW_FFTDomainSize_LOD1;
	half						KW_FFTDomainSize_LOD2;
	fixed						KW_Choppines;
	half						KW_RipplesScale;
	float2						KW_RipplesUVOffset;
	sampler2D					KW_RipplesTexture;
	float3						KW_WaterWorldPosition;
	float						KW_WaterHeightOffset;

	float4 KW_TurbidityColor;
	float KW_Transparent;

	sampler2D KW_UnderwaterPostFX_Blured;

	float4 _test;
	float4 _Color;
	sampler2D _CameraDepthTextureBeforeWaterZWrite;
	sampler2D _CameraDepthTextureBeforeWaterZWrite_Blured;
	float _InvFade;
	sampler2D _CameraOpaqueTexture;
	sampler2D _CameraOpaqueTexture_Blured;
	sampler2D KW_WaterDepth;
	float4 KW_WaterDepth_TexelSize;
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
	sampler2D KW_WaterMaskScatterNormals;
	float4 KW_WaterMaskScatterNormals_TexelSize;

	sampler2D KW_WaterMaskScatterNormals_Blured;
	float4 KW_WaterMaskScatterNormals_Blured_TexelSize;
	float4 _CameraDepthTexture_TexelSize;
	float4 KW_VolumetricLight_TexelSize;
	float4x4 KW_ViewToWorld;
	float4x4 KW_ProjToView;


	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	inline float3 ScreenToWorld(float2 UV, float depth)
	{
		float2 uvClip = UV * 2.0 - 1.0;
		float4 clipPos = float4(uvClip, depth, 1.0);
		float4 viewPos = mul(KW_ProjToView, clipPos);
		viewPos /= viewPos.w;
		float3 worldPos = mul(KW_ViewToWorld, viewPos).xyz;
		return worldPos;
	}

	v2f vert(appdata v)
	{
		v2f o;

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;

		return o;
	}
	float _MyTest;
	float4 Test4;
	float4 KW_WaterColor;
	float KW_Turbidity;
	sampler2D _CameraGBufferTexture2;
	

	half4 frag(v2f i) : SV_Target
	{
		
		half mask = tex2Dlod(KW_WaterMaskScatterNormals_Blured, float4(i.uv.x, i.uv.y , 0, 0)).x;
	//return float4(tex2D(KW_VolumetricLight, i.uv.xy).rgb, 0.6);
		if (mask < 0.7) return 0;
	//return 1;

		float waterDepth = tex2Dlod(KW_WaterDepth, float4(i.uv - KW_WaterDepth_TexelSize.y * 3, 0, 0));
		

#if UNITY_REVERSED_Z
		float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
#else
		float z = lerp(UNITY_NEAR_CLIP_VALUE, 1, SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy));
#endif

		float linearZ = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv.xy));
		float depthSurface = LinearEyeDepth(waterDepth);
		half waterSurfaceMask = saturate((depthSurface - linearZ));
		
		float fade = (min(depthSurface, linearZ) - UNITY_Z_0_FAR_FROM_CLIPSPACE(i.vertex.z)) * 0.5;
		//float fadeExp = saturate(1 - exp(-1 * fade / KW_Transparent));

#if USE_VOLUMETRIC_LIGHT
		half halfMask = 1-saturate(mask * 2 - 1);
		//halfMask *= halfMask;
		float3 volumeScattering = tex2D(KW_VolumetricLight, i.uv.xy - float2(0, halfMask * 0.1 + KW_VolumetricLight_TexelSize.y * 1)).rgb;
#else
		float3 volumeScattering = 0.5;
#endif

		//float halfWaterLineMask = saturate(0.45 - Pow5(maskRaw));
		half2 normals = tex2Dlod(KW_WaterMaskScatterNormals, float4(i.uv.xy, 0, 0)).zw * 2 - 1;
		half3 waterColorUnder = tex2D(_CameraOpaqueTexture, lerp(i.uv.xy, i.uv.xy * 1.75, 0));
		half3 waterColorBellow = tex2D(_CameraOpaqueTexture, i.uv.xy + normals);
		half3 refraction = lerp(waterColorBellow, waterColorUnder, waterSurfaceMask);

		
		///halfMask *= halfMask * halfMask;
		//return float4(halfMask, 0, 0, 1);
		
		half3 underwaterColor = ComputeUnderwaterColor(refraction, volumeScattering.rgb,  fade, KW_Transparent, KW_WaterColor, KW_Turbidity, KW_TurbidityColor);
		//underwaterColor.rgb = lerp(refraction, underwaterColor.rgb, mask);
		return float4(underwaterColor, 1);
	}

	half4 fragPostFX(v2f i) : SV_Target
	{

		half maskRaw = tex2Dlod(KW_WaterMaskScatterNormals_Blured, float4(i.uv, 0, 0)).x;
		if (maskRaw < 0.72) return 0;

		half3 color = tex2D(KW_UnderwaterPostFX_Blured, i.uv);

		return float4(color, 1);
	}

		ENDCG

	SubShader
	{
		Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Always
		ZWrite Off

		Stencil{
					Ref 230
					Comp Greater
					Pass keep
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ USE_VOLUMETRIC_LIGHT

			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragPostFX

			ENDCG
		}

	}
}
