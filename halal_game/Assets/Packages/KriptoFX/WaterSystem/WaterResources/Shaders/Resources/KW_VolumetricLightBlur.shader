Shader "Hidden/KriptoFX/Water/BilateralBlur"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM

			#define SIGMA 2.5
			#define BSIGMA 0.2
			#define MSIZE 11

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			static const float kernelArr[11] = { 0.0215, 0.0443, 0.0776, 0.1158, 0.1473, 0.1595, 0.1473, 0.1158, 0.0776, 0.0443, 0.0215 };
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 KW_WaterMaskDepth_TexelSize;
			sampler2D KW_WaterMaskDepth;

			float normpdf(in float x, in float sigma)
			{
				return 0.39894 * exp(-0.5 * x * x / (sigma * sigma)) / sigma;
			}

			fixed2 DecodeFixedRG(float v)
			{
				fixed2 result;
				uint t = uint(v * 255.0);
				result.x = float(t % 16) / 15;
				result.y = float(t / 16) / 15;
				return result;
			}

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			float _MyTest;


			half4 frag(v2f i) : SV_Target
			{
				const int kSize = 5; //(MSIZE - 1) / 2;
				half4 final_colour = 0;
				half4 color = tex2D(_MainTex, i.uv);

				half3 pixelOffset = half3(_MainTex_TexelSize.xy * 0, 0);
				float shadowMask = abs(color.a) > 0.75;


				half Z = 0;
				half4 curentColor = 0;
				half factor = 0;

				half bZ = 0.159576; //1.0 / normpdf(0.0, SIGMA);

				for (int x = -kSize; x <= kSize; ++x)
				{
					for (int y = -kSize; y <= kSize; ++y)
					{
						curentColor = tex2D(_MainTex, i.uv + half2(x, y) * _MainTex_TexelSize.xy);
						curentColor.a = curentColor.a > 0;

						factor = normpdf(curentColor.a - 1, BSIGMA) * bZ * kernelArr[kSize + y] * kernelArr[kSize + x];
						Z += factor;
						final_colour += factor * curentColor;
					}
				}

				return half4(final_colour.rgb / Z, shadowMask);
			}

			ENDCG
		}
	}
}

