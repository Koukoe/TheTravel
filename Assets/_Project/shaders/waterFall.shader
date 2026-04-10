Shader "Custom/Cozy/WaterfallShader_WorldPos"
{
    Properties
    {
        _MainTex ("水流纹理", 2D) = "white" {}
        _Color ("水体颜色", Color) =  (0.3, 0.7, 0.9, 0.9)
        _FoamColor ("泡沫颜色", Color) =  (1, 1, 1, 1)
        _FlowSpeed ("流动速度", Range(0, 3)) = 1.5
        _FoamIntensity ("泡沫强度", Range(0, 1)) = 0.5
        _Emission ("自发光强度", Range(0, 1)) = 0.3
        _FoamHeight ("泡沫高度 (世界单位)", Range(0, 30)) = 23
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _Color;
            float4 _FoamColor;
            float _FlowSpeed;
            float _FoamIntensity;
            float _Emission;
            float _FoamHeight;

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
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                // UV 滚动：只用于纹理采样，不用于泡沫判断
                OUT.uv = float2(IN.uv.x, IN.uv.y + _Time.y * _FlowSpeed);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 采样水流纹理
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // 基础水色
                half3 waterColor = _Color.rgb * texColor.rgb;

                // ========== 基于世界坐标的泡沫生成 ==========
                // 获取瀑布顶部的世界坐标 Y（需要手动在材质中设置，或通过脚本传入）
                // 简化方案：使用模型的最低点作为底部判断
                // 这里假设瀑布模型底部是世界坐标 Y = 0，你可以根据实际情况调整

                // 获取当前像素的世界坐标 Y 值
                float worldY = IN.positionWS.y;

                // 找到模型的底部世界坐标（需要传入，这里用简化方式）
                // 你可以在材质中设置 _WaterfallBottomY 参数，或者通过脚本计算
                // 暂时用世界坐标的小数部分模拟，实际使用时请替换为真实值
                float bottomY = _WorldSpaceCameraPos.y - 5;
                // 临时值，需要你手动调整！

                // 计算距离底部的距离
                float distToBottom = saturate((bottomY - worldY) / _FoamHeight);
                float foamStrength = pow(distToBottom, 1.5) * _FoamIntensity;

                // 可选：添加一点噪波感
                float noise = sin(IN.uv.x * 50 + _Time.y * 10) * 0.3;
                noise += cos(IN.uv.y * 80 + _Time.y * 8) * 0.3;
                noise = saturate(noise * 0.5 + 0.5);
                foamStrength = saturate(foamStrength + noise * 0.2);

                // 混合泡沫颜色
                half3 finalColor = lerp(waterColor, _FoamColor.rgb, foamStrength);

                // 添加自发光
                finalColor += _Emission * half3(0.8, 0.9, 1.0);

                float alpha = _Color.a * texColor.a;
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Unlit"
}