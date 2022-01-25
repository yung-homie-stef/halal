struct v2fDepth {
	float4  pos  : SV_POSITION;
	float3 worldPos : TEXCOORD1;
	float underwaterCubeMask : COLOR0;
};



struct v2f {
	float4  pos  : SV_POSITION;

	float3 worldPos : TEXCOORD1;
	float3 viewDir : TEXCOORD2;

	float4 screenPos : TEXCOORD3;
	float4 grabPos : TEXCOORD4;

	half heightScattering : TEXCOORD5;
	float3 derivativePos : TEXCOORD6;

	float3 worldPosRefracted : TEXCOORD8;

	float4 shorelineUVAnim1 : TEXCOORD9;
	float4 shorelineUVAnim2 : TEXCOORD10;
	float4 shorelineWaveData1 : TEXCOORD11;
	float4 shorelineWaveData2 : TEXCOORD12;
	float underwaterCubeMask : COLOR0;
	UNITY_FOG_COORDS(13)
};

float2 GetAnimatedUV(float2 uv, int _ColumnsX, int _RowsY, float FPS, float time)
{
	float2 size = float2(1.0f / _ColumnsX, 1.0f / _RowsY);
	uint totalFrames = _ColumnsX * _RowsY;
	uint index = time * 1.0f * FPS;
	uint indexX = index % _ColumnsX;
	uint indexY = floor((index % totalFrames) / _ColumnsX);

	float2 offset = float2(size.x * indexX, -size.y * indexY);
	float2 newUV = uv * size;
	newUV.y = newUV.y + size.y * (_RowsY - 1);

	return newUV + offset;
}

texture2D KW_DynamicWaves;
texture2D KW_DynamicWavesNormal;

float OverridedTime;
float KW_WavesMapSize;

texture2D KW_ScreenSpaceReflectionTex;


float4 Test;

float4 KW_LastAreaOffset;
float4 KW_AreaOffset;
float KW_DynamicWavesAreaSize;

float3 ComputeWaterOffset(float3 worldPos)
{
	//worldPos.xyz = mul(unity_ObjectToWorld, vertex);
	float2 uv = worldPos.xz / KW_FFTDomainSize;
	float3 offset = 0;
#if defined(USE_FILTERING) || defined(USE_CAUSTIC_FILTERING)
	float3 disp = SampleBicubicLevel(KW_DispTex, sampler_linear_repeat, uv, KW_DispTex_TexelSize, 0);
#else
	float3 disp = KW_DispTex.SampleLevel(sampler_linear_repeat, uv, 0).xyz;
#endif




#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE) 
	float2 flowMapUV = (worldPos.xz - KW_FlowMapOffset.xz) / KW_FlowMapSize + 0.5;
	float2 flowmap = KW_FlowMapTex.SampleLevel(sampler_linear_repeat, flowMapUV, 0) * 2 - 1;
	disp = ComputeDisplaceUsingFlowMap(KW_DispTex, sampler_linear_repeat, flowmap, disp, uv, KW_Time * KW_FlowMapSpeed);
#endif

#if KW_DYNAMIC_WAVES
	float2 dynamicWavesUV = (worldPos.xz - KW_DynamicWavesWorldPos.xz) / KW_DynamicWavesAreaSize + 0.5;
	float dynamicWave = KW_DynamicWaves.SampleLevel(sampler_linear_clamp, dynamicWavesUV, 0);
	disp.y -= dynamicWave * 0.15;
#endif

#if defined(KW_FLOW_MAP_FLUIDS) && !defined(KW_FLOW_MAP_EDIT_MODE)
	float2 fluidsUV_lod0 = (worldPos.xz - KW_FluidsMapWorldPosition_lod0.xz) / KW_FluidsMapAreaSize_lod0 + 0.5;
	float2 fluids_lod0 = tex2Dlod(KW_Fluids_Lod0, float4(fluidsUV_lod0, 0, 0));

	float2 fluidsUV_lod1 = (worldPos.xz - KW_FluidsMapWorldPosition_lod1.xz) / KW_FluidsMapAreaSize_lod1 + 0.5;
	float2 fluids_lod1 = tex2Dlod(KW_Fluids_Lod1, float4(fluidsUV_lod1, 0, 0));

	float2 maskUV_lod0 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
	float lodLevelFluidMask_lod0 = saturate((maskUV_lod0.x * maskUV_lod0.y - 0.01) * 3);
	float2 maskUV_lod1 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
	float lodLevelFluidMask_lod1 = saturate((maskUV_lod1.x * maskUV_lod1.y - 0.01) * 3);

	float2 fluids = lerp(fluids_lod1, fluids_lod0, lodLevelFluidMask_lod0);
	fluids *= lodLevelFluidMask_lod1;
	disp = ComputeDisplaceUsingFlowMap(KW_DispTex, sampler_linear_repeat, fluids * KW_FluidsVelocityAreaScale * 0.75, disp, uv, KW_Time * KW_FlowMapSpeed);

#endif



#ifdef USE_MULTIPLE_SIMULATIONS
	disp += KW_DispTex_LOD1.SampleLevel(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD1, 0).xyz;
	disp += KW_DispTex_LOD2.SampleLevel(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD2, 0).xyz;
#endif
	offset += disp;

	//offset.y += disp.y;
	//offset.xz += disp.xz * 1.25;
	return offset;
}

