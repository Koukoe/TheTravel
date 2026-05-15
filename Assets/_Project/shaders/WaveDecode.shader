Shader "Custom/WaveDecode"
{
    Properties
    {
        [Header(Answer Wave)]
        _AnsAmplitude ("Amplitude", Range(0.1, 2.0)) = 1.0
        _AnsFrequency ("Frequency", Range(0.5, 10.0)) = 3.0
        _AnsPhase ("Phase", Range(0.0, 6.283)) = 0.0
        _AnsOffset ("Offset Y", Range(-1.0, 1.0)) = 0.0
        
        [Header(Player Wave)]
        _PlayerAmplitude ("Amplitude", Range(0.1, 2.0)) = 1.0
        _PlayerFrequency ("Frequency", Range(0.5, 10.0)) = 3.0
        
        [Header(Appearance)]
        _AnsColor ("Answer Color", Color) = (0.2, 0.6, 1.0, 1.0)
        _PlayerColor ("Player Color", Color) = (1.0, 0.8, 0.2, 1.0)
        _MatchColor ("Match Highlight", Color) = (0.2, 1.0, 0.4, 1.0)
        _BgColor ("Background Color", Color) = (0.05, 0.05, 0.08, 1.0)
        _GridColor ("Grid Color", Color) = (0.15, 0.15, 0.2, 0.5)
        
        [Header(Decode Feedback)]
        _MatchThreshold ("Match Threshold", Range(0.01, 0.5)) = 0.15
        [HideInInspector] _MatchProgress ("Match Progress", Range(0.0, 1.0)) = 0.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Overlay" "Queue"="Overlay" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            Name "WaveDecode"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            // Answer wave (target)
            float _AnsAmplitude;
            float _AnsFrequency;
            float _AnsPhase;
            float _AnsOffset;
            
            // Player wave (adjustable)
            float _PlayerAmplitude;
            float _PlayerFrequency;
            
            // Colors
            float4 _AnsColor;
            float4 _PlayerColor;
            float4 _MatchColor;
            float4 _BgColor;
            float4 _GridColor;
            
            // Feedback
            float _MatchThreshold;
            float _MatchProgress;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            // Compute wave Y at given X (0-1 range)
            float WaveY(float x, float amp, float freq, float phase, float offset)
            {
                return amp * sin(x * freq * 6.283185 + phase) + offset;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                
                // Remap UV so the wave spans horizontally, and Y range is roughly ±1.5 visually
                float2 pos = uv;
                // Adjust for aspect ratio so wave isn't squished
                pos.x = pos.x;
                // Map Y from [0,1] to [-1.5, 1.5] for wave height visibility
                float waveY = (pos.y - 0.5) * 3.0;
                
                // Compute answer wave at this X
                float ansY = WaveY(pos.x, _AnsAmplitude, _AnsFrequency, _AnsPhase, _AnsOffset);
                
                // Compute player wave at this X
                float playerY = WaveY(pos.x, _PlayerAmplitude, _PlayerFrequency, 0.0, 0.0);
                
                // Sample wave lines with anti-aliased thickness
                float lineWidth = 0.015;
                float softness = 0.008;
                
                // Distance to answer wave line
                float distAns = abs(waveY - ansY);
                float ansLine = 1.0 - smoothstep(lineWidth - softness, lineWidth + softness, distAns);
                
                // Distance to player wave line
                float distPlayer = abs(waveY - playerY);
                float playerLine = 1.0 - smoothstep(lineWidth - softness, lineWidth + softness, distPlayer);
                
                // Compute match at each point: when both waves overlap within threshold
                float pointMatch = 1.0 - smoothstep(0.0, _MatchThreshold * 3.0, abs(ansY - playerY));
                
                // Grid lines (subtle)
                float2 grid = frac(pos * 12.0);
                float gridLine = min(
                    1.0 - smoothstep(0.0, 0.02, grid.x),
                    1.0 - smoothstep(0.0, 0.02, grid.y)
                ) * 0.3;
                // Center line
                float centerLine = 1.0 - smoothstep(0.0, 0.01, abs(pos.y - 0.5)) * 0.5;
                
                // Composite colors
                half4 col = _BgColor;
                
                // Grid overlay
                col = lerp(col, _GridColor, gridLine * _GridColor.a);
                col = lerp(col, half4(0.3, 0.3, 0.35, 1), centerLine * 0.3);
                
                // Match glow where waves overlap
                col = lerp(col, _MatchColor, pointMatch * 0.4 * _MatchColor.a);
                
                // Wave lines (player on top)
                col = lerp(col, _AnsColor, ansLine * _AnsColor.a);
                col = lerp(col, _PlayerColor, playerLine * _PlayerColor.a);
                
                // Edge glow if match is close
                float matchLine = 1.0 - smoothstep(lineWidth * 0.5, lineWidth * 2.0, abs(waveY - ansY));
                float matchArea = 1.0 - smoothstep(0.0, _MatchThreshold, abs(ansY - playerY));
                matchArea = matchArea * (1.0 - playerLine);
                col = lerp(col, _MatchColor, matchArea * 0.3);
                
                // Horizontal legend markers
                // If there's match progress, show a glow intensity
                float progressGlow = _MatchProgress;
                
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
