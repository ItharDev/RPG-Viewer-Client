Shader "Light2D/Internal/AlphaColor"
{        
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
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"
		
			struct vertice
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
			};

			struct pixel
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
			};

			pixel vert(vertice v)
			{
				pixel p;

				p.vertex = UnityObjectToClipPos(v.vertex);
				p.color = v.color;
				p.color.rgb *= v.color.a;
		
				return p;
			}

			fixed4 frag(pixel p) : SV_Target
			{
				return p.color;
			}

			ENDCG
		}
	}
}