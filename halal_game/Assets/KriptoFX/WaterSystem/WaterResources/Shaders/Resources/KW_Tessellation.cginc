#include "Tessellation.cginc"

//float _TesselationFactor;
float _TesselationMaxDistance;
float _TesselationMaxDisplace;

struct TessellationFactors
{
	float edge[3]    : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

struct Hull_Input
{
	float4 vertex  : POSITION;
	float4 color  : COLOR0;
};

struct Hull_ControlPointOutput
{
	float3 vertex    : POS;
	float4 color  : COLOR0;
};

Hull_Input vertHull(float4 vertex : POSITION, float color : COLOR0)
{
	Hull_Input o;
	o.vertex = vertex;
	o.color = color;
	return o;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 DistanceBasedTess(float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tessFactor)
{
	float3 pos0 = mul(unity_ObjectToWorld, v0).xyz;
	float3 pos1 = mul(unity_ObjectToWorld, v1).xyz;
	float3 pos2 = mul(unity_ObjectToWorld, v2).xyz;
	float4 tess;
	
	float3 f;
	f.x = UnityCalcDistanceTessFactor(v0, minDist, maxDist, tessFactor);
	f.y = UnityCalcDistanceTessFactor(v1, minDist, maxDist, tessFactor);
	f.z = UnityCalcDistanceTessFactor(v2, minDist, maxDist, tessFactor);
	tess = UnityCalcTriEdgeTessFactors(f);
	
	return tess;
}

float4 DistanceBasedTessCull(float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tessFactor, float maxDisplace)
{
	float3 pos0 = mul(unity_ObjectToWorld, v0).xyz;
	float3 pos1 = mul(unity_ObjectToWorld, v1).xyz;
	float3 pos2 = mul(unity_ObjectToWorld, v2).xyz;
	float4 tess;

	if (UnityWorldViewFrustumCull(pos0, pos1, pos2, maxDisplace))
	{
		tess = 0.0f;
	}
	else
	{
		float3 f;
		f.x = UnityCalcDistanceTessFactor(v0, minDist, maxDist, tessFactor);
		f.y = UnityCalcDistanceTessFactor(v1, minDist, maxDist, tessFactor);
		f.z = UnityCalcDistanceTessFactor(v2, minDist, maxDist, tessFactor);
		tess = UnityCalcTriEdgeTessFactors(f);
	}
	return tess;
}

TessellationFactors HSConstant(InputPatch<Hull_Input, 3> patch)
{
	TessellationFactors f;
	//half4 factor = UnityEdgeLengthBasedTessCull(patch[0].vertex, patch[1].vertex, patch[2].vertex, _TesselationFactor, _TessMaxDistance);
#if IGNORE_TESS_CULL
	half4 factor = DistanceBasedTess(patch[0].vertex, patch[1].vertex, patch[2].vertex, 1, _TesselationMaxDistance, _TesselationFactor);
#else
	half4 factor = DistanceBasedTessCull(patch[0].vertex, patch[1].vertex, patch[2].vertex, 1, _TesselationMaxDistance, _TesselationFactor, _TesselationMaxDisplace);
#endif

	f.edge[0] = factor.x;
	f.edge[1] = factor.y;
	f.edge[2] = factor.z;
	f.inside = factor.w;
	return f;
}

[domain("tri")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[patchconstantfunc("HSConstant")]
[outputcontrolpoints(3)]
Hull_ControlPointOutput HS(InputPatch<Hull_Input, 3> Input, uint uCPID : SV_OutputControlPointID)
{
	Hull_ControlPointOutput o;
	o.vertex = Input[uCPID].vertex.xyz;
	o.color = Input[uCPID].color;
	return o;
}

[domain("tri")]
v2f DS(TessellationFactors HSConstantData, const OutputPatch<Hull_ControlPointOutput, 3> Input, float3 BarycentricCoords : SV_DomainLocation)
{
	float fU = BarycentricCoords.x;
	float fV = BarycentricCoords.y;
	float fW = BarycentricCoords.z;

	float3 vertex = Input[0].vertex * fU + Input[1].vertex * fV + Input[2].vertex * fW;

	return vert(float4(vertex, 1), Input[0].color);
}

[domain("tri")]
v2fDepth DS_Depth(TessellationFactors HSConstantData, const OutputPatch<Hull_ControlPointOutput, 3> Input, float3 BarycentricCoords : SV_DomainLocation)
{
	float fU = BarycentricCoords.x;
	float fV = BarycentricCoords.y;
	float fW = BarycentricCoords.z;

	float3 vertex = Input[0].vertex * fU + Input[1].vertex * fV + Input[2].vertex * fW;

	return vertDepth(float4(vertex, 1), Input[0].color);
}
