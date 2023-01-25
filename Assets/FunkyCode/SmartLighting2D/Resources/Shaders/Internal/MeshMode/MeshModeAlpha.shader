
Shader "Light2D/Internal/MeshModeAlpha"
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

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend One OneMinusSrcAlpha

        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _Lightmap;
            sampler2D _Sprite;
            sampler2D _Freeform;
            float4 _Color;
            float _Invert;
            float _Outer;
            float _Inner;
            float _Rotation;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 xy : TEXCOORD1;       
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = _Color;
                o.xy.xy = float2(v.uv.x - 0.5, v.uv.y - 0.5);
                o.xy.z = _Inner >= 359;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float alpha = 1 - tex2D(_Lightmap, i.uv).r;
                float4 sprite = tex2D(_Sprite, i.uv);;
                float4 color = float4(1, 1, 1, 1);
                float4 freeForm = tex2D(_Freeform, i.texcoord);

                float dir = ((atan2(i.xy.y, i.xy.x) - _Rotation) * 57.2958 + 810) % 360;
                float distance = sqrt(i.xy.x * i.xy.x + i.xy.y * i.xy.y);
                float pointValue = max(0, (1 - distance * 2));
                pointValue *= lerp(max(0, min(1, (_Inner * 0.5 - abs(dir - 180) + _Outer) / _Outer)), 1, i.xy.z);

                pointValue = pointValue, pointValue * pointValue * pointValue;

                alpha *= pointValue;

                color.rgb *= sprite.rgb * sprite.a * alpha * i.color.a * i.color.rgb;
     
                color.a = alpha * sprite.a * sprite.r * i.color.a * i.color.rgb;

                color *= freeForm;

                return color;
            }
            ENDCG
        }
    }
}