float3 ComputeBeachWaveOffsetForOneLine(float2 wavesMapUV, float terrainDepth, float time, sampler2D tex_UV_angle_alpha, sampler2D tex_timeOffset_scale,
	inout float4 shorelineUVAnim, inout float4 shorelineWaveData)
{
	
	float fps = 20;
	float4 uv_angle_alpha = tex2Dlod(tex_UV_angle_alpha, float4(wavesMapUV, 0, 0));
	float2 timeOffset_Scale = tex2Dlod(tex_timeOffset_scale, float4(wavesMapUV, 0, 0));

	shorelineUVAnim.xy = uv_angle_alpha;

	float2 waveUV = uv_angle_alpha.xy; 
	float waveAngle = uv_angle_alpha.z;
	float waveAlpha = uv_angle_alpha.w;
	float timeOffset = timeOffset_Scale.x;
	
	float waveScale = timeOffset_Scale.y;

	if (waveAlpha.x > 0.1) 
	{
		
		time += timeOffset * KW_GlobalTimeOffsetMultiplier;
	
		float2 uv = GetAnimatedUV(waveUV, 14, 15, fps, time);
		float2 prevUV = GetAnimatedUV(waveUV, 14, 15, fps, time + 1.0 / fps);

		float3 pos = tex2Dlod(KW_ShorelineWaveDisplacement, float4(uv, 0, 0));
		float3 pos2 = tex2Dlod(KW_ShorelineWaveDisplacement, float4(prevUV, 0, 0));
		pos = lerp(pos, pos2, frac(time * fps));
		pos.y -= 2;
		pos.xy *= pos.z;

		float angle = (360 * waveAngle) * UNITY_PI / 180.0;
		float sina, cosa;
		sincos(angle, sina, cosa);
		float3 offsetWave = float3(cosa * pos.x, pos.y, -sina * pos.x);
		
		offsetWave = offsetWave * waveAlpha * waveScale * 0.3;
		offsetWave.y = max(offsetWave.y, pos.z * (terrainDepth - KW_WaterPosition.y + 0.125));


		shorelineUVAnim.xy = uv;
		shorelineUVAnim.zw = prevUV;

		shorelineWaveData.xy = float2(-sina, cosa);
		shorelineWaveData.z = frac(time * fps);
		shorelineWaveData.w = waveAlpha;

		return offsetWave;
	}
	return 0;
}

float3 ComputeBeachWaveNormal(float4 shorelineUVAnim, float4 shorelineWaveData)
{
	float4 waveNorm = tex2D(KW_ShorelineWaveNormal, shorelineUVAnim.xy).xyzw;
	float4 waveNorm2 = tex2D(KW_ShorelineWaveNormal, shorelineUVAnim.zw).xyzw;
	waveNorm = lerp(waveNorm, waveNorm2, shorelineWaveData.z);
	waveNorm.xyz = waveNorm.xyz * 2 - 1;
	waveNorm.xz *= -1;
	
	float2x2 m = float2x2(shorelineWaveData.y, -shorelineWaveData.x, shorelineWaveData.x, shorelineWaveData.y);
	waveNorm.xz = mul(m, waveNorm.xz);
	float wavesAlpha = shorelineWaveData.w > 0.999 ? 1 : 0;
	waveNorm.a *= wavesAlpha;
	return lerp(float3(0, 1, 0), waveNorm.xyz, waveNorm.a);
}

