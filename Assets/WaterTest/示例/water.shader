Shader "Unlit/water"
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
        _DiffPower ("扩散波浪倍率", Float) = 0.6
        _DisTime ("消失时间（倍率）", Float) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Cull Off // 禁用背面剔除

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            struct DiffWave
            {
                float2 pos;
                float power;
                float startTime;
            };

            float4 _WaterColor;
            float _WaveHeight;
            float _WaveLength;
            float _WaveSpeed;
            float _FoamDistance;
            float _Absorption;
            float _DiffPower;
            float _DisTime;
            
            StructuredBuffer<DiffWave> _DiffWaves;
            int _DiffWavesCount;

            float4 WaterWave(float4 vertex)
            {
                // 混合三个正弦波，模拟海浪效果
                float waveHeight = sin((vertex.x + vertex.z) / _WaveLength + _Time.y * _WaveSpeed) * _WaveHeight;
                waveHeight += sin((vertex.x * 1 + vertex.z * 2) / _WaveLength + _Time.y * 2 * _WaveSpeed)
                                                                                            * 0.2 * _WaveHeight;
                waveHeight += sin((vertex.x * 0.1 + vertex.z * 1.2) / _WaveLength + _Time.y * 3 * _WaveSpeed)
                                                                                            * 0.1 * _WaveHeight;
                vertex.y += waveHeight;

                // 遍历C#脚本传入的落水点，计算扩散波纹
                for (int i = 0; i < _DiffWavesCount; i++)
                {
                    DiffWave fw = _DiffWaves[i];

                    // 波浪强度、速度
                    float wavePower = fw.power * _DiffPower;
                    float waveSpeed = 10;
                    // 扩散经过的时间
                    float dt = _Time.y - fw.startTime;

                    // 最外圈的波当前扩散到的距离
                    float Dist = waveSpeed * dt;
                    Dist += wavePower * 0.2; // 外扩0.1个周期，避免初始水波出现过晚
                    // 当前点距离落水点的距离
                    float dist = distance(vertex.xz, fw.pos);

                    // 在扩散范围内，才有波
                    if (dist <= Dist)
                    {
                        float d = Dist - dist;
                        
                        // 整体衰减（传递越久越弱）
                        float maxTime = wavePower * _DisTime;
                        float p = 1 - saturate(dt / maxTime);
                        
                        // 波形限制（只保留最外圈1.5个周期的波）
                        float maxRange = wavePower * 3;
                        float mp = maxRange - d;
                        float inner = step(0, mp); // 替代if，等价于 (mp >= 0) ? 1 : 0
                        inner *= 1 - d / maxRange; // 离最外圈的波越远，波越弱
                    
                        // 波动 + 衰减（波长为 2π / 3 * wavePower，近似为 2 * wavePower）
                        vertex.y += sin(d * 3 / wavePower)
                            * wavePower * inner * p * 0.4;
                    }
                }
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
                float diff = saturate(dot(normal, lightDir)) * 0.5 + 0.5; // 避免背面过暗
                float3 ambient = SampleSH(normal); // Unity环境光
                
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

                // 透视效果
                if (depthDiff > 0.0)
                {
                    // 采样水下场景颜色
                    float3 sceneColor = SampleSceneColor(screenUV);

                    // 水对光的吸收衰减（指数衰减）
                    float transmit = exp(-_Absorption * depthDiff);
                    transmit = saturate(transmit);

                    // 与水色混合（浮沫处无透视效果）
                    float3 transmittedColor = sceneColor * _WaterColor.rgb * transmit;
                    float blendWeight = transmit * (1.0 - saturate(foam));
                    
                    col.rgb = lerp(col.rgb, transmittedColor, blendWeight);
                }
                
                return col;
            }
            ENDHLSL
        }
    }
}
