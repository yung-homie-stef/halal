Shader "KriptoFX/Water/Water" {
	Properties{

	}

		SubShader{

		Tags{ "Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil {
				Ref 230
				Comp Greater
				Pass keep
			}
			Pass
			{
				ZWrite On

				Cull Back
				CGPROGRAM

				//#define ENVIRO_FOG
				#if defined(ENVIRO_FOG)
				#include "Assets/Enviro - Sky and Weather/Core/Resources/Shaders/Core/EnviroFogCore.cginc"
				#endif

				#include "UnityCG.cginc"
				#include "UnityPBSLighting.cginc"
				#include "KW_WaterVariables.cginc"
				#include "KW_WaterHelpers.cginc"
				#include "WaterVertFrag.cginc"
				#include "KW_Tessellation.cginc"


				#pragma shader_feature  KW_FLOW_MAP_EDIT_MODE
				#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
				#pragma multi_compile _ KW_DYNAMIC_WAVES
				#pragma multi_compile _ USE_MULTIPLE_SIMULATIONS
				#pragma multi_compile _ PLANAR_REFLECTION SSPR_REFLECTION
				#pragma multi_compile _ USE_SHORELINE
				#pragma multi_compile _ REFLECT_SUN
				#pragma multi_compile _ USE_VOLUMETRIC_LIGHT
				#pragma multi_compile _ FIX_UNDERWATER_SKY_REFLECTION
				#pragma multi_compile _ USE_FILTERING
				#pragma multi_compile_fog

				#pragma vertex vert
				#pragma fragment frag

				ENDCG
			}
		}
}