float ComputeWaterOrthoDepth(float3 worldPos)
{
	float2 depthUV = (worldPos.xz - KW_ShorelineAreaPos.xz) / KW_ShorelineDepthOrthoSize + 0.5;
	if (depthUV.x < 0.001 || depthUV.x > 0.999 || depthUV.y < 0.001 || depthUV.y > 0.999) return 0;

	float terrainDepth = KW_ShorelineDepthTex.SampleLevel(sampler_linear_clamp, depthUV, 0).r * KW_ShorelineDepth_Near_Far_Dist.z - KW_ShorelineDepth_Near_Far_Dist.y + KW_ShorelineAreaPos.y;
	return terrainDepth;
}

float3 ComputeBeachWaveOffset(float3 worldPos, inout ShorelineData shorelineData, float timeOffset = 0)
{
	shorelineData.uv1 = 0;
	shorelineData.uv2 = 0;
	shorelineData.data1 = 0;
	shorelineData.data2 = 0;
	float3 offset = 0;

	float2 wavesMapUV = (worldPos.xz - KW_ShorelineAreaPos.xz) / KW_WavesMapSize + 0.5;

	//float2 depthUV = (worldPos.xz - KW_DepthPos.xz) / KW_DepthOrthographicSize + 0.5;
	//float terrainDepth = tex2Dlod(KW_OrthoDepth, float4(depthUV, 0, 0)).r * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y + KW_DepthPos.y;
	float terrainDepth = ComputeWaterOrthoDepth(worldPos);
	//_Time.y = TEST;
	
	float timeLimit = (14.0 * 15.0) / 20.0; //(frameX * frameY) / fps
	//KW_Time = Test4.x;
	float time = frac((KW_GlobalTimeSpeedMultiplier * KW_Time) / timeLimit) * timeLimit;
	time += timeOffset;

	float3 offsetWave1 = ComputeBeachWaveOffsetForOneLine(wavesMapUV, terrainDepth, time, KW_BakedWaves1_UV_Angle_Alpha, KW_BakedWaves1_TimeOffset_Scale, shorelineData.uv1, shorelineData.data1);
	float3 offsetWave2 = ComputeBeachWaveOffsetForOneLine(wavesMapUV, terrainDepth, time, KW_BakedWaves2_UV_Angle_Alpha, KW_BakedWaves2_TimeOffset_Scale, shorelineData.uv2, shorelineData.data2);
	offset.xyz += offsetWave1;
	offset.xyz += offsetWave2;
	return offset;
}


float4 KW_PlanarReflection_TexelSize;
sampler2D KW_FluidsPrebaked;
sampler2D KW_LeanTex;
float3 _MainLightPosition;
float3 _MainLightColor;

half3 SampleReflectionProbe(UNITY_ARGS_TEXCUBE(tex), half4 hdr, float3 reflDir, half mip)
{
	half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, reflDir, mip);
	return DecodeHDR(rgbm, hdr);
}

inline float3 BoxProjectedCubemap(float3 worldRefl, float3 worldPos, float3 cubemapCenter, float3 boxMin, float3 boxMax)
{
	// Do we have a valid reflection probe?
	
			float3 nrdir = normalize(worldRefl);

#if 1
			float3 rbmax = (boxMax.xyz - worldPos) / nrdir;
			float3 rbmin = (boxMin.xyz - worldPos) / nrdir;

			float3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;

#else // Optimized version
			float3 rbmax = (boxMax.xyz - worldPos);
			float3 rbmin = (boxMin.xyz - worldPos);

			float3 select = step(float3(0, 0, 0), nrdir);
			float3 rbminmax = lerp(rbmax, rbmin, select);
			rbminmax /= nrdir;
#endif

			float fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);

			worldPos -= cubemapCenter.xyz;
			worldRefl = worldPos + nrdir * fa;
			return worldRefl;
		
}
float3 KW_CubemapCenter;

