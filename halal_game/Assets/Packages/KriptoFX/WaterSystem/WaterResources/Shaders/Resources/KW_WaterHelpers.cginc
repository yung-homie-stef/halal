
inline half3 NormalLocalToEye(half3 normal)
{
	normal.xz = (mul((float3x3)UNITY_MATRIX_IT_MV, half3(normal.x, 0, normal.z))).xz;
	return normal;
}

float2 ComputeScreenSpaceReflectionDir(half3 normal, half3 viewDir)
{
	float3 viewNorm = normalize(NormalLocalToEye(normal));
	float3 ssrViewDir = mul((float3x3)UNITY_MATRIX_V, viewDir);
	float3 ssrReflRay = normalize(reflect(-ssrViewDir, viewNorm));

	float4 ssrScreenPos = mul(unity_CameraProjection, float4(ssrReflRay, 1));

	ssrScreenPos.xy /= ssrScreenPos.w;
	return float2(ssrScreenPos.x * 0.5 + 0.5, -ssrScreenPos.y * 0.5 + 0.5);
}


half ComputeCustomSSS(half3 normal, half3 lightDir, half3 worldPos)
{
	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
	float3 H = (lightDir + normal);
	float scattering = dot(viewDir, -H);
	
	return saturate(scattering  * scattering * scattering);

}

fixed2 DecodeFixedRG(float v)
{
	fixed2 result;
	uint t = uint(v * 255);
	result.x = float(t % 4) / 2;
	result.y = float(t / 4) / 2;
	return result;
}

//inline half3 ComputeMulitpleLodNormals(half3 normal, sampler2D lod1, sampler2D lod2, half FFTDomainSize_LOD1, half FFTDomainSize_LOD2, half3 worldPos)
//{
//	half3 normal_lod1 = tex2D(KW_NormTex_LOD1, worldPos.xz / FFTDomainSize_LOD1);
//	half3 normal_lod2 = tex2D(KW_NormTex_LOD2, worldPos.xz / FFTDomainSize_LOD2);
//	return normalize(half3(normal.xz + normal_lod1.xz + normal_lod2.xz, normal.y * normal_lod1.y * normal_lod2.y)).xzy;
//}

inline half3 ComputeDetailedNormal(half3 normal, sampler2D detailedTex, float2 domainSize_Detail, float3 worldPos, half detailMaxDistance)
{
	half detailDistanceLod = 1 - saturate(length(_WorldSpaceCameraPos - worldPos) / detailMaxDistance);
	half3 normal_detail = tex2D(detailedTex, worldPos.xz / domainSize_Detail);
	return normalize(half3(normal.xz + normal_detail.xz * detailDistanceLod, normal.y * normal_detail.y)).xzy;
}

half ComputeMipFade(float4 vertex, float waterFarDistance)
{
	return 1-saturate(UnityObjectToClipPos(vertex).w / waterFarDistance);
	//return saturate(exp(-UnityObjectToClipPos(vertex).w / waterFarDistance));
}
//
//float Pow5(float x)
//{
//	return x * x * x * x * x;
//}

//float ComputeFresnel(float angle)
//{
//	float fresnel = 1.0 - angle;
//	fresnel = Pow5(fresnel);
//	return lerp(0.05, 0.99, fresnel);
//
//}


float ComputeUnderwaterFresnel(float a)
{
	a = a * a * a * a * a;
	return saturate(a * 4 - 3);
}

inline half3 NormalsCombine(half3 n1, half3 n2)
{
	return normalize( half3(n1.xz + n2.xz, n1.y * n2.y).xzy);
}

inline half3 NormalsCombine(half3 n1, half3 n2, half3 n3)
{
	return normalize(half3(n1.xz + n2.xz + n3.xz, 1).xzy);
}

inline half3 NormalsCombineMasked(half3 n1, half3 n2, half mask)
{
	return normalize(half3(n1.xz + n2.xz * mask, lerp(n1.y, n1.y * n2.y, mask)).xzy);
}

