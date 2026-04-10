Shader "Custom/Cozy/RiverShader"
{
    Properties
    {
        _MainTex ("河流纹理", 2D) = "white" {}
        _Color ("水体颜色", Color) =  (0.2, 0.6, 0.8, 0.9)
        _FoamColor ("泡沫颜色", Color) =  (1, 1, 1, 1)
        _FlowSpeed ("流动速度", Range(0, 2)) = 0.8
        _FlowDirection ("流动方向", Range(0, 360)) = 0
        _WaveStrength ("波浪强度", Range(0, 10)) = 0.5
        _WaveSpeed ("波浪速度", Range(0, 2)) = 0.5
        _Smoothness ("平滑度", Range(0, 1)) = 0.6
        _Metallic ("金属度", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _Color;
            float4 _FoamColor;
            float _FlowSpeed;
            float _FlowDirection;
            float _WaveStrength;
            float _WaveSpeed;
            float _Smoothness;
            float _Metallic;

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

                // 简单的顶点波浪
                float3 pos = IN.positionOS.xyz;
                float wave = sin(pos.x * 1.5 + _Time.y * _WaveSpeed) * cos(pos.z * 1.5 + _Time.y * 0.7) * _WaveStrength;
                pos.y += wave;

                OUT.positionCS = TransformObjectToHClip(pos);
                OUT.positionWS = TransformObjectToWorld(pos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 计算流动 UV（沿方向滚动）
                float rad = _FlowDirection * 3.14159 / 180.0;
                float2 dir = float2(sin(rad), cos(rad));
                float2 flowUV = IN.uv + dir * _Time.y * _FlowSpeed;

                // 采样纹理
                // half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, flowUV);

                // 获取主光源
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = normalize(IN.normalWS);

                // 漫反射光照
                float NdotL = saturate(dot(normal, lightDir));
                float shadowAttenuation = 1.0;
                #if defined(_MAIN_LIGHT_SHADOWS)
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
                #endif

                float3 diffuse = (NdotL * shadowAttenuation) * mainLight.color;

                // 环境光
                float3 ambient = SampleSH(normal) * 0.5;

                // 最终颜色
                // half3 waterColor = texColor.rgb * _Color.rgb;
                // half3 finalColor = waterColor * (diffuse + ambient);

                // 添加一点泡沫效果（基于纹理的亮度）
                // float foamIntensity = saturate(1 - texColor.r * 1.5);
                // finalColor = lerp(finalColor, _FoamColor.rgb, foamIntensity * 0.5);
                // 注释掉纹理采样，直接用颜色
                half3 waterColor = _Color.rgb;
                half3 finalColor = waterColor * (diffuse + ambient);
                return half4(finalColor, _Color.a);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Unlit"
}