half4 ComputeWaterColor(v2f i, float facing)
{
	float rawZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w));
	if (rawZ < i.screenPos.w) discard;
	//return float4((i.worldPosRefracted.y) - 3, 0, 0, 1);
	i.viewDir = normalize(i.viewDir);

	float2 uv = i.worldPos.xz / KW_FFTDomainSize;

	half3 lean = tex2D(KW_LeanTex, uv);

	//

	
	//float gloss = lean;
	//gloss /= lerp(Test4.z / Test4.x, 1.0, gloss);
	float viewDist = length(i.worldPos.xyz - _WorldSpaceCameraPos);
#if USE_FILTERING
	half3 normalBicubic = SampleBicubic(KW_NormTex, sampler_KW_NormTex, uv, KW_NormTex_TexelSize);
	half3 normal = Tex2D_AA_LQ(KW_NormTex, sampler_KW_NormTex, uv);

	half bicubicLodDist = 10 + (1 - KW_FFT_Size_Normalized) * 40;
	normal = lerp(normalBicubic, normal, saturate(viewDist / bicubicLodDist));
	
	float rlen = 1.0 / saturate(length(normal));
	float gloss = 1.0 / (1.0 + 100 * (rlen - 1.0));
	
	gloss = lerp(1, gloss, saturate(viewDist / 200));
	
#else
	half3 normal = KW_NormTex.Sample(sampler_KW_NormTex, uv);
#endif
	normal = normalize(normal);
	//return float4(gloss.xxx, 1);
	
	
	
	
#ifdef USE_MULTIPLE_SIMULATIONS
	half3 norm_lod1 = KW_NormTex_LOD1.Sample(sampler_KW_NormTex, i.worldPos.xz / KW_FFTDomainSize_LOD1);
	//norm_lod1 = Tex2D_AA_LQ(KW_NormTex_LOD1, sampler_KW_NormTex, i.worldPos.xz / KW_FFTDomainSize_LOD1);

	half3 norm_lod2 = KW_NormTex_LOD2.Sample(sampler_KW_NormTex, i.worldPos.xz / KW_FFTDomainSize_LOD2);
	//norm_lod2 = Tex2D_AA_LQ(KW_NormTex_LOD2, sampler_KW_NormTex, i.worldPos.xz / KW_FFTDomainSize_LOD2);

	normal = NormalsCombine(normal, norm_lod1, norm_lod2);
#endif


	
#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE)
	float2 flowMapUV = (i.worldPos.xz - KW_FlowMapOffset.xz) / KW_FlowMapSize + 0.5;
	float2 flowmap = KW_FlowMapTex.Sample(sampler_linear_repeat, flowMapUV) * 2 - 1;
	normal = ComputeNormalUsingFlowMap(KW_NormTex, sampler_linear_repeat, flowmap, normal, uv, KW_Time * KW_FlowMapSpeed);
#endif
#if defined(KW_FLOW_MAP_FLUIDS) && !defined(KW_FLOW_MAP_EDIT_MODE)
	float2 flowMapUV = (i.worldPos.xz - KW_FlowMapOffset.xz) / KW_FlowMapSize + 0.5;

	float2 fluidsUV_lod0 = (i.worldPos.xz - KW_FluidsMapWorldPosition_lod0.xz) / KW_FluidsMapAreaSize_lod0 + 0.5;
	float4 fluidsData_lod0 = tex2D(KW_Fluids_Lod0, fluidsUV_lod0);

	float2 depthUV = (i.worldPos.xz - KW_FluidsDepthPos.xz) / KW_FluidsDepthOrthoSize + 0.5;

	float2 fluidsUV_lod1 = (i.worldPos.xz - KW_FluidsMapWorldPosition_lod1.xz) / KW_FluidsMapAreaSize_lod1 + 0.5;
	float4 fluidsData_lod1 = tex2D(KW_Fluids_Lod1, fluidsUV_lod1);

	float2 maskUV_lod0 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
	float lodLevelFluidMask_lod0 = saturate((maskUV_lod0.x * maskUV_lod0.y - 0.01) * 5);
	float2 maskUV_lod1 = 1 - saturate(abs(fluidsUV_lod1 * 2 - 1));
	float lodLevelFluidMask_lod1 = saturate((maskUV_lod1.x * maskUV_lod1.y - 0.01) * 5);

	float3  fluids = lerp(fluidsData_lod1, fluidsData_lod0, lodLevelFluidMask_lod0);
	fluids *= lodLevelFluidMask_lod1;
	//return float4(frac(abs(fluids.xyz)), 1);
	//float2 flowmap = tex2D(KW_FlowMapTex, flowMapUV) * 2 - 1;