float4 cubic(float v) {
	float4 n = float4(1.0, 2.0, 3.0, 4.0) - v;
	float4 s = n * n * n;
	float x = s.x;
	float y = s.y - 4.0 * s.x;
	float z = s.z - 4.0 * s.y + 6.0 * s.x;
	float w = 6.0 - x - y - z;
	return float4(x, y, z, w) * (1.0 / 6.0);
}

inline half ComputeMipLevel(float2 uv, float2 texelSize, float maxLod)
{
	float2 texelUV = uv * texelSize;
	float2 dx = abs(ddx(texelUV));
	float2 dy = abs(ddy(texelUV));
	float d = max(dot(dx, dx), dot(dy, dy));

	const float rangeClamp = pow(2, maxLod - 1);
	d = clamp(sqrt(d), 1.0, rangeClamp);

	half mipLevel = log2(d);
	//mipLevel = floor(mipLevel);
	return mipLevel;
}

inline half4 SampleBilinear(sampler2D tex, float2 uv, float4 texelSize)
{
	float2 f = frac(uv * texelSize.zw);
	uv += (0.5 - f) * texelSize.xy;  
	half4 a = tex2Dlod(tex, float4(uv, 0, 0));
	half4 b = tex2Dlod(tex, float4(uv + float2(texelSize.x, 0.0), 0, 0));
	half4 c = tex2Dlod(tex, float4(uv + float2(0.0, texelSize.y), 0, 0));
	half4 d = tex2Dlod(tex, float4(uv + texelSize.xy, 0, 0));
	return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

inline half4 SampleBilinear(texture2D tex, SamplerState state, float2 uv, float4 texelSize, float level)
{
	float2 f = frac(uv * texelSize.zw);
	uv += (0- f) * texelSize.xy;
	half4 a = tex.SampleLevel(state, uv, level);
	half4 b = tex.SampleLevel(state, uv + float2(texelSize.x, 0.0), level);
	half4 c = tex.SampleLevel(state, uv + float2(0.0, texelSize.y), level);
	half4 d = tex.SampleLevel(state, uv + texelSize.xy, level);
	return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

//
//half4 SampleBilinear(sampler2D tex, float2 uv, float4 texelSize)
//{
//	half4 s1 = tex2Dlod(tex, float4(uv.x, uv.y + texelSize.y, 0, 0));
//	half4 s2 = tex2Dlod(tex, float4(uv.x + texelSize.x, uv.y, 0, 0));
//	half4 s3 = tex2Dlod(tex, float4(uv.x + texelSize.x, uv.y + texelSize.y, 0, 0));
//	half4 s4 = tex2Dlod(tex, float4(uv.x, uv.y, 0, 0));
//
//	float2 TexturePosition = float2(uv)*texelSize.z;
//
//	float fu = frac(TexturePosition.x);
//	float fv = frac(TexturePosition.y);
//
//	float4 tmp1 = lerp(s4, s2, fu);
//	float4 tmp2 = lerp(s1, s3, fu);
//
//	return lerp(tmp1, tmp2, fv);
//}
//
inline half4 Tex2D_AA_HQ(texture2D tex, SamplerState state, float2 uv)
{
	half4 color = tex.Sample(state, uv.xy);
	half lum = dot(color.xz, float3(0, 1, 0));

	float2 uv_dx = ddx(uv.xy);
	float2 uv_dy = ddy(uv.xy);

	color += tex.Sample(state, uv.xy + (1.0 / 5.5) * uv_dx + (4.0 / 5.5) * uv_dy);
	color += tex.Sample(state, uv.xy + (4.0 / 5.5) * uv_dx + (5.0 / 5.5) * uv_dy);
	color += tex.Sample(state, uv.xy + (3.0 / 5.5) * uv_dx + (1.0 / 5.5) * uv_dy);
	color += tex.Sample(state, uv.xy + (5.0 / 5.5) * uv_dx + (-3.0 / 5.5) * uv_dy);

	color += tex.Sample(state, uv.xy + (2.0 / 5.5) * uv_dx + (-4.0 / 5.5) * uv_dy);
	color += tex.Sample(state, uv.xy + (-1.0 / 5.5) * uv_dx + (-2.0 / 5.5) * uv_dy);
	color += tex.Sample(state, uv.xy + (-3.0 / 5.5) * uv_dx + (-5.0 / 5.5) * uv_dy);
	color += tex.Sample(state, uv.xy + (-4.0 / 5.5) * uv_dx + (-1.0 / 5.5) * uv_dy);

	color += tex.Sample(state, uv.xy + (-5.0 / 5.5) * uv_dx + (3.0 / 5.5) * uv_dy);
	color += tex.Sample(state, uv.xy + (-2.0 / 5.5) * uv_dx + (2.0 / 5.5) * uv_dy);


	color /= 11.0;

	return color;
}

inline half4 Tex2D_AA_LQ(texture2D tex, SamplerState state, float2 uv)
{
	half4 color = tex.Sample(state, uv.xy);
	half lum = dot(color.xz, float3(0, 1, 0));

	float2 uv_dx = ddx(uv);
	float2 uv_dy = ddy(uv);

	color += tex.Sample(state, uv.xy + (0.25) * uv_dx + (0.75) * uv_dy);
	color += tex.Sample(state, uv.xy + (-0.25) * uv_dx + (-0.75) * uv_dy);
	color += tex.Sample(state, uv.xy + (-0.75) * uv_dx + (0.25) * uv_dy);
	color += tex.Sample(state, uv.xy + (0.75) * uv_dx + (-0.25) * uv_dy);

	color /= 5.0;

	return color;
}


inline half4 SampleBicubicLevel(texture2D tex, SamplerState state, float2 uv, float4 texelSize, float level)
{
	uv = uv * texelSize.zw - 0.5;
	float2 fxy = frac(uv);
	uv -= fxy;

	float4 xcubic = cubic(fxy.x);
	float4 ycubic = cubic(fxy.y);

	float4 c = uv.xxyy + float2(-0.5, +1.5).xyxy;
	float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
	float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;
	offset *= texelSize.xxyy;

	half4 sample0 = tex.SampleLevel(state, offset.xz, level);
	half4 sample1 = tex.SampleLevel(state, offset.yz, level);
	half4 sample2 = tex.SampleLevel(state, offset.xw, level);
	half4 sample3 = tex.SampleLevel(state, offset.yw, level);

	float sx = s.x / (s.x + s.y);
	float sy = s.z / (s.z + s.w);

	return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
}

inline half4 SampleBicubic(texture2D tex, SamplerState state, float2 uv, float4 texelSize)
{
	uv = uv * texelSize.zw - 0.5;
	float2 fxy = frac(uv);
	uv -= fxy;

	float4 xcubic = cubic(fxy.x);
	float4 ycubic = cubic(fxy.y);

	float4 c = uv.xxyy + float2(-0.5, +1.5).xyxy;
	float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
	float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;
	offset *= texelSize.xxyy;

	half4 sample0 = tex.Sample(state, offset.xz);
	half4 sample1 = tex.Sample(state, offset.yz);
	half4 sample2 = tex.Sample(state, offset.xw);
	half4 sample3 = tex.Sample(state, offset.yw);

	float sx = s.x / (s.x + s.y);
	float sy = s.z / (s.z + s.w);

	return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
}


inline half4 SampleSupersampledMip(sampler2D tex, float2 uv, float2 texSize, float bias)
{
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);// manually calculate the per axis mip level, clamp to 0 to 1
	// and use that to scale down the derivatives
	dx *= saturate(
		0.5 * log2(dot(dx * texSize, dx * texSize))
	);
	dy *= saturate(
		0.5 * log2(dot(dy * texSize, dy * texSize))
	);// rotated grid uv offsets
	float2 uvOffsets = float2(0.125, 0.375);
	float4 offsetUV = float4(0.0, 0.0, 0.0, bias);// supersampled using 2x2 rotated grid

	half4 color;
	offsetUV.xy = uv + uvOffsets.x * dx + uvOffsets.y * dy;
	color = tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv - uvOffsets.x * dx - uvOffsets.y * dy;
	color += tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv + uvOffsets.y * dx - uvOffsets.x * dy;
	color += tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv - uvOffsets.y * dx + uvOffsets.x * dy;
	color += tex2Dbias(tex, offsetUV);
	color *= 0.25;
	return color;
}

inline half4 SampleSupersampled(sampler2D tex, float2 uv, float2 texelSize, float bias)
{
	half4 color;
	float3 texelOffset = float3(texelSize.xy, 0);
	color = tex2Dbias(tex, float4(uv + texelOffset.xz, 0, bias));
	color += tex2Dbias(tex, float4(uv - texelOffset.xz, 0, bias));
	color += tex2Dbias(tex, float4(uv + texelOffset.zy, 0, bias));
	color += tex2Dbias(tex, float4(uv - texelOffset.zy, 0, bias));
	color *= 0.25;

	//color = color * 0.5 + tex2Dbias(tex, float4(uv, 0, bias)) * 0.5;

	return color;
}

inline half4 SampleSupersampledLod(sampler2D tex, float2 uv, float2 texelSize)
{
	half4 color;
	float3 texelOffset = float3(texelSize.xy, 0);
	color = tex2Dlod(tex, float4(uv + texelOffset.xz, 0, 0));
	color += tex2Dlod(tex, float4(uv - texelOffset.xz, 0, 0));
	color += tex2Dlod(tex, float4(uv + texelOffset.zy, 0, 0));
	color += tex2Dlod(tex, float4(uv - texelOffset.zy, 0, 0));
	color *= 0.25;
	return color;
}


inline void ComputeScreenAndGrabPassPosSelf(float4 pos, out float4 screenPos, out float4 grabPassPos)
{
#if UNITY_UV_STARTS_AT_TOP
	float scale = -1.0;
#else
	float scale = 1.0f;
#endif

	screenPos = ComputeNonStereoScreenPos(pos);
	screenPos.zw = pos.w;
	grabPassPos.xy = (float2(pos.x, pos.y*scale) + pos.w) * 0.5;
	grabPassPos.zw = pos.w;
}


inline half Pow3 (half x)
{
    return x*x*x;
}

inline fixed ComputeSSS(half lh, float3 normalLod, half3 lightDir)
{
	return saturate(1 - lh) * max(0, dot(normalLod, -lightDir * float3(1, 0.1, 1)));
}

inline half3 ComputeDerivativeNormal(float3 pos)
{
	return normalize(cross(ddx(pos), ddy(pos)  * _ProjectionParams.x));
}


inline half3 ComputeDisplaceUsingFlowMap(texture2D displaceTex, SamplerState state, float2 flowMap, half3  displace, float2 uv, float time)
{
	half blendMask = abs(flowMap.x) + abs(flowMap.y);
	if (blendMask < 0.01) return displace;
	
	half time1 = frac(time + 0.5);
	half time2 = frac(time);
	half flowLerp = abs((0.5 - time1) / 0.5);
	half flowLerpFix = lerp(1, 0.65, abs(flowLerp * 2 - 1)); 

	half3 tex1 = displaceTex.SampleLevel(state, uv - 0.25 * flowMap * time1, 0);
	half3 tex2 = displaceTex.SampleLevel(state, uv - 0.25 * flowMap * time2, 0, 0);
	half3 flowDisplace = lerp(tex1, tex2, flowLerp);
	flowDisplace.xz *= flowLerpFix;
	return lerp(displace, flowDisplace, saturate(blendMask));
}


inline half3 ComputeNormalUsingFlowMap(texture2D normTex, SamplerState state, float2 flowMap, half3 normal, float2 uv, float time)
{
	half blendMask = abs(flowMap.x) + abs(flowMap.y);
	//if (blendMask < 0.01) return normal;

	half time1 = frac(time + 0.5);
	half time2 = frac(time);
	half flowLerp = abs((0.5 - time1) / 0.5);
	half flowLerpFix = lerp(1, 0.65, abs(flowLerp * 2 - 1)); //fix interpolation bug, TODO: I need find what cause this. 
	
	half3 tex1 = normTex.Sample(state, uv - 0.25 * flowMap * time1);
	half3 tex2 = normTex.Sample(state, uv - 0.25 * flowMap * time2);
	half3 flowNormal = lerp(tex1, tex2, flowLerp);
	flowNormal.xz *= flowLerpFix;
	return lerp(normal, flowNormal, saturate(blendMask));
}

inline void ComputeNormalsUsingDynamicRipples(sampler2D ripplesNormTex, sampler2D ripplesNormTexLast, sampler2D ripplesTex, float2 uv, inout half3 normal, inout half3 normalLod, inout float2 mainUV)
{
	half2 ripples = tex2D(ripplesTex, uv).xy;
	half2 ripplesPrev = tex2D(ripplesTex, uv).xy;
	ripples = lerp(ripples, ripplesPrev, 0.5).xy;
	half ripplesMask = tex2D(ripplesTex, uv).r;
	
	mainUV += ripples.xy / 10;
	//normal.xz += ripples.xy;
	
	normal.xz = lerp(normal.xz, ripples.xy, abs(ripplesMask));
	//normNotDetailed.xz += ripplesNormal * 2;
	normalLod.xz += ripples.xy;
	//return saturate(1 - saturate(abs(ripples.x * 5)));
}


//inline void ComputeFoam(float2 uv, half3 normal, inout half foam)
//{
//	half jacobian = tex2D(KW_NormTex, uv).a;
//	fixed normalizedWindSpeed = saturate(KW_WindSpeed - 4.5);
//
//	half foamTex = tex2D(_FoamTex, uv * 4 - _Time.x * 0.25 + normal.xz / 50).r;
//	half foamTex2 = tex2D(_FoamTex, uv * 5 + _Time.x * 0.35 + normal.xz / 40).r;
//	foamTex = max(foamTex, foamTex2);
//
//	half buubleTex = tex2D(_BubblesTex, uv * 1 + _Time.x * 0.3 - normal.xz / 150).r;
//	half buubleTex2 = tex2D(_BubblesTex, uv * 1.5 - _Time.x * 0.5 - normal.xz / 120).r;
//	buubleTex = max(buubleTex, buubleTex2);
//
//	foam += (saturate(pow(foamTex * 1.1, 5) * jacobian) + buubleTex * normalizedWindSpeed * 2);
//	foam = 0;
//}

//inline void ComputeBeachFoam(float2 uv, float3 worldPos, inout half2 foamMask, inout half foam)
//{
//	float2 distanceFieldUV = (worldPos.xz - KW_DistanceFieldDepthPos.xz) / KW_DistanceFieldDepthArea * 0.5 + 0.5;
//	half terrainDepth = tex2D(KW_DistanceField, distanceFieldUV).a + 1;
//	half foamMaskWithDepth = saturate((terrainDepth - 0.3) * 10) * foamMask.x;
//	float foam1 = tex2D(_FoamTex, uv * 6);
//	float foam2 = tex2D(_NoiseTex, uv * 2);
//
//	foam += lerp(foam2, foam1, foamMaskWithDepth) * foamMaskWithDepth * 3;
//	
//	foamMask.x = foamMaskWithDepth;
//}


inline half3 ViewNormal(half3 worldNorm)
{
	float3 viewNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
	//float3 viewPos = UnityObjectToViewPos(vertex);
	//float3 viewDir = normalize(viewPos);
	//float3 viewCross = cross(viewDir, viewNorm);
	//viewNorm = float3(-viewCross.y, viewCross.x, 0.0);
	return viewNorm;
}


inline half SelfSmithJointGGXVisibilityTerm(half NdotL, half NdotV, half roughness)
{
#if 0
	// Original formulation:
	//  lambda_v    = (-1 + sqrt(a2 * (1 - NdotL2) / NdotL2 + 1)) * 0.5f;
	//  lambda_l    = (-1 + sqrt(a2 * (1 - NdotV2) / NdotV2 + 1)) * 0.5f;
	//  G           = 1 / (1 + lambda_v + lambda_l);

	// Reorder code to be more optimal
	half a = roughness;
	half a2 = a * a;

	half lambdaV = NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
	half lambdaL = NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);

	// Simplify visibility term: (2.0f * NdotL * NdotV) /  ((4.0f * NdotL * NdotV) * (lambda_v + lambda_l + 1e-5f));
	return 0.5f / (lambdaV + lambdaL + 1e-5f);  // This function is not intended to be running on Mobile,
												// therefore epsilon is smaller than can be represented by half
#else
	// Approximation of the above formulation (simplify the sqrt, not mathematically correct but close enough)
	half a = roughness;
	half lambdaV = NdotL * (NdotV * (1 - a) + a);
	half lambdaL = NdotV * (NdotL * (1 - a) + a);

	return 0.5f / (lambdaV + lambdaL + 1e-5f);
#endif
}


