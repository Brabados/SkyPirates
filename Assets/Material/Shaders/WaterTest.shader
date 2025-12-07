Shader "Custom/StylizedWaterURP_JumpFlood"
{
    Properties
    {
        _BaseColor("Water Color", Color) = (0.1, 0.4, 0.7, 1)
        _DeepColor("Deep Water Color", Color) = (0, 0.1, 0.2, 1)
        _ShallowDistance("Shallow Distance", Float) = 4.0
        _RefractionStrength("Refraction Strength", Float) = 0.03
        _DistortionTex("Distortion Normal Map", 2D) = "bump" {}
        _DistortionStrength("Distortion Strength", Float) = 0.25
        _WaveAmplitude("Wave Amplitude", Float) = 0.1
        _WaveFrequency("Wave Frequency", Float) = 0.5
        _WaveSpeed("Wave Speed", Float) = 1.0
        _FoamColor("Foam Color", Color) = (1,1,1,1)
        _FoamDistance("Foam Distance", Float) = 0.08
        _FoamIntensity("Foam Intensity", Float) = 1.0
        _FoamSoftness("Foam Softness", Float) = 0.5
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
                "RenderPipeline" = "UniversalPipeline"
            }

            Pass
            {
                Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }

                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off
                Cull Back

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                #pragma target 3.0

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 positionWS : TEXCOORD1;
                    float4 screenPos : TEXCOORD2;
                };

                TEXTURE2D(_DistortionTex);
                SAMPLER(sampler_DistortionTex);

                TEXTURE2D(_WaterDistanceField);
                SAMPLER(sampler_WaterDistanceField);

                CBUFFER_START(UnityPerMaterial)
                    half4 _BaseColor;
                    half4 _DeepColor;
                    half _ShallowDistance;
                    half _RefractionStrength;
                    half _DistortionStrength;
                    half _WaveAmplitude;
                    half _WaveFrequency;
                    half _WaveSpeed;
                    half4 _FoamColor;
                    half _FoamDistance;
                    half _FoamIntensity;
                    half _FoamSoftness;
                    float4 _DistortionTex_ST;
                CBUFFER_END

                Varyings vert(Attributes input)
                {
                    Varyings output;

                    float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                    half t = _Time.y * _WaveSpeed;
                    half wave = sin(worldPos.x * _WaveFrequency + t) +
                               cos(worldPos.z * _WaveFrequency * 1.2 + t * 1.3);

                    input.positionOS.y += wave * _WaveAmplitude * 0.5;

                    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                    output.positionCS = vertexInput.positionCS;
                    output.positionWS = vertexInput.positionWS;
                    output.screenPos = ComputeScreenPos(output.positionCS);
                    output.uv = TRANSFORM_TEX(input.uv, _DistortionTex);

                    return output;
                }

                half4 frag(Varyings input) : SV_Target
                {
                    float2 screenUV = input.screenPos.xy / input.screenPos.w;

                    half2 distortion = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, input.uv).rg;
                    distortion = (distortion * 2.0 - 1.0) * _DistortionStrength;

                    float2 refractUV = screenUV + distortion * _RefractionStrength;
                    refractUV = saturate(refractUV);

                    half3 sceneColor = SampleSceneColor(refractUV);

                    Light mainLight = GetMainLight();
                    half3 lightColor = mainLight.color;

                    float rawDepth = SampleSceneDepth(screenUV);
                    float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                    float waterDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                    float depthDiff = sceneDepth - waterDepth;

                    // CRITICAL: Add depth bias to prevent jittering at very small depths
                    depthDiff = max(0, depthDiff - 0.01);

                    // Jump flood foam calculation
                    half foam = 0.0;
                    if (depthDiff > 0.0 && depthDiff < _ShallowDistance)
                    {
                        float2 seedUV = SAMPLE_TEXTURE2D(_WaterDistanceField, sampler_WaterDistanceField, screenUV).rg;

                        if (seedUV.x > -100.0)
                        {
                            float distToEdge = distance(screenUV, seedUV);
                            distToEdge *= 3.0;

                            // Smooth foam gradient
                            float foamGradient = 1.0 - saturate(distToEdge / _FoamDistance);
                            foam = smoothstep(0.0, 1.0, foamGradient);

                            // Apply softness control
                            foam = pow(foam, 2.0 - _FoamSoftness) * _FoamIntensity;

                            // Fade foam with depth to prevent it appearing in deep water
                            float depthFade = saturate(depthDiff / 0.5);
                            foam *= (1.0 - depthFade);

                            // Add noise-based detail using distortion
                            half foamNoise = distortion.x * 0.5 + 0.5;
                            foam *= lerp(0.7, 1.0, foamNoise);

                            // Animated foam sparkle
                            half foamAnim = sin(_Time.y * 3.0 + distToEdge * 20.0) * 0.15 + 0.85;
                            foam *= foamAnim;
                        }
                    }

                    // Depth-based water color
                    half depthFade = saturate(depthDiff / _ShallowDistance);
                    half4 waterColor = lerp(_BaseColor, _DeepColor, depthFade);

                    // Fresnel effect
                    half3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                    half fresnel = pow(1.0 - saturate(dot(half3(0, 1, 0), viewDir)), 4.0);

                    // Combine all effects
                    half3 finalColor = lerp(sceneColor, waterColor.rgb, 0.75);
                    finalColor += fresnel * 0.3 * lightColor;
                    finalColor = lerp(finalColor, _FoamColor.rgb, saturate(foam));

                    return half4(finalColor, waterColor.a);
                }

                ENDHLSL
            }
        }

            Fallback Off
}