//	return float4((fluids.xy), 0, 1);
	normal = ComputeNormalUsingFlowMap(KW_NormTex, sampler_linear_repeat, fluids.xy * KW_FluidsVelocityAreaScale * 0.75, normal, uv, KW_Time * KW_FlowMapSpeed);
#endif



//	float3 reflectionNormal = normal;
//#if KW_DYNAMIC_WAVES
//	float3 dynamicWaves = tex2D(KW_DynamicWaves, dynamicWavesUV) * 2 - 1;
//	reflectionNormal = lerp(reflectionNormal, reflectionNormal * Test4.x, abs(dynamicWaves));
//#endif

#ifdef KW_FLOW_MAP_EDIT_MODE
	if (flowMapUV.x < 0 || flowMapUV.x > 1 || flowMapUV.y < 0 || flowMapUV.y > 1) return 0;
	return half4(pow((normal.xz + 0.75), 5), 1, 1);
#endif


#if USE_SHORELINE
	float3 shorelineWave1 = ComputeBeachWaveNormal(i.shorelineUVAnim1, i.shorelineWaveData1);
	float3 shorelineWave2 = ComputeBeachWaveNormal(i.shorelineUVAnim2, i.shorelineWaveData2);
	float terrainDepth = ComputeWaterOrthoDepth(i.worldPos);
	float shorelineNearDepthMask = saturate(terrainDepth - KW_WaterPosition.y + 0.85);

	normal = lerp(normal, float3(0, 1, 0), shorelineNearDepthMask);
	normal = NormalsCombine(normal, shorelineWave1, shorelineWave2);
	
	
#endif
	
	//normal = ComputeDerivativeNormal(i.worldPosRefracted);


	half sssMask = tex2Dlod(KW_WaterMaskScatterNormals_Blured, float4(i.screenPos.xy / i.screenPos.w, 0, 0)).y;
	
	float depthFix = dot(i.viewDir, float3(0, 1, 0));
	
	float3 normalReflection = normal;
#if KW_DYNAMIC_WAVES
	float2 dynamicWavesUV = (i.worldPos.xz - KW_DynamicWavesWorldPos.xz) / KW_DynamicWavesAreaSize + 0.5;
	float3 dynamicWavesNormals = KW_DynamicWavesNormal.Sample(sampler_linear_clamp, dynamicWavesUV) * 2 - 1;
	dynamicWavesNormals = (float3(dynamicWavesNormals.x, 1, dynamicWavesNormals.y));
	/*if (dynamicWavesUV.x > 0.99 || dynamicWavesUV.x < 0.01 || dynamicWavesUV.y > 0.99 || dynamicWavesUV.x < 0.01) return 0;
	return float4(dynamicWavesNormals.xz, 1, 1);*/
	
	normalReflection = NormalsCombine(normal, dynamicWavesNormals * float3(0.25, 1, 0.25) );
	normal = NormalsCombine(normal, dynamicWavesNormals);
#endif

#if USE_FILTERING
	normalReflection = lerp(half3(0, 1, 0), normalReflection, gloss);
#endif
	
	float3 reflVec = reflect(-i.viewDir, normalReflection);
	

	half3 reflection;
#if PLANAR_REFLECTION
	float2 refl_uv = ComputeScreenSpaceReflectionDir(normalReflection, i.viewDir);
	refl_uv.y -= KW_PlanarReflectionClipOffset;
	reflection = tex2D(KW_PlanarReflection, refl_uv);

	#if FIX_UNDERWATER_SKY_REFLECTION
		float2 bounceMaskUV = i.screenPos.xy / i.screenPos.w + normalReflection.xz * 0.5 - float2(0, KW_PlanarReflectionClipOffset);
		float reflDepth = tex2D(KW_PlanarReflectionDepth, bounceMaskUV);
		reflection = (reflVec.y < 0.1) ? tex2D(KW_PlanarReflection, bounceMaskUV).xyz : reflection;
	#endif
