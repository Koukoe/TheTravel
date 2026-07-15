Shader "Unlit/折射"
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
        [Space(10)]
        _RefractionStrength ("折射强度", Float) = 0.1
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
            float _RefractionStrength;
            
            StructuredBuffer<DiffWave> _DiffWaves;
            int _DiffWavesCount;

            float4 WaterWave(float4 vertex)
            {
                float waveHeight = sin((vertex.x + vertex.z) / _WaveLength + _Time.y * _WaveSpeed) * _WaveHeight;
                waveHeight += sin((vertex.x * 1 + vertex.z * 2) / _WaveLength + _Time.y * 2 * _WaveSpeed)
                                                                                            * 0.2 * _WaveHeight;
                waveHeight += sin((vertex.x * 0.1 + vertex.z * 1.2) / _WaveLength + _Time.y * 3 * _WaveSpeed)
                                                                                            * 0.1 * _WaveHeight;
                vertex.y += waveHeight;

                // 扩散波
                for (int i = 0; i < _DiffWavesCount; i++)
                {
                    DiffWave fw = _DiffWaves[i];

                    // 波浪强度
                    float wavePower = fw.power * _DiffPower;
                    float waveSpeed = 10;
                    float dt = _Time.y - fw.startTime;

                    float Dist = waveSpeed * dt;
                    Dist += wavePower * 0.2; // 外扩0.1个周期，避免初始水波出现过晚
                    float dist = distance(vertex.xz, fw.pos);

                    if (dist <= Dist)
                    {
                        float d = Dist - dist;
                        
                        // 整体衰减（传递越久越弱）
                        float maxTime = wavePower * _DisTime;
                        float p = 1 - saturate(dt / maxTime);
                        
                        // 波形限制（只保留最外圈1.5个周期的波）
                        float maxRange = wavePower * 3;
                        float mp = maxRange - d;
                        float inner = step(0, mp);
                        inner *= 1 - d / maxRange; // 中心衰减
                    
                        vertex.y += sin(d * 3 / wavePower) // 波形 
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

                // ===== 修改处：折射效果 =====
                float2 offset = normal.xz * depthDiff * 0.05;
                float mp = 0.01;
                offset.x = clamp(offset.x , -mp, mp);
                offset.y = clamp(offset.y , -mp, mp);
                float2 refractedUV = screenUV + offset / sceneEyeDepth * _RefractionStrength;
                // 避免采样到屏幕外
                refractedUV.x = saturate(refractedUV.x);
                refractedUV.y = saturate(refractedUV.y);
                float3 sceneColor = SampleSceneColor(refractedUV);

                // 偏折后的深度
                rawSceneDepth = SampleSceneDepth(refractedUV);
                sceneEyeDepth = LinearEyeDepth(rawSceneDepth, _ZBufferParams);
                float refDepthDiff = sceneEyeDepth - waterEyeDepth;
                float transmit = 0;
                if (refDepthDiff > 0)
                {
                    // 计算水对光的吸收衰减（指数衰减）
                    transmit = exp(-_Absorption * refDepthDiff);
                }
                else
                {
                    // 偏折到岸上，回退到显示水色
                    sceneColor = SampleSceneColor(refractedUV);
                    // 计算水对光的吸收衰减（指数衰减）
                    transmit = exp(-_Absorption * depthDiff);
                    
                }
                transmit = saturate(transmit);
                    
                // 透射颜色 = 场景颜色 × 水色 × 衰减因子
                float3 transmittedColor = sceneColor * _WaterColor.rgb * transmit;
                
                // 混合权重：透射强度 × (1 - 泡沫遮罩)，泡沫区域不显示透射
                float blendWeight = transmit * (1.0 - saturate(foam));
                
                // 将透射颜色与水面颜色混合
                col.rgb = lerp(col.rgb, transmittedColor, blendWeight);
                
                return col;
            }
            ENDHLSL
        }
    }
}