inline half SelfGGXTerm(half NdotH, half roughness)
{
	half a2 = roughness * roughness;
	half d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
	return 0.31830988618f * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,
											// therefore epsilon is smaller than what can be represented by half
}



half2 Refract(half3 viewDir, half3 normal, float ior) {
	half nv = dot(normal, viewDir);
	half v2 = dot(viewDir, viewDir);
	half knormal = (sqrt(((ior * ior - 1)*v2) / (nv*nv) + 1.0) - 1.0)* nv;
	
	return (knormal * normal).xz;
}

inline float3 ScreenToWorld(float2 UV, float depth, float4x4 projToView, float4x4 viewToWorld)
{
	float2 uvClip = UV * 2.0 - 1.0;
	float4 clipPos = float4(uvClip, depth, 1.0);
	float4 viewPos = mul(projToView, clipPos);
	viewPos /= viewPos.w;
	float3 worldPos = mul(viewToWorld, viewPos).xyz;
	return worldPos;
}

half3 ComputeWaterRefractRay(half3 viewDir, half3 normal, half depth) {
	half nv = dot(normal, viewDir);
	half v2 = dot(viewDir, viewDir);
	half knormal = (sqrt(((1.7689 - 1.0) * v2) / (nv * nv) + 1.0) - 1.0) * nv;
	half3 result = depth * (viewDir + (knormal * normal));
	result.y = result.y * 0.35; //fix information lost in the near camera
	return result;
}

