Shader "Light2D/Misc/MultiplyDarkness" {
    Properties{
        _MainTex("Main Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 1)) = 0.5
    }

    Category{
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Blend Zero SrcColor

        Cull Off Lighting Off ZWrite Off

        SubShader {
            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;

                float _Brightness;

                struct input {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct output {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                output vert(input v)
                {
                    output o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                    return o;
                }

                float4 frag(output i) : SV_Target
                {
                    float4 color = tex2D(_MainTex, i.texcoord);

                    color = lerp(color, float4(1, 1, 1, 1), _Brightness);

                    return color;
                }
                ENDCG
            }
        }
    }
}
