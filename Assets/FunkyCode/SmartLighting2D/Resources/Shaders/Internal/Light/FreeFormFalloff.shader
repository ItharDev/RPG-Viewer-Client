Shader "Light2D/Internal/Light/FreeFormFalloff"
{
    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        BlendOp max
        Cull Off
        Lighting Off
        ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                
                #pragma vertex vert
                #pragma fragment frag
        
                #include "UnityCG.cginc"

                float _Strength;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 xy : TEXCOORD0;
                };

                v2f vert (appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.xy = float2(v.color.x - 0.5, v.color.y - 0.5);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    float color = 0;

                    if (i.color.a > 0)
                    {
                        // point
                        color = 1 - sqrt(i.xy.x * i.xy.x + i.xy.y * i.xy.y) * 2;

                        float draw = step(0, color);

                        color = lerp(color, ((1 / cos(color)) - 1) * 1.175, _Strength);

                        color *= draw;
                    }
                        else
                    {
                        // edge

                        color = i.color.r;

                        color = lerp(color, ((1 / cos(color)) - 1) * 1.175, _Strength);
                    }
                    
                    return color;
                }

                ENDCG
            }
        }
    }
}