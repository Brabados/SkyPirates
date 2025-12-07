Shader "Custom/JumpFlooding"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off ZTest Always Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        TEXTURE2D(_SourceTex);
        SAMPLER(sampler_SourceTex);
        int _StepSize;
        float4 _SourceTex_TexelSize;

        Varyings VertexSimple(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.uv = input.uv;
            return output;
        }
        ENDHLSL

            // Pass 0: Seed - Detect depth discontinuities
            Pass
            {
                Name "Seed"

                HLSLPROGRAM
                #pragma vertex VertexSimple
                #pragma fragment FragSeed

                float2 FragSeed(Varyings input) : SV_Target
                {
                    float2 uv = input.uv;

                    // Sample scene depth
                    float sceneDepth = SampleSceneDepth(uv);

                    // Check if this is a valid depth (not skybox/far plane)
                    if (sceneDepth >= 0.9999)
                    {
                        return float2(-999.0, -999.0); // No geometry here
                    }

                    // Convert to linear depth
                    float sceneLinear = LinearEyeDepth(sceneDepth, _ZBufferParams);

                    // Sample neighboring depths in a cross pattern
                    float2 texelSize = _SourceTex_TexelSize.xy;

                    float rightDepth = SampleSceneDepth(uv + float2(texelSize.x, 0));
                    float leftDepth = SampleSceneDepth(uv + float2(-texelSize.x, 0));
                    float upDepth = SampleSceneDepth(uv + float2(0, texelSize.y));
                    float downDepth = SampleSceneDepth(uv + float2(0, -texelSize.y));

                    // Convert to linear
                    float rightLinear = LinearEyeDepth(rightDepth, _ZBufferParams);
                    float leftLinear = LinearEyeDepth(leftDepth, _ZBufferParams);
                    float upLinear = LinearEyeDepth(upDepth, _ZBufferParams);
                    float downLinear = LinearEyeDepth(downDepth, _ZBufferParams);

                    // Calculate depth differences
                    float diffRight = abs(sceneLinear - rightLinear);
                    float diffLeft = abs(sceneLinear - leftLinear);
                    float diffUp = abs(sceneLinear - upLinear);
                    float diffDown = abs(sceneLinear - downLinear);

                    float maxDiff = max(max(diffRight, diffLeft), max(diffUp, diffDown));

                    // Only detect significant depth discontinuities (object edges)
                    float depthThreshold = 0.15;
                    bool isEdge = maxDiff > depthThreshold;

                    // Mark as edge if this is a depth discontinuity
                    if (isEdge)
                    {
                        return uv;
                    }
                    else
                    {
                        return float2(-999.0, -999.0);
                    }
                }
                ENDHLSL
            }

            // Pass 1: Jump Flooding
            Pass
            {
                Name "JumpFlood"

                HLSLPROGRAM
                #pragma vertex VertexSimple
                #pragma fragment FragJumpFlood

                float2 FragJumpFlood(Varyings input) : SV_Target
                {
                    float2 uv = input.uv;
                    float2 bestSeed = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv).rg;

                    // Check if current pixel has invalid seed
                    bool currentInvalid = bestSeed.x < -100.0;
                    float bestDist = currentInvalid ? 9999.0 : distance(uv, bestSeed);

                    // Check 8 neighbors at _StepSize distance
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            if (x == 0 && y == 0) continue;

                            float2 offset = float2(x, y) * _StepSize * _SourceTex_TexelSize.xy;
                            float2 sampleUV = uv + offset;

                            // Check bounds
                            if (sampleUV.x < 0 || sampleUV.x > 1 || sampleUV.y < 0 || sampleUV.y > 1)
                                continue;

                            float2 seedUV = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, sampleUV).rg;

                            // Skip invalid seeds
                            if (seedUV.x < -100.0) continue;

                            float dist = distance(uv, seedUV);

                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestSeed = seedUV;
                            }
                        }
                    }

                    return bestSeed;
                }
                ENDHLSL
            }
    }
}
