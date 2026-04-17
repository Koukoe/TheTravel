Shader "Custom/FountainWater"
{
    Properties
    {
        _Color ("水面颜色", Color) =  (0.2, 0.4, 0.8, 1)
        _RippleStrength ("波纹强度", Range(0, 1)) = 0.12
        _RippleSpeed ("波纹速度", Range(0, 10)) = 3
        _RippleRadius ("波纹半径", Range(0, 5)) = 2
        _RippleFrequency ("波纹频率", Range(5, 20)) = 12
        _Gloss ("光滑度", Range(0, 2)) = 0.8
        _Specular ("高光强度", Range(0, 5)) = 1.5 // 增加范围，默认1.5
        _RippleSpecular ("波纹高光强度", Range(0, 100)) = 3 // 增加默认值
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldpos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldViewDir : TEXCOORD3;
                float rippleIntensity : TEXCOORD4;
            };

            float4 _Color;
            float _RippleStrength;
            float _RippleSpeed;
            float _RippleRadius;
            float _RippleFrequency;
            float _Gloss;
            float _Specular;
            float _RippleSpecular;

            //C#传参
            float3 _RippleCenter;
            float _RippleTime;
            int _RippleOn;

            v2f vert(appdata v)
            {
                v2f o;

                // 先计算原始世界坐标
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);

                // 计算涟漪偏移
                float2 dir = worldPos.xz - _RippleCenter.xz;
                float dist = length(dir);
                float rippleValue = 0;
                float offset = 0;

                if (_RippleOn == 1 && dist < _RippleRadius)
                {
                    float falloff = 1 - dist / _RippleRadius;
                    rippleValue = sin(dist * _RippleFrequency - _RippleTime * _RippleSpeed);
                    offset = rippleValue * falloff * _RippleStrength;
                    worldPos.y += offset;
                }

                // 存储波纹强度（放大后用于高光）
                o.rippleIntensity = saturate(abs(rippleValue) * 2.0);
                // 放大波纹强度

                // 计算偏移后的法线
                float3 modifiedNormal = v.normal;
                if (_RippleOn == 1 && dist < _RippleRadius)
                {
                    float delta = 0.05;
                    // 减小采样步长
                    float2 dirX = (worldPos.xz + float2(delta, 0)) - _RippleCenter.xz;
                    float2 dirZ = (worldPos.xz + float2(0, delta)) - _RippleCenter.xz;

                    float distX = length(dirX);
                    float distZ = length(dirZ);

                    float heightX = 0, heightZ = 0;
                    if (distX < _RippleRadius)
                    {
                        float falloffX = 1 - distX / _RippleRadius;
                        float rippleX = sin(distX * _RippleFrequency - _RippleTime * _RippleSpeed);
                        heightX = rippleX * falloffX * _RippleStrength;
                    }
                    if (distZ < _RippleRadius)
                    {
                        float falloffZ = 1 - distZ / _RippleRadius;
                        float rippleZ = sin(distZ * _RippleFrequency - _RippleTime * _RippleSpeed);
                        heightZ = rippleZ * falloffZ * _RippleStrength;
                    }

                    float3 slope = float3(heightX - offset, delta, heightZ - offset);
                    float3 bumpNormal = normalize(float3(-slope.x * 2.0, 1, -slope.z * 2.0));
                    // 增强法线扰动
                    modifiedNormal = normalize(modifiedNormal + bumpNormal * 0.8);
                    // 增加法线影响
                }

                o.worldpos = worldPos;
                o.vertex = TransformWorldToHClip(worldPos);
                o.worldNormal = TransformObjectToWorldNormal(modifiedNormal);
                o.worldViewDir = normalize(_WorldSpaceCameraPos - worldPos);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // 获取主光源
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;

                // 法线
                float3 normal = normalize(i.worldNormal);

                // 漫反射
                float NdotL = saturate(dot(normal, lightDir));
                float3 diffuse = _Color.rgb * lightColor * NdotL;

                // 环境光
                float3 ambient = SampleSH(normal) * _Color.rgb * 0.3;

                // 高光
                float3 viewDir = normalize(i.worldViewDir);
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normal, halfDir));

                // 基础高光（不再削弱）
                float baseSpecular = pow(NdotH, _Gloss * 100) * _Specular;

                // 波纹高光增强（放大强度）
                float rippleSpecularBoost = pow(NdotH, _Gloss * 50) * _RippleSpecular * i.rippleIntensity;

                float specular = baseSpecular + rippleSpecularBoost;

                // 最终颜色
                float3 finalColor = diffuse + ambient + specular;


                return half4(finalColor, _Color.a);
            }
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