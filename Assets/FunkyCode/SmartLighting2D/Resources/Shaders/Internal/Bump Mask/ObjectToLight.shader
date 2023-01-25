Shader "Light2D/Internal/BumpMap/ObjectToLight"
{
    // materialNormalMap_ObjectToLight.SetFloat("_translucency", (1 * 100) / light.size);
    
    Properties
    {
        _MainTex ("Diffuse Texture", 2D) = "white" {}
        _Bump ("Bump", 2D) = "Bump" {}
        _SecTex ("Translucency Texture", 2D) = "black" {}

        _LightSize ("Light Size", Float) = 1
        _LightZ ("Light Z", Float) = 1
  
        _LightRX("Light RX", float) = 1
        _LightRY("Light RY", float) = 1

        _LightIntensity ("Light Intensity", Float) = 1
        _LightColor("Light Color", float) = 1

		_TextureSize("Texture Size", Range(32,4000)) = 2048
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

        Pass
        {    
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag 

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Bump;
            sampler2D _SecTex;
  
            float _LightSize;
            float _LightZ;
            float _LightIntensity;

            float _LightRX;
            float _LightRY;
            float _LightColor;

			float _TextureSize;

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

            float4 frag(VertexOutput input) : COLOR
            {
                float alpha = tex2D(_MainTex, input.uv).a;

                float3 normalDirection = (tex2D(_Bump, input.uv).xyz - 0.5f) * 2.0f;
                normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), unity_WorldToObject).xyz);
                normalDirection.z *= -1;
                normalDirection = normalize(normalDirection);

                float3 vertexToLightSource = float3(_LightRX, _LightRY , -1.5f);
                
                float distance = length(vertexToLightSource);
                float lightUV = 1 - ((distance - _LightSize) / _LightSize);

                lightUV *= _LightColor;

                float intensity = _LightIntensity * 0.5f;

                float attenuation = sqrt(distance * distance) * intensity; 
                float3 lightDirection = normalize(vertexToLightSource);

                float normalDotLight = dot(normalDirection, lightDirection);
                float diffuseLevel = attenuation * max(0.0f, normalDotLight);

                float specularLevel = step(0, normalDotLight) * attenuation * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);

                float diffuseReflection = diffuseLevel * lightUV + specularLevel * lightUV;

                float2 pos = float2(input.pos.x / _TextureSize, input.pos.y / _TextureSize);

                float tex = tex2D(_SecTex, float2(pos.x, pos.y)).r;

                float result = tex  + (1 - diffuseReflection);

                return float4(result, result, result, alpha );
             }

             ENDCG
        }
    }
}