half3 ComputeRefractionWithDispersion(sampler2D refractedTex, float3 refractedPos, float3 refractedRay)
{
	half3 refraction;
	refraction.r = tex2Dproj(refractedTex, ComputeGrabScreenPos(mul(UNITY_MATRIX_VP, float4(refractedPos + refractedRay, 1.0)))).r;
	refraction.g = tex2Dproj(refractedTex, ComputeGrabScreenPos(mul(UNITY_MATRIX_VP, float4(refractedPos + refractedRay * 1.075, 1.0)))).g;
	refraction.b = tex2Dproj(refractedTex, ComputeGrabScreenPos(mul(UNITY_MATRIX_VP, float4(refractedPos + refractedRay * 1.15, 1.0)))).b;
	return refraction;
}

half3 KW_AmbientColor;

float Fresnel_IOR(float3 viewDir, float3 normal, float ior)
{
	float cosi = clamp(-1, 1, dot(viewDir, normal));
	float etai = 1, etat = ior;
	if (cosi > 0) 
	{ 
		float temp = etat;
		etat = etai;
		etai = temp;
	}
	
	float sint = etai / etat * sqrt(max(0.f, 1 - cosi * cosi));
	
	if (sint >= 1) {
		return 1;
	}
	else {
		float cost = sqrt(max(0.f, 1 - sint * sint));
		cosi = abs(cosi);
		float Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
		float Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
		return (Rs * Rs + Rp * Rp) / 2;
	}
	// As a consequence of the conservation of energy, transmittance is given by:
	// kt = 1 - kr;
}