#else
	
	reflection = UNITY_SAMPLE_TEXCUBE(KW_ReflectionCube, reflVec);

	
#if SSPR_REFLECTION
	float2 refl_uv = ComputeScreenSpaceReflectionDir(normalReflection, i.viewDir);
	refl_uv.y -= KW_SSR_ClipOffset;
	float4 ssr = KW_ScreenSpaceReflectionTex.Sample(sampler_linear_clamp, refl_uv).xyzw;
	reflection.xyz = lerp(reflection.xyz, ssr.xyz, ssr.a);
	
#endif
	
#endif
	
	//return float4(reflection.xyz, 1);
	//depthFix = 1;
	///////////////////////////////////////////////////////////////////// REFRACTION ///////////////////////////////////////////////////////////////////
	

	//return float4(surfaceTensionFade.xxx, 1);
	float sceneZRefr = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTextureBeforeWaterZWrite_Blured, i.screenPos.xy / i.screenPos.w));
	float surfaceDepthRefr = (i.screenPos.w);
	float fadeRefr = clamp((sceneZRefr - surfaceDepthRefr) * depthFix, 0.5, min(KW_Transparent, 5));
	

	float3 refractedRay = ComputeWaterRefractRay(-i.viewDir, normal, fadeRefr * 2 * 1);
	

	float4 refractedClipPos = mul(UNITY_MATRIX_VP, float4(i.worldPosRefracted + refractedRay, 1.0));
	float4 refractionScreenPos = ComputeScreenPos(refractedClipPos);

	
#if USE_VOLUMETRIC_LIGHT
	half4 volumeScattering = tex2D(KW_VolumetricLight, refractionScreenPos.xy / refractionScreenPos.w);
#else
	half4 volumeScattering = half4(KW_AmbientColor.rgb, 1.0);
#endif
	volumeScattering = lerp(volumeScattering, 0.5, i.underwaterCubeMask);
	
	half3 refraction = ComputeRefractionWithDispersion(_CameraOpaqueTexture, i.worldPosRefracted, refractedRay);
	
	float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, refractionScreenPos));
	float surfaceDepth = (i.screenPos.w);
	float fade = (sceneZ - surfaceDepth);
	
	if (fade < 0)
	{
		//return float4(1, 0, 0, 1);
		sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
		fade = (sceneZ - surfaceDepth);

		refraction = tex2Dproj(_CameraOpaqueTexture, i.grabPos);
		volumeScattering = tex2Dproj(KW_VolumetricLight, UNITY_PROJ_COORD(i.screenPos));
	}
	
	
	fade = (sceneZ - (i.screenPos.w)) * saturate(0.3 + depthFix);

	
	float viewDistNormalized = saturate(viewDist / (KW_WaterFarDistance * 2));
	half3 underwaterColor = ComputeUnderwaterColor(refraction, volumeScattering.rgb,  fade, KW_Transparent, KW_WaterColor, KW_Turbidity, KW_TurbidityColor);

#if defined(KW_FLOW_MAP_FLUIDS) && !defined(KW_FLOW_MAP_EDIT_MODE)
	float foamMask_lod0 = tex2D(KW_FluidsFoam_Lod0, fluidsUV_lod0);
	float foamMask_lod1 = tex2D(KW_FluidsFoam_Lod1, fluidsUV_lod1);
	float foamTex_lod0 = tex2D(KW_FluidsFoamTex, i.worldPos.xz / 40  + fluidsData_lod0.xy * 0.125 + float2(-_Time.x * 1, 0));
	float foamTex_lod1 = tex2D(KW_FluidsFoamTex, i.worldPos.xz / 100 + fluidsData_lod1.xy * 0.25 + float2(-_Time.x * 1, 0));

	foamMask_lod1 = min(foamMask_lod1, (fluidsData_lod1.z - 0.5));
	float foamMask = lerp(foamMask_lod1, foamMask_lod0, lodLevelFluidMask_lod0);
	float foamTex = lerp(foamTex_lod1 * 1.5, foamTex_lod0, lodLevelFluidMask_lod0);
	foamTex *= lodLevelFluidMask_lod1;
	
	underwaterColor = lerp(underwaterColor, clamp(KW_AmbientColor.rgb * 0.25 + volumeScattering.a + volumeScattering.rgb * 0.5, 0, 1.0), foamTex * foamMask);

