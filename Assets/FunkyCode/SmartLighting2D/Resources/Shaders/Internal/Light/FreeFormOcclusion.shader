Shader "Light2D/Internal/Light/FreeFormOcclusion"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Point ("Free Form Point", Float) = 0
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        
        Blend One One
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

                sampler2D _MainTex;
  
                float _Point;
                
                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float4 texcoord : TEXCOORD0;
                    float2 xy : TEXCOORD1;
                };

                v2f vert (appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = float4(v.texcoord.x, v.texcoord.y, 1, 1);
                    o.xy = float2(v.texcoord.x - 0.5, v.texcoord.y - 0.5);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 tex = tex2Dproj(_MainTex, i.texcoord);

                    float distance = lerp(1, 1 - sqrt(i.xy.x * i.xy.x + i.xy.y * i.xy.y) * 2, _Point);

                    fixed4 col = float4(tex.r * 2, tex.g * 2, tex.b * 2, 1 - tex.a);

                    col.rgb = col.r * distance * i.color.a * 2 * i.color;
                
                    return col;
                }

                ENDCG
            }
        }
    }
}