inline half ComputeSpecular(half nl, half nv, half nh, half viewDistNormalized)
{
	half V = SelfSmithJointGGXVisibilityTerm(nl, nv, 0.03);
	half D = SelfGGXTerm(nh, viewDistNormalized * 0.1 + 0.03);

	half specularTerm = V * D;
	//
	//#   ifdef UNITY_COLORSPACE_GAMMA
	//	specularTerm = sqrt(max(1e-4h, specularTerm));
	//#   endif


	specularTerm = max(0, specularTerm * nl);

	return specularTerm;
}

half3 ComputeUnderwaterColor(half3 refraction, half3 volumeLight, half fade, half transparent, half3 waterColor, half turbidity, half3 turbidityColor)
{
	float fadeExp = saturate(1 - exp(-5 * fade / transparent));

	half3 absorbedColor = pow(clamp(waterColor.xyz, 0.1, 0.95), 25 * fade / transparent) ; //min range ~ 0.0  with pow(x, 70)
	absorbedColor = lerp(pow(waterColor.xyz, 15.0) * 0.05 * volumeLight.rgb, refraction, absorbedColor);
	
	//volumeLight.rgb = lerp(refraction, volumeLight, saturate(1 - exp(-1 * fade / transparent)));
	turbidityColor = lerp(refraction, turbidityColor * volumeLight.rgb, fadeExp);
	absorbedColor = lerp(absorbedColor, turbidityColor, turbidity);
	
	
	//absorbedColor *= volumeLight.rgb;

	return absorbedColor;
}

