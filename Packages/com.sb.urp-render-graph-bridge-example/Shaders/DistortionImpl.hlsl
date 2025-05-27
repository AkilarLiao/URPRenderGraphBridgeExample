/// <summary>
/// Author: SmallBurger Inc
/// Date: 2025/05/27
/// Desc:
/// </summary>
#ifndef DISTORTION_IMPL_INCLUDED
#define DISTORTION_IMPL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

static const float3x3 c_TheTangentMatrix =
{
    1.0, 0.0, 0.0,//Tangent
    0.0, 0.0, 1.0,//Binormal
    0.0, 1.0, 0.0 //iNormal
};

TEXTURE2D(_DistortionNormalTexture); SAMPLER(sampler_DistortionNormalTexture);

float2 _DistortionNormalTextureScale;

//x:linearStart, y:linearEnd, z:density
float3 _DistortionParams;

//x:U speed
//x:V speed
//z:normal distortion
float3 _DistortionAnimateParams;

struct VertexOutput
{
    float4 positionCS		: SV_POSITION;
    float2 texcoord			: TEXCOORD0;
    float2 normalUV			: TEXCOORD1;
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput VertexProgram(Attributes input)
{
    VertexOutput output;

#if SHADER_API_GLES
    float4 pos = input.positionOS;
    float2 uv = input.uv;
#else
    float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
    float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
#endif
    output.positionCS = pos;
    output.texcoord = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
    float2 theUVOffest = (0.0 + 2.0 + 4.0) * output.texcoord * _DistortionNormalTextureScale;
    float timeOffestValue = _Time.y * (0.0 + 4.0 + 8.0);
    float2 theUVAnimSpeed = timeOffestValue * _DistortionAnimateParams.xy;
    output.normalUV = (theUVOffest + theUVAnimSpeed) / 3.0;

    return output;
}


float3 ComputeWorldSpacePositionFromDepth(in float sceneDepthSource, in float2 positionCS2)
{
#if UNITY_REVERSED_Z    
    float sceneDepth = sceneDepthSource;
#else
    // Adjust z to match NDC for OpenGL    
    float sceneDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, sceneDepthSource);
#endif
    float2 positionSS = positionCS2 * _ScreenSize.zw;
    return ComputeWorldSpacePosition(positionSS, sceneDepth, UNITY_MATRIX_I_VP);
}

bool GetScreenWorldPosition(in half2 texcoord, in float2 positionCS2D,
    out float3 screenPositionWS)
{
    screenPositionWS = float3(0.0, 0.0, 0.0);
    float depth = SampleSceneDepth(texcoord);
    float linearDepth = Linear01Depth(depth, _ZBufferParams);
    if (linearDepth >= 0.95)
        return false;
    screenPositionWS = ComputeWorldSpacePositionFromDepth(depth, positionCS2D);
    return true;
}

half GetDistanceRatio(in float3 positionWS)
{
#ifdef PROCESS_2D_DISTANCE
    float viewDistance = length((_WorldSpaceCameraPos - positionWS).xz);
#else
    float viewDistance = length(_WorldSpaceCameraPos - positionWS);
#endif
    float clampDistance = clamp(viewDistance, _DistortionParams.x, _DistortionParams.y);
    return (clampDistance - _DistortionParams.x) * _DistortionParams.z;
}

half GetDestDistanceRatio(in half2 texcoord, in float2 positionCS2D)
{
    float3 screenPositionWS;
    half distanceRatio = 1.0;
    if (GetScreenWorldPosition(texcoord, positionCS2D.xy, screenPositionWS))
        distanceRatio = GetDistanceRatio(screenPositionWS);
    return distanceRatio;
}

half4 FragmentProgram(VertexOutput input) : SV_Target
{
    half distanceRatio = GetDestDistanceRatio(input.texcoord, input.positionCS.xy);
 
    float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DistortionNormalTexture,
        sampler_DistortionNormalTexture, input.normalUV));

    float3 normalizeNormal = normalize(mul(c_TheTangentMatrix, normalTS));
    
    float2 distortionUV = input.texcoord - distanceRatio *
        float2(normalizeNormal.z * _DistortionAnimateParams.z, 0.0);
    
    return half4(SampleSceneColor(distortionUV), 1.0);
}

#endif //DISTORTION_IMPL_INCLUDED
