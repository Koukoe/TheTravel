Shader "Custom/Cozy/OceanShader"
{
    Properties
    {
        _ShallowColor ("浅水颜色", Color) =  (0.3, 0.7, 0.9, 1)
        _DeepColor ("深水颜色", Color) =  (0.05, 0.1, 0.4, 1)
        _FoamColor ("泡沫颜色", Color) =  (1, 1, 1, 1)
        _DepthMax ("最大深度", Float) = 10
        _FoamDepth ("泡沫深度阈值", Float) = 0.5
        _RefractionStrength ("折射强度", Range(0, 0.1)) = 0.02
        _WaveSpeed ("波浪速度", Float) = 0.5
        _WaveStrength ("波浪强度", Float) = 0.1
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"


            float4 _ShallowColor;
            float4 _DeepColor;
            float4 _FoamColor;
            float _DepthMax;
            float _FoamDepth;
            float _RefractionStrength;
            float _WaveSpeed;
            float _WaveStrength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // 简单的波浪顶点动画
                float3 pos = IN.positionOS.xyz;
                float wave = sin(pos.x * 0.5 + _Time.y * _WaveSpeed) * cos(pos.z * 0.5 + _Time.y * 0.8) * _WaveStrength;
                pos.y += wave;
                OUT.positionCS = TransformObjectToHClip(pos);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 水深计算
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float depth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(depth, _ZBufferParams);
                float waterDepth = sceneDepth - IN.screenPos.w;
                float depthFactor = saturate(waterDepth / _DepthMax);
                float depthOneMinus = 1 - depthFactor;

                // 泡沫计算（岸边泡沫）
                float foamIntensity = saturate((_FoamDepth - waterDepth) / _FoamDepth);
                foamIntensity = pow(foamIntensity, 2);

                // 折射采样
                float2 refractUV = screenUV + (foamIntensity * _RefractionStrength);
                half3 refractColor = SampleSceneColor(refractUV);

                // 颜色混合
                half3 waterColor = lerp(_ShallowColor, _DeepColor, (half)depthFactor);
                half3 finalColor = lerp(refractColor, waterColor, (half)depthOneMinus);

                // 添加泡沫
                finalColor = lerp(finalColor, _FoamColor, foamIntensity);

                // 简单的环境光照
                half3 ambient = SampleSH(0);
                finalColor *= (0.5 + ambient);

                return half4(finalColor, 0.8);
            }
            ENDHLSL
        }
    }
}