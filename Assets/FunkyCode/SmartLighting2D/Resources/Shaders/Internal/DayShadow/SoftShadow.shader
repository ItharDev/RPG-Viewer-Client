Shader "Light2D/Internal/Day/SoftShadow"
{        
	Properties
	{
		_Darkness("Darkness",Color) = (0, 0, 0, 1)
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
		BlendOp Min

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"

			float4 _Darkness;
		
			struct vertice
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct pixel
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD0;
				float2 xy : TEXCOORD1;
				float4 effect : TEXCOORD2;
			};

			pixel vert(vertice v)
			{
				pixel p;

				float4 color = v.color;
			
				float4 vertex = v.vertex;
				
				if (color.b > 1)
				{
					color.a += vertex.z;

					p.texcoord = 0;
				}

				vertex.z = 0;
		
				p.vertex = UnityObjectToClipPos(vertex);
			
				if (color.b <= 1)
				{
					if (color.g > 0.1)
					{
						if (v.vertex.z > 2)
						{
							p.texcoord = float2(0, 1);
						}
							else if (v.vertex.z > 1)
						{
							p.texcoord = float2(1, 1);
						}
							else if (v.vertex.z > 0)
						{
							p.texcoord = float2(1, 0);
						}
							else
						{
							p.texcoord = float2(0, 0);
						}
					}
						else if (color.r > 0.1)
					{
						p.texcoord = float2(v.vertex.z > 0, 0);
					}
						else
					{
						p.texcoord = v.texcoord;
					}
				}

				p.xy = float2(p.texcoord.x - 0.5, p.texcoord.y - 0.5);

				p.effect = float4(color.r > 0.1, color.g > 0.1, color.b > 0.1, color.a);
		
				return p;
			}

			fixed4 frag(pixel p) : SV_Target
			{
				float4 color = float4(1, 1, 1, 1);
				float dist = sqrt(p.xy.x * p.xy.x + p.xy.y * p.xy.y) * 2;

				color.rgb = lerp(color.rgb, p.texcoord.x, p.effect.r);
				color.rgb = lerp(color.rgb, 0, p.effect.b);
				color.rgb = lerp(color.rgb, dist, p.effect.g);

				color.rgb = lerp(color.rgb + _Darkness.rgb + p.effect.a, 1, 1 - _Darkness.a);
				
				return color;
			}

			ENDCG
		}
	}
}