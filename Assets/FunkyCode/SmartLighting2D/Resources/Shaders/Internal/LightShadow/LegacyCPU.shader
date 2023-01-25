Shader "Light2D/Internal/Shadow/LegacyCPU"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		BlendOp Max

		// Blend SrcAlpha OneMinusSrcAlpha
		// Blend One OneMinusSrcAlpha, One One // multiply
		// Blend ne One // additive
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
				float2 texcoord : TEXCOORD0;
			};

			struct pixel
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
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
				float color = tex2D(_MainTex, p.texcoord).r;

				float shadow = (1 - color) * p.color.r;

				return fixed4(shadow, 0, 0, 0);
			}

			ENDCG
		}
	}
}