#endif

#if SSPR_REFLECTION
	UNITY_APPLY_FOG(i.fogCoord, underwaterColor);
#endif

#if USE_SHORELINE
	half r = 1 - saturate(dot(reflVec, float3(0, 1, 0)));
	r = r * r * r * r * r;
	reflection = lerp(reflection, max(underwaterColor, reflection) , r);
#endif
	half linearFresnel = ComputeLinearFresnel(normalReflection, i.viewDir);
	half waterFresnel = ComputeWaterFresnel(linearFresnel);
	
	half3 finalColor = lerp(underwaterColor, reflection * (1 - i.underwaterCubeMask), waterFresnel);

#if REFLECT_SUN
	finalColor += ComputeSunlight(normalReflection, i.viewDir, linearFresnel, _MainLightPosition.xyz, _MainLightColor.rgb, volumeScattering.a, viewDistNormalized, KW_Transparent);
#endif
	finalColor += ComputeSSS(sssMask, underwaterColor, volumeScattering.a, KW_Transparent);
	
#if defined(ENVIRO_FOG)
	finalColor.rgb = TransparentFog(half4(finalColor, 0), i.worldPos.xyz, uv, sceneZ);
#endif
#ifndef SSPR_REFLECTION
	UNITY_APPLY_FOG(i.fogCoord, finalColor);
#endif

	half surfaceTensionFade = saturate((rawZ - i.screenPos.w) * 10);
	return float4(finalColor, surfaceTensionFade);
}

v2fDepth vertDepth(float4 vertex : POSITION, float underwaterCubeMask : COLOR0)
{
	v2fDepth o;
	o.worldPos = mul(unity_ObjectToWorld, vertex);
	o.underwaterCubeMask = underwaterCubeMask;
	float3 waterOffset = ComputeWaterOffset(o.worldPos);

#if USE_SHORELINE
	ShorelineData shorelineData;
	float3 beachOffset = ComputeBeachWaveOffset(o.worldPos, shorelineData);
	float terrainDepth = ComputeWaterOrthoDepth(o.worldPos);
	vertex.xyz += lerp(waterOffset, 0, saturate(terrainDepth - KW_WaterPosition.y + 0.85));
	vertex.xyz += beachOffset;
#else
	vertex.xyz += waterOffset;
#endif

	o.pos = UnityObjectToClipPos(vertex);
	
	return o;
}


