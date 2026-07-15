Shader "Unlit/water_8"
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
        _Diaphaneity("水体透明度", Range(0, 1)) = 0.3
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
            float _Diaphaneity;
            
            StructuredBuffer<DiffWave> _DiffWaves;
            int _DiffWavesCount;

            float4 WaterWave(float4 vertex)
            {
                // ===== 修改处：混合8个盖斯特纳波 =====
                float3 pos = vertex.xyz;

                int waveCount = 8;
                float angle[8] = {0.0, 1.2, 2.7, 4.1, 0.8, 3.4, 5.0, 1.9};
                float freq[8] = {0.7, 1.1, 1.8, 2.3, 0.9, 1.5, 2.7, 3.2};
                float speed[8] = {1.0, 1.3, 1.7, 0.9, 1.4, 2.0, 1.1, 1.6};
                float amp[8] = {0.30, 0.25, 0.18, 0.12, 0.22, 0.14, 0.08, 0.06};
                float steep[8] = {0.6, 0.5, 0.4, 0.3, 0.5, 0.3, 0.2, 0.2};

                float timeAngle = _Time.y * 0.02;
                angle[0] += sin(timeAngle * 1.3) * 0.5;
                angle[2] += cos(timeAngle * 0.7) * 0.4;
                angle[5] += sin(timeAngle * 0.9) * 0.3;

                float3 newPos = vertex.xyz;
                for (int i = 0; i < waveCount; i++)
                {
                    float2 dir = float2(cos(angle[i]), sin(angle[i]));
                    float f = freq[i];
                    float s = speed[i];
                    float a = amp[i];
                    float st = steep[i];

                    float phase = dot(pos.xz, dir) * f + _Time.y * s * _WaveSpeed;
                    float hash = frac(sin(dot(pos.xz, float2(127.1, 311.7))) * 43758.5453);
                    phase += hash * 0.2;
                    phase /= _WaveLength;

                    float sinWave = sin(phase);
                    float cosWave = cos(phase);
                    float qi = st / (f * a * 4.0 + 0.001);
                    float weight = 1.0 / (1.0 + qi * qi);

                    newPos.x += dir.x * a * cosWave * qi * weight;
                    newPos.z += dir.y * a * cosWave * qi * weight;
                    newPos.y += a * sinWave * weight;
                }

                newPos.y = vertex.y + (newPos.y - vertex.y) * _WaveHeight;

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
                    float dist = distance(newPos.xz, fw.pos);

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
                    
                        newPos.y += sin(d * 3 / wavePower) // 波形 
                            * wavePower * inner * p * 0.4;
                    }
                }
                vertex.xyz = newPos;
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

                // 透视效果
                if (depthDiff > 0.0)
                {
                    float3 sceneColor = SampleSceneColor(screenUV);
                    
                    float transmit = exp(-_Absorption * depthDiff);
                    transmit = saturate(transmit);
                    
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
