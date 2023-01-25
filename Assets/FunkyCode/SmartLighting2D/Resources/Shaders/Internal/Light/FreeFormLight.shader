Shader "Light2D/Internal/Light/FreeFormLight"
{
    Properties
    {
        _MainTex ("Lightmap Texture", 2D) = "white" {}
        _Sprite ("Free Form Texture?", 2D) = "white" {}

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

        // Blend SrcAlpha One
        //Blend OneMinusDstColor One // Soft additive
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
                sampler2D _Sprite;

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
                    fixed4 lightTex = tex2Dproj(_MainTex, i.texcoord);

                    float freeForm = tex2Dproj(_Sprite, i.texcoord).r;

                    float light = 2 - lightTex.r * 2;

                    float distance = lerp(1, 1 - sqrt(i.xy.x * i.xy.x + i.xy.y * i.xy.y) * 2, _Point);

                    fixed4 col = float4(1, 1, 1, 1);
                    
                    col.rgb *= light * distance * i.color.rgb * i.color.a * 2;

                    col *= freeForm;

                    return col;
                }
                
                ENDCG
            }
        }
    }
}