Shader "Custom/Cozy/LowPolyLit_Fixed"
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

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 positionWS = IN.positionWS;

                // ========== 1. 主光源计算 ==========
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
                // 修复：删除 smoothstep 和 pow，直接用线性漫反射
                float3 diffuseColor = lerp(_ShadowColor.rgb, _BaseColor.rgb, lightIntensity);
                float3 lightingResult = diffuseColor * mainLight.color;

                // ========== 2. 额外光源计算（点光源/聚光灯） ==========
                #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0; i < additionalLightsCount; i++)
                {
                    Light additionalLight = GetAdditionalLight(i, positionWS);

                    // 点光源方向（从光源指向顶点）
                    float3 lightDirAdd = normalize(additionalLight.direction);
                    float NdotLAdd = saturate(dot(normal, lightDirAdd));

                    // 点光源阴影
                    float shadowAdd = 1.0;
                    #if defined(_ADDITIONAL_LIGHT_SHADOWS)
                    shadowAdd = additionalLight.shadowAttenuation;
                    #endif

                    float intensityAdd = NdotLAdd * shadowAdd;
                    float3 diffuseAdd = lerp(_ShadowColor.rgb, _BaseColor.rgb, intensityAdd);

                    lightingResult += diffuseAdd * additionalLight.color;
                }
                #endif

                // ========== 3. 环境光 ==========
                float3 ambientColor = _AmbientStrength * _BaseColor.rgb;
                lightingResult += ambientColor;

                // ========== 4. 边缘光 ==========
                float3 viewDir = normalize(_WorldSpaceCameraPos - positionWS);
                float rim = 1.0 - saturate(dot(normal, viewDir));
                rim = pow(rim, _RimPower);
                // 边缘光强度受主光源方向影响
                float3 rimLight = _RimColor.rgb * rim * NdotL;
                lightingResult += rimLight;

                // ========== 5. 自发光 ==========
                lightingResult += _Glow;

                // ========== 6. 饱和度调整 ==========
                float gray = dot(lightingResult, float3(0.3, 0.59, 0.11));
                float3 finalColor = lerp(lightingResult, gray, 1.0 - _Saturation);

                // 钳制并输出
                finalColor = saturate(finalColor);
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Unlit"
}