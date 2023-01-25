Shader "Light2D/Internal/LightSprite/Light"
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

		//Blend SrcAlpha One
		Blend One One

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;

				fixed4 data : TEXCOORD1;

				// data
				// x - radius
				// y - ratio
				// z - not used
				// w - not used
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				fixed4 data : TEXCOORD1;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				OUT.data = IN.data;
	
				return OUT;
			}

			#if !SHADER_API_METAL

		
			
			#endif

			fixed4 frag(v2f IN) : SV_Target
			{
				float4 color = tex2D (_MainTex, IN.texcoord);

				color.rgb *= color.a * 2;
		
				// Mask Type
				if (IN.data.z > 0)
				{
					if (IN.data.x > 0)
					{
						// #if !SHADER_API_METAL
						// 	color *= Blur(IN).a;
						// #endif

						return(color* IN.color * 1.5);
					}
						else
					{
						return(color * IN.color * 1.5); //  tex2D (_MainTex, IN.texcoord).a
					}
				// Light Type
				}
					else
				{
					if (IN.data.x > 0)
					{
						// #if !SHADER_API_METAL
							// color *= Blur(IN);
						// #endif
						
						return(color * IN.color * 1.5);
					}
						else
					{
						return(color * IN.color * IN.color.a);
					}
				}
				
			}
			
			ENDCG
		}
	}
}