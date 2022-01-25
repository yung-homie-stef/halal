Shader "Hidden/KriptoFX/Water/FlowMapEdit" {
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

		CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		float2 _MousePos;
		half _Size;
		half2 _Direction;
		float _BrushStrength;
		float isErase;
		float _UvScale;

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord: TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
			half2 flowMap = tex2D(_MainTex, i.uv);
			if (i.uv.x <= 0.01 || i.uv.x >= 0.99 || i.uv.y <= 0.01 || i.uv.y >= 0.99) return half4(0.5, 0.5, 0, 0);
			/*float clampMutliplier = 1 - step(i.uv.x, _MainTex_TexelSize * 1);
			clampMutliplier *= 1 - step(1.0 - _MainTex_TexelSize * 1, i.uv.x);
			clampMutliplier *= 1 - step(i.uv.y, _MainTex_TexelSize * 1);
			clampMutliplier *= 1 - step(1.0 - _MainTex_TexelSize * 1, i.uv.y);*/
			//clampMutliplier = 1 - clampMutliplier;
			//fixed orig = (tex2D(_MainTex, i.uv).r - 0.5) * 2.0;
			half grad = length(_MousePos.xy - i.uv);
			grad = 1 - saturate(grad * _Size);
			//grad = pow(grad, 1.5);
			//grad *= _BrushStrength * clampMutliplier;
			//half2 newVal = lerp(flowMap - grad * _Direction, flowMap + grad * _Direction, length(_Direction));

			half2 newVal;
			if (isErase > 0.5)
			{
				newVal = lerp(flowMap, half2(0.5, 0.5), grad * _BrushStrength * 0.25);
			}

			else
			{
				newVal = flowMap + grad * _Direction * _BrushStrength;
				newVal = clamp(newVal, 0.05, 0.95);
				//if (newVal.x < 0.01 || newVal.x > 0.99 || newVal.y < 0.01 || newVal.y > 0.99) newVal = flowMap;
			}


			//half blendMask = abs(flowMap.x * 2 - 1) + abs(flowMap.y * 2 - 1);

			/*#ifdef ERASE_MODE
				half2 newVal = lerp(flowMap, 0.5, grad);
			#else
				half2 newVal = flowMap + grad * _Direction;
			#endif*/

			//if(newVal.y > 0.5) newVal.y = flowMap.y - dir.y;
			//	else newVal.y = flowMap.y + dir.y;
			return float4(newVal, 0, 0);
		}

		half4 fragResize(v2f i) : SV_Target
		{
			i.uv = (i.uv - 0.5) * _UvScale + 0.5;
			if (i.uv.x <= 0.01 || i.uv.x >= 0.99 || i.uv.y <= 0.01 || i.uv.y >= 0.99) return half4(0.5, 0.5, 0, 0);
			half2 clampMask = half2(abs(i.uv.x - 0.5), abs(i.uv.y - 0.5)) * 2;
			clampMask = pow(clampMask, 10);
			clampMask = saturate(max(clampMask.x, clampMask.y));
			half2 resizedFlowMap = tex2D(_MainTex, i.uv).xy;
			return float4(resizedFlowMap, 0, 0);
		}

		ENDCG

		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				ENDCG
			}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment fragResize
				ENDCG
			}
	}
}
