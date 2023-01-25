Shader "Light2D/Internal/Light/SpriteLight"
{
    Properties
    {
        _MainTex ("Lightmap Texture", 2D) = "white" {}
        _Sprite ("Sprite Texture", 2D) = "black" {}
        _Rotation ("Sprite Rotation", Float) = 0
        _LinearColor ("Linear Color", Float) = 0
        _Outer("Outer", Float) = 0
        _Inner("Inner", Float) = 0
        _FlipX("FlipX", Float) = 0
        _FlipY("FlipY", Float) = 0
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
       
                float _LinearColor;
                sampler2D _Sprite;
                float _Rotation;

                float _Outer;
                float _Inner;
                float _FlipX;
                float _FlipY;

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
                    float3 xy : TEXCOORD1;
                    float4 spriteCoord : TEXCOORD2; 
                };

                float2 SpriteToCamera(float2 pos, float rotation)
                {
                    float c = cos(-rotation);
                    float s = sin(-rotation);

                    float x = pos.x;
                    float y = pos.y;

                    pos.x = x * c - y * s + 0.5;
                    pos.y = x * s + y * c + 0.5;

                    return(pos);
                }

                v2f vert (appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = float4(v.texcoord.x, v.texcoord.y, 1, 1);
                    
                    o.xy.xy = float2(v.texcoord.x - 0.5, v.texcoord.y - 0.5);
                    o.xy.z = _Inner >= 359;

                    o.spriteCoord.xy = SpriteToCamera(v.texcoord - float2(0.5, 0.5), _Rotation);
                    o.spriteCoord.zw = float2(1, 1);

                    o.spriteCoord.x = lerp(o.spriteCoord.x, 1 - o.spriteCoord.x, _FlipX);
                    o.spriteCoord.y = lerp(o.spriteCoord.y, 1 - o.spriteCoord.y, _FlipY);
     
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    float lightmap = tex2Dproj(_MainTex, i.texcoord).r;

                    fixed4 sprite = tex2Dproj(_Sprite, i.spriteCoord);
                    sprite.rgb *= sprite.a;

                    float dir = ((atan2(i.xy.y, i.xy.x) - _Rotation) * 57.2958 + 810) % 360;

                    float pointValue = lerp(max(0, min(1, (_Inner * 0.5 - abs(dir - 180) + _Outer) / _Outer)), 1, i.xy.z);
                    
                    fixed4 output = fixed4(1, 1, 1, 1);

                    output.rgb = (2 - lightmap * 2) * i.color.a * 2;

                    output.rgb *= pointValue;

                    output.rgb *= i.color;

                    output *= sprite;
   
                    return output;
                }

                ENDCG
            }
        }
    }
}