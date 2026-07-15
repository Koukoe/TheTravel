Shader "Unlit/water_mask"
{
    Properties
    {
    }
    SubShader
    {
        // Geometry+1确保在不透明物体之后渲染
        Tags { "RenderType"="Opaque" "Queue" = "Geometry+1" }
        LOD 100

        Pass
        {
            ZWrite On // 写入深度
            ColorMask 0 // 不写入颜色

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag () : SV_Target
            {
                return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}
