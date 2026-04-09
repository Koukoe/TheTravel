Shader "Custom/Cozy/LowPolyLit_Textured"
{
    Properties
    {
        _MainTex ("基础纹理", 2D) = "white" {}
        _ShadowIntensity ("阴影强度", Range(0, 1)) = 0.4
        _Smoothness ("光照平滑度", Range(0, 1)) = 0.6
        _AmbientStrength ("环境光强度", Range(0, 1)) = 0.3
        _RimColor ("边缘光颜色", Color) =  (1, 0.85, 0.7, 1)
        _RimPower ("边缘光强度", Range(0, 3)) = 1.2
        _Saturation ("饱和度", Range(0, 1)) = 0.7
        _Glow ("自发光", Range(0, 0.3)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 多光源编译指令
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float _ShadowIntensity;
            float _Smoothness;
            float _AmbientStrength;
            float4 _RimColor;
            float _RimPower;
            float _Saturation;
            float _Glow;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. 采样纹理
                float3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;

                // 2. 基于亮度的阴影颜色（纹理本身的暗部作为阴影基础）
                float brightness = (albedo.r + albedo.g + albedo.b) / 3.0;
                float3 shadowColor = albedo * (1.0 - _ShadowIntensity);

                float3 normal = normalize(IN.normalWS);

                // 3. 获取主光源
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(normal, lightDir));

                // 4. 主光源阴影
                float shadowAttenuation = 1.0;
                #if defined(_MAIN_LIGHT_SHADOWS)
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
                #endif

                float lightIntensity = NdotL * shadowAttenuation;
                float smoothIntensity = pow(smoothstep(0, 1, lightIntensity), _Smoothness);
                float3 diffuseColor = lerp(shadowColor, albedo, smoothIntensity);

                float3 result = diffuseColor * mainLight.color;

                // 5. 额外光源（点光源、聚光灯等）
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0; i < additionalLightsCount; i++)
                {
                    Light additionalLight = GetAdditionalLight(i, IN.positionWS);
                    float3 lightDirAdditional = normalize(additionalLight.direction);
                    float NdotLAdditional = saturate(dot(normal, lightDirAdditional));

                    float shadowAdditional = 1.0;
                    #if defined(_ADDITIONAL_LIGHT_SHADOWS)
                    shadowAdditional = additionalLight.shadowAttenuation;
                    #endif

                    float intensityAdditional = NdotLAdditional * shadowAdditional;
                    float smoothIntensityAdditional = pow(smoothstep(0, 1, intensityAdditional), _Smoothness);
                    float3 diffuseColorAdditional = lerp(shadowColor, albedo, smoothIntensityAdditional);

                    result += diffuseColorAdditional * additionalLight.color;
                }
                #endif

                // 6. 环境光
                float3 ambientColor = _AmbientStrength * albedo;

                // 7. 边缘光
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float rim = 1 - saturate(dot(normal, viewDir));
                rim = pow(rim, _RimPower);
                float3 rimLight = _RimColor.rgb * rim * NdotL;

                // 8. 组合最终颜色
                float3 finalColor = result;
                finalColor = lerp(finalColor, finalColor + ambientColor, _AmbientStrength);
                finalColor += rimLight;
                finalColor += _Glow;

                // 9. 饱和度调整
                float gray = dot(finalColor, float3(0.3, 0.59, 0.11));
                finalColor = lerp(finalColor, gray, 1 - _Saturation);

                finalColor = saturate(finalColor);

                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Unlit"
}