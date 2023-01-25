Shader "Light2D/Internal/BumpMap/Day" {
    Properties
    {
        _MainTex ("Diffuse Texture", 2D) = "white" {}
        _Bump ("Bump", 2D) = "Bump" {}

        _LightSize ("LightSize", Float) = 1
        _LightIntensity ("LightIntensity", Float) = 1

        _LightRX("LightRX", float) = 1
        _LightRY("LightRY", float) = 1
        _LightRZ("LightRZ", float) = 1
    }

    SubShader
    {
        Tags
        { 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True" 
            "RenderType" = "Transparent" 
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {    
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag 

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Bump;
  
            float _LightSize;
            float _LightIntensity;

            float _LightRX;
            float _LightRY;
            float _LightRZ;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
            };

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;

                output.pos = UnityObjectToClipPos(input.vertex);
                output.posWorld = mul(unity_ObjectToWorld, input.vertex);

                output.uv = float2(input.uv.xy);
                output.color = input.color;

                return output;
            }

            fixed4 DayBump(VertexOutput input)
			{
                float3 normalDirection = UnpackNormal(tex2D(_Bump, input.uv)).xyz;

                normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), unity_WorldToObject).xyz);
                normalDirection.z *= -1;
                normalDirection = normalize(normalDirection);

                float3 lightDirection = normalize(float3(_LightRX, _LightRY, _LightRZ - 0.5));

                float normalDotLight = dot(normalDirection, lightDirection);
  
                float specularLevel = 0.0f;

                if (normalDotLight > 0.0f) {
                    specularLevel = _LightIntensity * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
                }

                float diffuseLevel = _LightIntensity * max(0.0f, normalDotLight);

                float3 diffuseAndSpecular = diffuseLevel + specularLevel;

                return float4(diffuseAndSpecular, 1);
            }

            float4 frag(VertexOutput input) : COLOR
            {
                float alpha = tex2D(_MainTex, input.uv).a;

                fixed4 day = DayBump(input);

                day *= alpha;
                day.a = alpha;

                return(day);
            }

            ENDCG
        }
    }
}
