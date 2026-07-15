Shader "Unlit/water_reflect"
{
    Properties
    {
        [Space(10)]
        _WaterColor ("水面颜色", Color) = (0.13, 0.7, 0.7, 1)
        _WaveHeight ("波峰高度(倍率)", Float) = 1
        _WaveSpeed ("水波速度(倍率)", Float) = 1
        _WaveLength ("波长(倍率)", Float) = 1
        [Space(10)]
        _FoamDistance ("浮沫距离", Range(0, 5)) = 0.3
        [Space(10)]
        _Absorption ("吸收系数", Float) = 0.5
        [Space(10)]
        [NoScaleOffset] _ReflectionTex ("反射纹理", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };
            float4 _WaterColor;
            float _WaveHeight;
            float _WaveLength;
            float _WaveSpeed;
            float _FoamDistance;
            float _Absorption;
            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);


            float4 WaterWave(float4 vertex)
            {
                float waveHeight = sin((vertex.x + vertex.z) / _WaveLength + _Time.y * _WaveSpeed) * _WaveHeight;
                waveHeight += sin((vertex.x * 1 + vertex.z * 2) / _WaveLength + _Time.y * 2 * _WaveSpeed)
                                                                                            * 0.2 * _WaveHeight;
                waveHeight += sin((vertex.x * 0.1 + vertex.z * 1.2) / _WaveLength + _Time.y * 3 * _WaveSpeed)
                                                                                            * 0.1 * _WaveHeight;
                vertex.y += waveHeight;
                return vertex;
            }
            
            v2f vert (appdata v)
            {
                v.vertex = WaterWave(v.vertex);
                v2f o;
                o.worldPos = TransformObjectToWorld(v.vertex);
                
                o.vertex = TransformObjectToHClip(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // 法线计算
                float3 normal = normalize(cross(ddy(i.worldPos), ddx(i.worldPos)));

                // 光照（使用 URP 主光源）
                float3 lightDir = normalize(GetMainLight().direction);
                float diff = dot(normal, lightDir) * 0.5 + 0.5;
                float3 ambient = SampleSH(normal);
                
                // 高光
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);
                float spec = pow(max(0, dot(normal, halfDir)), 128);
                float3 specular = spec * 0.8;
                
                half4 col = half4(_WaterColor.rgb * (diff + ambient) + specular, _WaterColor.a);

                // 白色浮沫
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float3 viewPos = TransformWorldToView(i.worldPos);
                float waterEyeDepth = -viewPos.z; // 屏幕深度
                
                float rawSceneDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth;
                // 深度计算，透视和正交分别处理
                if (unity_OrthoParams.w > 0.5) // 正交相机
                {
                    #if UNITY_REVERSED_Z
                        sceneEyeDepth = (_ProjectionParams.z - _ProjectionParams.y)
                                            * (1.0 - rawSceneDepth) + _ProjectionParams.y;
                    #else
                        sceneEyeDepth = (_ProjectionParams.z - _ProjectionParams.y)
                                            * rawSceneDepth + _ProjectionParams.y;
                    #endif
                }
                else // 透视相机
                {
                    sceneEyeDepth = LinearEyeDepth(rawSceneDepth, _ZBufferParams);
                }
                float depthDiff = sceneEyeDepth - waterEyeDepth;

                float foam = 1.0 - smoothstep(0.0, _FoamDistance, depthDiff);

                col = lerp(col, float4(1.0, 1.0, 1.0, 1.0), saturate(foam));

                
                // ===== 修改处：水面反射效果 =====
                screenUV.x = 1 - screenUV.x;
                float4 rtColor = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, screenUV);
                col = col * 0.6 + rtColor * 0.4;
                
                return col;
            }
            ENDHLSL
        }
    }
}
