Shader "Light2D/Internal/BlurVertical"
{
    Properties
    {
        _MainTex ("Texture", any) = "" {}
    }

    SubShader
    {
        Pass
        {
            Cull Off
            ZWrite Off
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
 
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float _Strength;  
 
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
 
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };
 
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                half4 sum = half4(0, 0, 0, 0);

                float sy = _MainTex_TexelSize.y * _Strength; 

                #define GRABPIXEL_Y(kernely) tex2Dproj(_MainTex, float4(i.texcoord.x, i.texcoord.y + kernely * sy, 1, 1))
   
                sum += GRABPIXEL_Y(-4.0) * 0.05;
                sum += GRABPIXEL_Y(-3.0) * 0.09;
                sum += GRABPIXEL_Y(-2.0) * 0.12;
                sum += GRABPIXEL_Y(-1.0) * 0.15;
                sum += GRABPIXEL_Y(0.0) * 0.18;
                sum += GRABPIXEL_Y(1.0) * 0.15;
                sum += GRABPIXEL_Y(2.0) * 0.12;
                sum += GRABPIXEL_Y(3.0) * 0.09;
                sum += GRABPIXEL_Y(4.0) * 0.05;
              
                return sum;
            }
            ENDCG
        }
    }
}