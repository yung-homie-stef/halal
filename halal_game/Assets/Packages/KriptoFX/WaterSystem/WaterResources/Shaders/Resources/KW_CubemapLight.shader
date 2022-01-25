Shader "Hidden/KriptoFX/Water/CubemapLight"
{

    SubShader
    {
        Tags{ "Queue" = "Transparent"}

        Blend SrcAlpha One

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float height : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float screenZPos : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 KW_WaterPosition;


            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _CubemapLightColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _HeightOffset)
           UNITY_INSTANCING_BUFFER_END(Props)


            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = v.uv;
                o.height = mul(unity_ObjectToWorld, v.vertex).y - UNITY_ACCESS_INSTANCED_PROP(Props, _HeightOffset);
                o.screenZPos = ComputeScreenPos(o.vertex).z;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                if (i.height < KW_WaterPosition.y) return 0;
                i.uv = i.uv * 2  - 1;
                float circle = saturate(1-length(i.uv));
                float4 lightColor = UNITY_ACCESS_INSTANCED_PROP(Props, _CubemapLightColor);
                return float4(circle * lightColor.xyz * lightColor.a, UNITY_Z_0_FAR_FROM_CLIPSPACE(i.screenZPos));
            }
            ENDCG
        }
    }
}