half4 fragDepth(v2fDepth i, float facing : VFACE) : SV_Target
{
		//FragmentOutput o;

		float2 uv = i.worldPos.xz / KW_FFTDomainSize;
		
		half3 norm = KW_NormTex.Sample(sampler_linear_repeat, uv);
		half3 normScater = KW_NormTex.SampleLevel(sampler_linear_repeat, uv, KW_NormalScattering_Lod);
		
		#ifdef USE_MULTIPLE_SIMULATIONS
			half3 normScater_lod1 = KW_NormTex_LOD1.SampleLevel(sampler_linear_repeat, i.worldPos.xz / KW_FFTDomainSize_LOD1, 2);
			half3 normScater_lod2 = KW_NormTex_LOD2.SampleLevel(sampler_linear_repeat, i.worldPos.xz / KW_FFTDomainSize_LOD2, 1);
			normScater = normalize(half3(normScater.xz + normScater_lod1.xz + normScater_lod2.xz, normScater.y * normScater_lod1.y * normScater_lod2.y)).xzy;

			half3 norm_lod1 = KW_NormTex_LOD1.Sample(sampler_linear_repeat, i.worldPos.xz / KW_FFTDomainSize_LOD1);
			half3 norm_lod2 = KW_NormTex_LOD2.Sample(sampler_linear_repeat,  i.worldPos.xz / KW_FFTDomainSize_LOD2);
			norm = normalize(half3(norm.xz + norm_lod1.xz + norm_lod2.xz, norm.y * norm_lod1.y * norm_lod2.y)).xzy;
		#endif


		float3 viewDir = (_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
		float distance = length(viewDir);
		viewDir = normalize(viewDir);
		int idx;
		half sss = 0;
		half windLimit = clamp(KW_WindSpeed - 0.25, -0.25, 1);
		windLimit -= clamp((KW_WindSpeed - 4) * 0.25, 0, 0.8);
			
			float3 lightDir = KW_DirLightForward.xyz;
			
			half zeroScattering = saturate(dot(viewDir, -(lightDir + float3(0, 1, 0))));

			float3 H = (lightDir + norm * float3(-1, 1, -1));
			float scattering = (dot(viewDir, -H));
			sss += windLimit * (scattering - zeroScattering  * 0.95);

		

		/*float mipUV = i.screenPos.xy / KW_FFTDomainSize;
		float2 dx = ddx(mipUV * KW_NormTex_TexelSize.zw);
		float2 dy = ddy(mipUV * KW_NormTex_TexelSize.zw);
		float d = max(dot(dx, dx), dot(dy, dy));
		const float rangeClamp = pow(2.0, (KW_NormMipCount - 1) * 2.0);
		d = clamp(d, 1.0, rangeClamp);
		float mipLevel = 0.5 * log2(d);
		mipLevel = floor(mipLevel);*/

		//o.dest0 =  half4(saturate(0.75 - facing * 0.25), i.pos.z, saturate(sss), 0);  //-1 back face, 1 fron face
		//o.dest1 = half4(norm, 1);
			norm.xz *= 1-i.underwaterCubeMask;
		return  half4(saturate(0.75 - facing * 0.25) , saturate(sss - 0.1), norm.xz * 0.5 + 0.5);
}

v2f ComputeVertexInterpolators(v2f o, float3 worldPos, float4 vertex : POSITION)
{
	o.pos = UnityObjectToClipPos(vertex);
	//o.viewDir = _WorldSpaceCameraPos.xyz - worldPos.xyz;
	o.viewDir = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz - vertex.xyz;
	o.worldPosRefracted = mul(unity_ObjectToWorld, vertex).xyz;
	o.screenPos = ComputeScreenPos(o.pos);
	o.grabPos = ComputeGrabScreenPos(o.pos);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

v2f ComputeShorelineVertexInterpolators(v2f o, ShorelineData shorelineData)
{
	o.shorelineUVAnim1 = shorelineData.uv1;
	o.shorelineUVAnim2 = shorelineData.uv2;
	o.shorelineWaveData1 = shorelineData.data1;
	o.shorelineWaveData2 = shorelineData.data2;

	return o;
}

v2f vert(float4 vertex : POSITION, float underwaterCubeMask : COLOR0)
{
	v2f o;
	o.worldPos = mul(unity_ObjectToWorld, vertex);

	float3 waterOffset = ComputeWaterOffset(o.worldPos);
#if USE_SHORELINE
	ShorelineData shorelineData;
	float3 beachOffset = ComputeBeachWaveOffset(o.worldPos, shorelineData);
	float terrainDepth = ComputeWaterOrthoDepth(o.worldPos);
	vertex.xyz += lerp(waterOffset, 0, saturate(terrainDepth - KW_WaterPosition.y + 0.85));
	vertex.xyz += beachOffset;
	
	o = ComputeShorelineVertexInterpolators(o, shorelineData);
#else
	vertex.xyz += waterOffset;
#endif
	o = ComputeVertexInterpolators(o, o.worldPos.xyz, vertex);
	o.underwaterCubeMask = underwaterCubeMask;
	return o;
}


half4 frag(v2f i, float facing : VFACE) : SV_Target
{
	//	float2 depthUV = (i.worldPos.xz) / KW_DepthOrthoSize * 0.5 + 0.5;
	//float4 bakedWavesPosition = tex2Dlod(KW_BakedWavesPosition, float4(depthUV, 0, 0));
	//if (bakedWavesPosition.z > 0.5) discard;
	return ComputeWaterColor(i, facing);
}


