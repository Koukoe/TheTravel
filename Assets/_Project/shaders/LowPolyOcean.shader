Shader "Custom/LowPolyOcean"
{
    Properties
    {
        _Color ("水颜色", Color) =  (0.2, 0.5, 0.8, 1)
        _ShallowColor ("浅水颜色", Color) =  (0.3, 0.7, 0.9, 1)
        _HeightTex ("高度纹理", 2D) = "white" {}
        _HeightScale ("波浪强度", Range(0, 5)) = 0.15
        _HeightSpeed ("波浪速度", Range(0, 2)) = 0.5
        _Opacity ("透明度", Range(0, 1)) = 0.8
        _Smoothness ("光滑度", Range(0, 1)) = 0.6
        _Specular ("高光强度", Range(0, 2)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _Color;
            float4 _ShallowColor;
            float _HeightScale;
            float _HeightSpeed;
            float _Opacity;
            float _Smoothness;
            float _Specular;

            sampler2D _HeightTex;
            float4 _HeightTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 color : COLOR;
            };

            // 简单噪声函数，用于法线计算
            float getHeight(float3 pos, float2 uv, float time)
            {
                float2 movingUV = uv * _HeightTex_ST.xy + _HeightTex_ST.zw;
                movingUV += float2(time * 0.3, time * 0.2);
                float height = tex2Dlod(_HeightTex, float4(movingUV, 0, 0)).r;
                return height * _HeightScale;
            }

            v2f vert(appdata v)
            {
                v2f o;

                float3 pos = v.vertex.xyz;
                float time = _Time.y * _HeightSpeed;
                float2 uv = v.uv;

                // 计算当前点高度
                float height = getHeight(pos, uv, time);
                pos.y += height;

                // 计算世界坐标
                float3 worldPos = mul(unity_ObjectToWorld, float4(pos, 1)).xyz;
                o.worldPos = worldPos;

                // 计算法线
                o.worldNormal = TransformObjectToWorldNormal(v.normal);

                o.vertex = TransformWorldToHClip(worldPos);

                // 采样高度纹理（需要重新采样，因为之前采样的值没有传递）
                float2 movingUV = uv * _HeightTex_ST.xy + _HeightTex_ST.zw;
                movingUV += float2(time * 0.3, time * 0.2);
                float heightValue = tex2Dlod(_HeightTex, float4(movingUV, 0, 0)).r;

                // 根据纹理值决定颜色（波峰深蓝，波谷浅蓝）
                float t = heightValue;
                // 直接使用纹理值 0-1
                float3 baseColor = lerp(_ShallowColor.rgb, _Color.rgb, t);
                o.color = float4(baseColor, _Opacity);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // 获取主光源
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;

                float3 normal = normalize(i.worldNormal);

                // 漫反射
                float NdotL = saturate(dot(normal, lightDir));
                float3 diffuse = i.color.rgb * lightColor * NdotL;

                // 环境光
                float3 ambient = SampleSH(normal) * i.color.rgb * 0.5;

                // 高光（Blinn-Phong）
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normal, halfDir));
                float specular = pow(NdotH, _Smoothness * 100) * _Specular * lightColor;

                // 菲涅尔效果（边缘光，让水更有质感）
                float fresnel = pow(1 - abs(dot(normal, viewDir)), 0.5);
                specular += fresnel * 0.3;

                float3 finalColor = diffuse + ambient + specular;

                return half4(finalColor, i.color.a);
            }
            // half4 frag(v2f i) : SV_TARGET
            // {
            //     return half4(0.2, 0.5, 0.8, _Opacity);
            // }
            ENDHLSL
        }

        // 阴影投射通道
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}