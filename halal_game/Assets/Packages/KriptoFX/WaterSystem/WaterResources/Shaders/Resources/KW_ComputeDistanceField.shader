Shader "Hidden/KriptoFX/Water30/KW_ComputeDistanceField"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "" {}
	}

		CGINCLUDE

	#include "UnityCG.cginc"

	sampler2D _MainTex;
	sampler2D _PrevTex;
	sampler2D KW_DistanceFieldDepthIntersection;
	float4 KW_DistanceFieldDepthIntersection_TexelSize;
	float4 _MainTex_TexelSize;
	int DF_Distance;
	int DF_DistanceOffset;
	float Radius;
	float KW_DistanceFieldMinBlurRadius;
	float _MyTest;
	float3 KW_WaterPosition;

	///////////////////////////////////// horizontal
	struct v2fH
	{
		float4 vertex  : POSITION;
		float2 uv : TEXCOORD0;
	};

	v2fH vertH(appdata_img v)
	{
		v2fH o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}


	float CalcC(float H, float V)
	{
		return (sqrt(H*H + V * V));
	}


	half fragH(v2fH i) : COLOR
	{


		half dist = CalcC(0.0, tex2D(_PrevTex, i.uv).r);
		for (int idx = 1; idx <= DF_Distance; idx += 10)
		{
			half H = idx * 1.0 / DF_Distance;
			float2 offset = float2(idx * KW_DistanceFieldDepthIntersection_TexelSize.x * 2, 0.0);
			dist = min(dist, CalcC(H, tex2D(_PrevTex, i.uv + offset).r));
			dist = min(dist, CalcC(H, tex2D(_PrevTex, i.uv - offset).r));
		}

		/*if (dist < 1.0)
		{
			dist = frac(dist*DF_Distance / 15.999);
		}*/
		return dist;
	}

	/////////////////////////////////// vertical

	struct v2fV
	{
		float4 vertex  : POSITION;
		float4 uv : TEXCOORD0;
	};

	v2fV vertV(appdata_img v)
	{
		v2fV o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = float4(v.texcoord.xy, 0, 0);
		return o;
	}

	half fragV(v2fV i) : COLOR
	{
		if (tex2Dlod(KW_DistanceFieldDepthIntersection, i.uv).r < 0.01) return 0.0;

		[loop]
		for (int idx = 1; idx <= DF_Distance; idx += 10)
		{
			float4 offset = float4(0.0, idx * KW_DistanceFieldDepthIntersection_TexelSize.x * 2, 0, 0);

			if (tex2Dlod(KW_DistanceFieldDepthIntersection, i.uv + offset).r < 0.01)
			{
				return idx * 1.0 / DF_Distance;
			}

			if (tex2Dlod(KW_DistanceFieldDepthIntersection, i.uv - offset).r < 0.01)
			{
				return idx * 1.0 / DF_Distance;
			}
		}
		return 1.0;
	}

	/////////////////////////////////// blur filter

	struct v2f
	{
		float4 vertex  : POSITION;
		float2 uv : TEXCOORD0;
		float4 uvOffset : TEXCOORD1;
	};

	v2f vertVertical(appdata_img v)
	{
		v2f o;

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		float2 offset1 = float2(0.0, _MainTex_TexelSize.y * Radius * 1.38461538);
		float2 offset2 = float2(0.0, _MainTex_TexelSize.y * Radius * 3.23076923);
		o.uvOffset.xy = offset1;
		o.uvOffset.zw = offset2;

		return o;
	}

	v2f vertHorizontal(appdata_img v)
	{
		v2f o;

		o.vertex = UnityObjectToClipPos(v.vertex);

		o.uv = v.texcoord.xy;
		float2 offset1 = float2(_MainTex_TexelSize.y * Radius * 1.38461538, 0.0);
		float2 offset2 = float2(_MainTex_TexelSize.y * Radius * 3.23076923, 0.0);
		o.uvOffset.xy = offset1;
		o.uvOffset.zw = offset2;

		return o;
	}

	half computeBlur(float2 uv, float4 uvOffset)
	{
		half tex = tex2D(_MainTex, uv).xyz;
		half sum = tex * 0.22702702;
		//tex = lerp(KW_DistanceFieldMinBlurRadius, 5, saturate(tex * 2));
		sum += tex2D(_MainTex, uv + uvOffset.xy * tex).x * 0.31621621;
		sum += tex2D(_MainTex, uv - uvOffset.xy * tex).x * 0.31621621;
		sum += tex2D(_MainTex, uv + uvOffset.zw * tex).x * 0.07027027;
		sum += tex2D(_MainTex, uv - uvOffset.zw * tex).x * 0.07027027;
		return sum;
	}

	half2 computeNormal(float2 uv, float scale)
	{
		float left = tex2D(_MainTex, uv + float2(-_MainTex_TexelSize.x * scale, 0)).r * 100;
		float right = tex2D(_MainTex, uv + float2(_MainTex_TexelSize.x * scale, 0)).r * 100;
		float top = tex2D(_MainTex, uv + float2(0, _MainTex_TexelSize.y * scale)).r * 100;
		float down = tex2D(_MainTex, uv + float2(0, -_MainTex_TexelSize.y * scale)).r * 100;

		float3 va = float3(float2(1.0, 0.0), right - left);
		float3 vb = float3(float2(0.0, 1.0), top - down);

		return normalize(cross(va, vb).rbg).xz;
	}

	half fragVertical(v2f i) : COLOR
	{
		half sum = computeBlur(i.uv, i.uvOffset);
		return sum;
	}

	half4 fragHorizontal(v2f i) : COLOR
	{
		half sum = computeBlur(i.uv, i.uvOffset);
		half2 normal = computeNormal(i.uv, 4);
		half depth = tex2D(KW_DistanceFieldDepthIntersection, i.uv).r;
		//half mask = saturate((1-length(i.uv * 2 - 1)) * 5);

		return half4(sum, normal, depth);
	}


	ENDCG

Subshader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Blend Off

		CGPROGRAM
		#pragma vertex vertH
		#pragma fragment fragH
		ENDCG
	}

	Pass {
		ZTest Always Cull Off ZWrite Off
		Blend Off

		CGPROGRAM
		#pragma vertex vertV
		#pragma fragment fragV
		ENDCG
	}

	Pass{
		ZTest Always Cull Off ZWrite Off
		Blend Off

		CGPROGRAM
		#pragma vertex vertVertical
		#pragma fragment fragVertical
		ENDCG
	}
	Pass{
		ZTest Always Cull Off ZWrite Off
		Blend Off

		CGPROGRAM
		#pragma vertex vertHorizontal
		#pragma fragment fragHorizontal
		ENDCG
	}

}

Fallback off

} // shader