half3 ComputeLinearFresnel(half3 normal, half3 viewDir)
{
	half fresnel = dot(normal, viewDir);
	

	return saturate(fresnel);
	//normal = fresnel < 0.0f ? normal + viewDir * (-fresnel + 1e-5f) : normal;
	//return abs(dot(normal, viewDir)); //todo check abs
}

float ComputeWaterFresnel(float x)
{
	x = 1 - x;
	return 0.02 + 0.98 * x * x * x * x * x * x * x; //fresnel aproximation http://wiki.nuaj.net/images/thumb/1/16/Fresnel.jpg/800px-Fresnel.jpg
}

half3 ComputeSSS(half sssMask, half3 underwaterColor, half shadowMask, half KW_Transparent)
{
	float3 sssColor = dot(underwaterColor, 0.333) * 0.25 + underwaterColor * 0.75;
	return 2 * sssMask * shadowMask * sssColor * saturate(1 - KW_Transparent / 50);
}

half3 ComputeSunlight(half3 normal, half3 viewDir, half3 linearFresnel, float3 lightDir, float3 lightColor, half shadowMask, float viewDistNormalized, half KW_Transparent)
{
	half3 halfDir = normalize(lightDir + viewDir);
	half nh = saturate(dot(normal, halfDir));
	half nl = saturate(dot(normal, lightDir));
	half lh = saturate(dot(lightDir, halfDir));
	
	half3 specular = saturate(ComputeSpecular(nl, linearFresnel, nh, viewDistNormalized) - 1);
	specular *= specular;
	half sunset = saturate(0.01 + dot(lightDir, float3(0, 1, 0))) * 30;

	return shadowMask * specular * lightColor * sunset;

}

