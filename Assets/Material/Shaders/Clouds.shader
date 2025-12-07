Shader "URP/VolumetricCloud"
{
    Properties
    {
        _NoiseTex("3D Noise Texture", 3D) = "white" {}
        _DetailNoiseTex("Detail Noise", 3D) = "white" {}

        [Header(Cloud Appearance)]
        _CloudColor("Cloud Bright Color", Color) = (1, 1, 1, 1)
        _CloudMidColor("Cloud Mid Color", Color) = (0.85, 0.88, 0.92, 1)
        _CloudDarkColor("Cloud Shadow Color", Color) = (0.5, 0.55, 0.65, 1)

        [Header(Density)]
        _DensityMultiplier("Density Multiplier", Range(0, 20)) = 5.0
        _DensityOffset("Density Offset", Range(-1, 1)) = 0.0
        _Coverage("Coverage", Range(0, 1)) = 0.5
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.01

        [Header(Shape)]
        _NoiseScale("Noise Scale", Float) = 1.5
        _DetailScale("Detail Scale", Float) = 4.0
        _DetailStrength("Detail Strength", Range(0, 1)) = 0.15
        _EdgeSoftness("Edge Softness", Range(0.1, 1.0)) = 0.4

        [Header(Animation)]
        _WindSpeed("Wind Speed", Vector) = (0.02, 0.01, 0.015, 0)

        [Header(Stylized Lighting)]
        _ShadowSteps("Shadow Steps", Range(1, 5)) = 3
        _ShadowSoftness("Shadow Softness", Range(0, 1)) = 0.3
        _RimLightStrength("Rim Light", Range(0, 2)) = 0.8
        _RimLightPower("Rim Sharpness", Range(1, 10)) = 3.0
        _CoreDarkening("Core Darkening", Range(0, 1)) = 0.4
        _Brightness("Brightness", Range(0, 3)) = 1.2

        [Header(Raymarching)]
        _MaxSteps("Max Steps", Range(32, 256)) = 96
        _StepSize("Step Size", Range(0.005, 0.1)) = 0.03
        _LightSteps("Light Steps", Range(3, 12)) = 5

            ////// ADDED FOR DISTANCE FADE
            [Header(Fade Out With Distance)]
            _FadeStart("Fade Start Distance", Float) = 450
            _FadeEnd("Fade End Distance", Float) = 600
    }

        SubShader
            {
                Tags
                {
                    "RenderType" = "TransparentCutout"
                    "Queue" = "AlphaTest"
                    "RenderPipeline" = "UniversalPipeline"
                }

                Pass
                {
                    Name "CloudVolume"
                    Tags { "LightMode" = "UniversalForward" }

                    Blend SrcAlpha OneMinusSrcAlpha
                    ZWrite On
                    ZTest LEqual
                    Cull Front

                    HLSLPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma multi_compile_fog
                    #pragma multi_compile_instancing
                    #pragma target 3.5

                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                    struct Attributes
                    {
                        float4 positionOS : POSITION;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct Varyings
                    {
                        float4 positionCS : SV_POSITION;
                        float3 positionWS : TEXCOORD0;
                        float3 objectCenter : TEXCOORD1;
                        float3 objectScale : TEXCOORD2;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    TEXTURE3D(_NoiseTex);
                    SAMPLER(sampler_NoiseTex);
                    TEXTURE3D(_DetailNoiseTex);
                    SAMPLER(sampler_DetailNoiseTex);

                    CBUFFER_START(UnityPerMaterial)
                        float4 _CloudColor;
                        float4 _CloudMidColor;
                        float4 _CloudDarkColor;
                        float4 _WindSpeed;
                        float _DensityMultiplier;
                        float _DensityOffset;
                        float _Coverage;
                        float _AlphaCutoff;
                        float _NoiseScale;
                        float _DetailScale;
                        float _DetailStrength;
                        float _EdgeSoftness;
                        float _ShadowSteps;
                        float _ShadowSoftness;
                        float _RimLightStrength;
                        float _RimLightPower;
                        float _CoreDarkening;
                        float _Brightness;
                        float _MaxSteps;
                        float _StepSize;
                        float _LightSteps;

                        ////// ADDED FOR DISTANCE FADE
                        float _FadeStart;
                        float _FadeEnd;
                    CBUFFER_END

                    Varyings vert(Attributes input)
                    {
                        Varyings output;
                        UNITY_SETUP_INSTANCE_ID(input);
                        UNITY_TRANSFER_INSTANCE_ID(input, output);

                        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                        output.positionCS = vertexInput.positionCS;
                        output.positionWS = vertexInput.positionWS;
                        output.objectCenter = TransformObjectToWorld(float3(0, 0, 0));

                        output.objectScale = float3(
                            length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                            length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                            length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z))
                        );

                        return output;
                    }

                    float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
                    {
                        return newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
                    }

                    bool RayEllipsoidIntersection(float3 rayOrigin, float3 rayDir, float3 center, float3 radii, out float tNear, out float tFar)
                    {
                        float3 oc = (rayOrigin - center) / radii;
                        float3 rd = rayDir / radii;

                        float a = dot(rd, rd);
                        float b = 2.0 * dot(oc, rd);
                        float c = dot(oc, oc) - 1.0;

                        float discriminant = b * b - 4.0 * a * c;
                        if (discriminant < 0.0)
                            return false;

                        float sqrtDisc = sqrt(discriminant);
                        tNear = (-b - sqrtDisc) / (2.0 * a);
                        tFar = (-b + sqrtDisc) / (2.0 * a);

                        return true;
                    }

                    float SampleDensity(float3 worldPos, float3 center, float3 scale)
                    {
                        float3 objectPos = (worldPos - center);
                        float3 normalizedPos = float3(
                            objectPos.x / scale.x,
                            objectPos.y / scale.y,
                            objectPos.z / scale.z
                        );

                        float distFromCenter = length(normalizedPos);
                        if (distFromCenter > 0.5) return 0.0;

                        float sphereDensity = 1.0 - smoothstep(0.5 - _EdgeSoftness, 0.5, distFromCenter);

                        float heightGradient = saturate(Remap(normalizedPos.y, -0.5, 0.5, 0.8, 1.2));
                        sphereDensity *= heightGradient;

                        float3 windOffset = _WindSpeed.xyz * _Time.y;
                        float3 samplePos = (normalizedPos + windOffset) * _NoiseScale;
                        float baseNoise = SAMPLE_TEXTURE3D_LOD(_NoiseTex, sampler_NoiseTex, samplePos + 0.5, 0).r;

                        float baseCloudWithCoverage = Remap(baseNoise, 1.0 - _Coverage, 1.0, 0.0, 1.0);
                        baseCloudWithCoverage = saturate(baseCloudWithCoverage);
                        if (baseCloudWithCoverage < 0.01) return 0.0;

                        float3 detailPos = (normalizedPos + windOffset * 1.3) * _DetailScale;
                        float detail = SAMPLE_TEXTURE3D_LOD(_DetailNoiseTex, sampler_DetailNoiseTex, detailPos + 0.5, 0).r;

                        float detailErosion = detail * (1.0 - baseCloudWithCoverage);
                        float finalCloud = baseCloudWithCoverage - detailErosion * _DetailStrength;

                        float density = finalCloud * sphereDensity;
                        density = saturate(density + _DensityOffset);
                        density *= _DensityMultiplier;

                        return max(0.0, density);
                    }

                    float StylizedLightEnergy(float3 pos, float3 center, float3 scale, float3 rayDir)
                    {
                        Light mainLight = GetMainLight();
                        float3 lightDir = mainLight.direction;

                        float totalDensity = 0.0;
                        float3 rayPos = pos;
                        float avgScale = (scale.x + scale.y + scale.z) / 3.0;
                        float stepSize = _StepSize * avgScale * 2.5;

                        for (int i = 0; i < (int)_LightSteps; i++)
                        {
                            rayPos += lightDir * stepSize;
                            float density = SampleDensity(rayPos, center, scale);
                            totalDensity += density * stepSize;
                        }

                        float shadow = 1.0 - saturate(totalDensity * 0.3);
                        float steps = _ShadowSteps;
                        shadow = floor(shadow * steps) / steps;
                        shadow = lerp(shadow, saturate(1.0 - totalDensity * 0.3), _ShadowSoftness);

                        return saturate(shadow);
                    }

                    struct FragOutput
                    {
                        half4 color : SV_Target;
                        float depth : SV_Depth;
                    };

                    FragOutput frag(Varyings input)
                    {
                        FragOutput output;
                        UNITY_SETUP_INSTANCE_ID(input);

                        float3 rayOrigin = _WorldSpaceCameraPos;
                        float3 rayDir = normalize(input.positionWS - rayOrigin);
                        float3 center = input.objectCenter;
                        float3 radii = input.objectScale * 0.5;

                        float tStart, tEnd;
                        if (!RayEllipsoidIntersection(rayOrigin, rayDir, center, radii, tStart, tEnd))
                            discard;

                        tStart = max(0.0, tStart);
                        if (tEnd < 0.0) discard;

                        float avgScale = (radii.x + radii.y + radii.z) / 1.5;
                        float stepSize = _StepSize * avgScale;
                        int numSteps = min((int)_MaxSteps, (int)((tEnd - tStart) / stepSize) + 1);

                        float transmittance = 1.0;
                        float3 lightAccumulation = float3(0, 0, 0);
                        float t = tStart;
                        float firstDenseHit = tEnd;
                        bool foundDensity = false;
                        float accumulatedDensity = 0.0;

                        Light mainLight = GetMainLight();
                        float3 lightDir = mainLight.direction;

                        float blueNoise = frac(sin(dot(input.positionCS.xy, float2(12.9898, 78.233))) * 43758.5453);
                        t += stepSize * blueNoise * 0.5;

                        for (int i = 0; i < numSteps; i++)
                        {
                            float3 pos = rayOrigin + rayDir * t;
                            float density = SampleDensity(pos, center, input.objectScale);

                            if (density > 0.001)
                            {
                                if (!foundDensity)
                                {
                                    firstDenseHit = t;
                                    foundDensity = true;
                                }

                                accumulatedDensity += density * stepSize;

                                float lightValue = StylizedLightEnergy(pos, center, input.objectScale, rayDir);

                                float3 toCamera = -rayDir;
                                float rimDot = saturate(dot(toCamera, lightDir));
                                float rimLight = pow(rimDot, _RimLightPower) * _RimLightStrength;

                                float coreDarkness = saturate(accumulatedDensity * _CoreDarkening);
                                float finalLight = saturate((lightValue + rimLight) * (1.0 - coreDarkness * 0.5));

                                float3 cloudColor;
                                if (finalLight > 0.66)
                                {
                                    cloudColor = lerp(_CloudMidColor.rgb, _CloudColor.rgb, (finalLight - 0.66) * 3.0);
                                }
                                else if (finalLight > 0.33)
                                {
                                    cloudColor = lerp(_CloudDarkColor.rgb, _CloudMidColor.rgb, (finalLight - 0.33) * 3.0);
                                }
                                else
                                {
                                    cloudColor = lerp(_CloudDarkColor.rgb * 0.7, _CloudDarkColor.rgb, finalLight * 3.0);
                                }

                                float densityStep = density * stepSize;
                                lightAccumulation += cloudColor * densityStep * transmittance * _Brightness;
                                transmittance *= exp(-densityStep * 1.2);

                                if (transmittance < 0.005)
                                {
                                    transmittance = 0.0;
                                    break;
                                }
                            }

                            t += stepSize;
                        }

                        float alpha = 1.0 - transmittance;
                        if (alpha < _AlphaCutoff) discard;

                        float dist = distance(_WorldSpaceCameraPos, center);
                        float fade = saturate(1.0 - (dist - _FadeStart) / (_FadeEnd - _FadeStart));

                        alpha *= fade;
                        lightAccumulation *= fade;  // optional but looks better

                        if (alpha < _AlphaCutoff) discard;

                        float3 depthPos = rayOrigin + rayDir * firstDenseHit;
                        float4 clipPos = TransformWorldToHClip(depthPos);
                        output.depth = clipPos.z / clipPos.w;

                        output.color = half4(lightAccumulation, alpha);
                        return output;
                    }
                    ENDHLSL
                }
            }

                Fallback Off
}
