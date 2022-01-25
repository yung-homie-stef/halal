Shader "Hidden/KriptoFX/Water/ScreenSpaceWaterMeshCombine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D KW_ScreenSpaceWater;

            half4 frag(v2f i) : SV_Target
            {
                half4 sceneColor = tex2D(_MainTex, i.uv);
                half4 waterColor = tex2D(KW_ScreenSpaceWater, i.uv);
                sceneColor.rgb = lerp(sceneColor.rgb, waterColor.rgb, waterColor.a > 0.9);
                return sceneColor;
            }
            ENDCG
        }
    }
}
