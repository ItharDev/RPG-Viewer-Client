Shader "Light2D/Internal/Light/PointOcclusion"
{
    Properties
    {
        _Strength ("Strength", Float) = 0
        _Outer("Outer", Float) = 0
        _Inner("Inner", Float) = 0
        _Rotation("Rotation", Float) = 0
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

                float _Strength;
                float _Outer;
                float _Inner;
                float _Rotation;

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
                    float2 texcoord : TEXCOORD0;
                    float3 xy : TEXCOORD1;
                };

                v2f vert (appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = v.texcoord;
                    
                    o.xy.xy = float2(v.texcoord.x - 0.5, v.texcoord.y - 0.5);
                    o.xy.z = _Inner >= 359;

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    float dir = ((atan2(i.xy.y, i.xy.x) - _Rotation) * 57.2958 + 450 + 360) % 360;

                    float pointValue = max(0, (1 - sqrt(i.xy.x * i.xy.x + i.xy.y * i.xy.y) * 2));

                    pointValue *= lerp(max(0, min(1, (_Inner * 0.5 - abs(dir - 180) + _Outer) / _Outer)), 1, i.xy.z);

                    fixed4 output = fixed4(1, 1, 1, 1);

                    output.rgb *= lerp(pointValue, pointValue * pointValue * pointValue, _Strength);

                    output.rgb *= i.color * i.color.a * 2 * 2;
                
                    return output;
                }
                
                ENDCG
            }
        }
    }
}