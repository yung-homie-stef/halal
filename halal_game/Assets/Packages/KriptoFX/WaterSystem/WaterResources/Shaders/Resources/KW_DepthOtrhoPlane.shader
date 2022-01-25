Shader "Hidden/KriptoFX/Water/KW_DepthOtrhoPlane" {
	SubShader{

		Tags { "RenderType" = "Opaque" }
		ZTest Always
		Cull Off
		Zwrite Off

		Pass{


			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float3				KW_WaterPosition;
			sampler2D KW_RawOrthoDepth;
			sampler2D KW_RawOrthoDepthFront;
			sampler2D KW_RawOrthoDepthBack;
			float _MyTest;
			float2 KW_CameraNearFar;

			struct v2f {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(float4(v.vertex.xyz, 1));
				o.uv = v.texcoord;
				return o;
			}

			float frag(v2f i) : SV_Target{

				float rawDepth = tex2D(KW_RawOrthoDepth, i.uv);
				float2 invertedUV = i.uv;
				invertedUV.y = 1 - invertedUV.y;
				float depthFront = 1 - tex2D(KW_RawOrthoDepthFront, invertedUV) - 0.5;
				float depthBack = tex2D(KW_RawOrthoDepthBack, i.uv) - 0.5;

				//float depth = (min(rawDepth, KW_CameraNearFar.x + KW_CameraNearFar.y - rawDepth));
				//float intersection = depthFront * depthBack;
				//return lerp(depth, 0, intersection);

				return rawDepth < 0.55 ? -(rawDepth - 0.5) * 4: -1;

			//	return depthFront > _MyTest;
			//	return depthBack*1000;

			}

			ENDCG
		}
	}
}
