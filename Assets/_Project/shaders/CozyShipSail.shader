Shader "Custom/CozyShipSail"
{
    Properties
    {
        _BaseColor ("基础颜色", Color) =  (0.9, 0.7, 0.8, 1)
        _ShadowColor ("阴影颜色", Color) =  (0.6, 0.5, 0.7, 1)
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
            Cull Off

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

            float4 _BaseColor;
            float4 _ShadowColor;
            float4 _RimColor;
            float _Smoothness;
            float _AmbientStrength;
            float _RimPower;
            float _Saturation;
            float _Glow;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            // 计算单个光源的贡献
            float3 CalculateLighting(float3 normal, float3 positionWS, float3 baseColor, float3 shadowColor, float smoothness)
            {
                float3 result = 0;

                // 获取主光源
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(normal, lightDir));

                // 主光源阴影
                float shadowAttenuation = 1.0;
                #if defined(_MAIN_LIGHT_SHADOWS)
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
                #endif

                float lightIntensity = NdotL * shadowAttenuation;
                float smoothIntensity = pow(smoothstep(0, 1, lightIntensity), smoothness);
                float3 diffuseColor = lerp(shadowColor, baseColor, smoothIntensity);

                result += diffuseColor * mainLight.color;

                // 获取额外光源（点光源、聚光灯等）
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0; i < additionalLightsCount; i++)
                {
                    Light additionalLight = GetAdditionalLight(i, positionWS);

                    // 计算点光源方向
                    float3 lightDirAdditional = normalize(additionalLight.direction);
                    float NdotLAdditional = saturate(dot(normal, lightDirAdditional));

                    // 点光源阴影（可选）
                    float shadowAdditional = 1.0;
                    #if defined(_ADDITIONAL_LIGHT_SHADOWS)
                    shadowAdditional = additionalLight.shadowAttenuation;
                    #endif

                    float intensityAdditional = NdotLAdditional * shadowAdditional;
                    float smoothIntensityAdditional = pow(smoothstep(0, 1, intensityAdditional), smoothness);
                    float3 diffuseColorAdditional = lerp(shadowColor, baseColor, smoothIntensityAdditional);

                    result += diffuseColorAdditional * additionalLight.color;
                }
                #endif

                return result;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                // 计算所有光源的贡献
                float3 lightingResult = CalculateLighting(normal, IN.positionWS, _BaseColor.rgb, _ShadowColor.rgb, _Smoothness);

                // 环境光
                float3 ambientColor = _AmbientStrength * _BaseColor.rgb;

                // 边缘光（使用主光源方向计算强度）
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(normal, lightDir));

                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float rim = 1 - saturate(dot(normal, viewDir));
                rim = pow(rim, _RimPower);
                float3 rimLight = _RimColor.rgb * rim * NdotL;

                // 最终颜色组合
                float3 finalColor = lightingResult;
                finalColor = lerp(finalColor, finalColor + ambientColor, _AmbientStrength);
                finalColor += rimLight;
                finalColor += _Glow;

                // 降低饱和度
                float gray = dot(finalColor, float3(0.3, 0.59, 0.11));
                finalColor = lerp(finalColor, gray, 1 - _Saturation);

                // 钳制最终颜色
                finalColor = saturate(finalColor);

                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Unlit"
}