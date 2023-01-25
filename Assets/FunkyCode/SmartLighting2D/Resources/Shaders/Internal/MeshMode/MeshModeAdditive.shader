Shader "Light2D/Internal/MeshModeAdditive"
{
    Properties
    {
        _Lightmap ("Lightmap", 2D) = "black" {}
        _Sprite ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Invert("Invert", Float) = 0
        _Outer("Outer", Float) = 0
        _Inner("Inner", Float) = 0
        _Rotation("Rotation", Float) = 0
        _Freeform("Freeform", 2D) = "white" {}
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

        Blend SrcAlpha One
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
                #pragma target 2.0

                #include "UnityCG.cginc"

                sampler2D _Lightmap;
                sampler2D _Sprite;
                sampler2D _Freeform;
                float4 _Color;
                float _Invert;
                float _Outer;
                float _Inner;
                float _Rotation;
                
                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    float3 xy : TEXCOORD1;        
                };
     
                v2f vert (appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = _Color;
                    o.texcoord = v.texcoord;
                    o.xy.xy = float2(v.texcoord.x - 0.5, v.texcoord.y - 0.5);
                    o.xy.z = _Inner >= 359;
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    float alpha = 1 - tex2D(_Lightmap, i.texcoord).r;
                    float4 sprite = tex2D(_Sprite, i.texcoord);
                    float4 freeForm = tex2D(_Freeform, i.texcoord);

                    float4 color = float4(1, 1, 1, 1);

                    float dir = ((atan2(i.xy.y, i.xy.x) - _Rotation) * 57.2958 + 810) % 360;

                    float distance = sqrt(i.xy.x * i.xy.x + i.xy.y * i.xy.y);
                    float pointValue = max(0, (1 - distance * 2));
                    pointValue *= lerp(max(0, min(1, (_Inner * 0.5 - abs(dir - 180) + _Outer) / _Outer)), 1, i.xy.z);

                    pointValue = pointValue, pointValue * pointValue * pointValue;

                    color.rgb *= sprite.rgb * sprite.a * i.color.rgb * i.color.a * alpha * pointValue;

                    color *= freeForm;

                    return color;
                }
                
                ENDCG
            }
        }
    }
}