Shader "Hidden/KriptoFX/Water/KW_MaskDepthNormalPass" {
	Properties{

	}

		SubShader{
			Pass // dx11 tesselation
			{
				ZWrite On


				Cull Off

				CGPROGRAM

				#include "UnityCG.cginc"
				#include "UnityPBSLighting.cginc"
				#include "KW_WaterVariables.cginc"
				#include "KW_WaterHelpers.cginc"
				#include "WaterVertFrag.cginc"
				#include "KW_Tessellation.cginc"

				#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
				#pragma multi_compile _ KW_DYNAMIC_WAVES
				#pragma multi_compile _ USE_MULTIPLE_SIMULATIONS
				#pragma multi_compile _ USE_SHORELINE
				#pragma multi_compile _ REFLECT_SUN
				#pragma multi_compile _ IGNORE_TESS_CULL
				//#define IGNORE_TESS_CULL

				#pragma target 4.6

				//#pragma vertex vertDepth
				//#pragma fragment fragDepth
				#pragma vertex vertHull
				#pragma fragment fragDepth
				#pragma hull HS
				#pragma domain DS_Depth

				ENDCG
			}

			Pass // dx9 without tesselation
			{
				ZWrite On
				Cull Off

				CGPROGRAM

				#include "UnityCG.cginc"
				#include "UnityPBSLighting.cginc"
				#include "KW_WaterVariables.cginc"
				#include "KW_WaterHelpers.cginc"
				#include "WaterVertFrag.cginc"

				#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
				#pragma multi_compile _ KW_DYNAMIC_WAVES
				#pragma multi_compile _ USE_MULTIPLE_SIMULATIONS
				#pragma multi_compile _ USE_SHORELINE
				#pragma multi_compile _ REFLECT_SUN


				#pragma vertex vertDepth
				#pragma fragment fragDepth


				ENDCG
			}
		}
}
