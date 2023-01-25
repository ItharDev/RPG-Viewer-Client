Shader "Light2D/Internal/Depth/DayShadow"
{        
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
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
		BlendOp Max // , Max

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
				float4 texcoord  : TEXCOORD0;
			};

			pixel vert(vertice v)
			{
				pixel p;

				p.vertex = UnityObjectToClipPos(v.vertex);
				p.texcoord = float4(v.texcoord.x, v.texcoord.y, 1, 1);
				p.color = v.color;
		
				return p;
			}

			fixed4 frag(pixel p) : SV_Target
			{
				float draw = tex2Dproj (_MainTex, p.texcoord).a > 0.5;

				return lerp(float4(0, 1, 1, 1), p.color, draw);
			}

			ENDCG
		}
	}
}