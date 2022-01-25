Shader "Hidden/KriptoFX/Water/KW_ShorelineWavePosition"
{
	Properties
	{

	}
	SubShader
	{


		Pass
		{
			Cull Off
			//ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing


			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 tangent : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			/*struct FragmentOutput
			{
				half4 dest0 : SV_Target0;
				half4 dest1 : SV_Target1;
			};*/

			uint _ShorelineBake_mrtBufferIdx;

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, KW_WavesUVOffset)
				UNITY_DEFINE_INSTANCED_PROP(float, KW_WaveScale)
				UNITY_DEFINE_INSTANCED_PROP(float, KW_WaveTimeOffset)
				UNITY_DEFINE_INSTANCED_PROP(float, KW_WaveAngle)
				UNITY_DEFINE_INSTANCED_PROP(float, KW_IsFirstWave)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				//FragmentOutput o;
				UNITY_SETUP_INSTANCE_ID(i);


				float4 waveUVOffset = UNITY_ACCESS_INSTANCED_PROP(Props, KW_WavesUVOffset);
				float alpha = 0;
				if (i.uv.x > waveUVOffset.x && i.uv.x < 1.0 - waveUVOffset.x && i.uv.y > waveUVOffset.y && i.uv.y < 1.0 - waveUVOffset.y) alpha = 1;

				i.uv.x = 1 - i.uv.x;
				float angle = UNITY_ACCESS_INSTANCED_PROP(Props, KW_WaveAngle);
				float timeOffset = UNITY_ACCESS_INSTANCED_PROP(Props, KW_WaveTimeOffset);

				if (_ShorelineBake_mrtBufferIdx == 0) return float4(i.uv.xy * (1 + waveUVOffset.xy * 2) - waveUVOffset.xy, angle, alpha);
				if (_ShorelineBake_mrtBufferIdx == 1) return float4(timeOffset, UNITY_ACCESS_INSTANCED_PROP(Props, KW_WaveScale), 0, 1);
				return 0;
				//o.dest0 = float4(i.uv.xy * (1 + waveUVOffset.xy * 2) - waveUVOffset.xy, angle, alpha);
				//o.dest1 = float4(timeOffset, UNITY_ACCESS_INSTANCED_PROP(Props, KW_WaveScale), 0, 1);


				//return o;
			}
			ENDCG
		}
	}
}
