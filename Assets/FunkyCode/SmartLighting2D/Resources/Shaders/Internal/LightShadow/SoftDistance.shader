Shader "Light2D/Internal/Shadow/SoftDistance"
{
	Properties
	{
		_MainTex ("Penumbra", 2D) = "white" {}
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

		BlendOp Max

		// Blend SrcAlpha OneMinusSrcAlpha
		// Blend One OneMinusSrcAlpha, One One // multiply
		// Blend One // additive
		// Blend Zero SrcColor

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;

			struct vertice
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float3 texcoord : TEXCOORD0;
			};

			struct pixel
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float3 texcoord  : TEXCOORD0;
			};

			pixel vert(vertice v)
			{
				pixel p;

				p.vertex = UnityObjectToClipPos(v.vertex);
				p.texcoord = v.texcoord;
				p.color = v.color;
		
				return p;
			}

			fixed4 frag(pixel p) : SV_Target
			{
				float x = p.texcoord.x - 0.5;
				float y = p.texcoord.y - 0.5;

				fixed color = tex2D(_MainTex, p.texcoord).r * step(0.1, p.texcoord.z);
				color += step(0.1, p.color.a) * (1 - p.texcoord.x);
				color += step(0.1, p.color.g) * max(0, (1 - sqrt(x * x + y * y) * 2));
				color += step(0.1, p.color.r);
				color *= (1 - p.color.b);
				
				return color;
			}

			ENDCG
		}
	}
}