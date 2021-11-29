// Source / Author:
	//https://www.reddit.com/user/rotoscope-/
	//https://www.reddit.com/r/Unity3D/comments/dhr5g2/i_made_a_stochastic_texture_sampling_shader/
	//https://pastebin.com/sDrnzYxB


//hash for randomness
float2 hash2D2D (float2 s){
	//magic numbers
	return frac(sin(fmod(float2(dot(s, float2(127.1,311.7)), dot(s, float2(269.5,183.3))), 3.14159))*43758.5453);
}

void Tex2DStochastic_float(Texture2D Texture, SamplerState Sample, float2 UV, out float4 Out){
	//triangle vertices and blend weights
	//BW_vx[0...2].xyz = triangle verts
	//BW_vx[3].xy = blend weights (z is unused)
	float4x3 BW_vx;

	//uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
	float2 skewUV = mul(float2x2 (1.0 , 0.0 , -0.57735027 , 1.15470054), UV * 3.464);

	//vertex IDs and barycentric coords
	float2 vxID = float2 (floor(skewUV));
	float3 barry = float3 (frac(skewUV), 0);
	barry.z = 1.0-barry.x-barry.y;

	BW_vx = ((barry.z>0) ? 
		float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
		float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0-barry.y, 1.0-barry.x)));

	//calculate derivatives to avoid triangular grid artifacts
	float2 dx = ddx(UV);
	float2 dy = ddy(UV);

	//blend samples with calculated weights
	Out = mul(SAMPLE_TEXTURE2D(Texture, Sample, UV + hash2D2D(BW_vx[0].xy) + dx + dy), BW_vx[3].x) + 
			mul(SAMPLE_TEXTURE2D(Texture, Sample, UV + hash2D2D(BW_vx[1].xy)+ dx + dy), BW_vx[3].y) + 
			mul(SAMPLE_TEXTURE2D(Texture, Sample, UV + hash2D2D(BW_vx[2].xy)+ dx + dy), BW_vx[3].z);
}