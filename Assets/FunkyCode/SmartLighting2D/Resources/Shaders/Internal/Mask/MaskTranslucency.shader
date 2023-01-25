Shader "Light2D/Internal/MaskTranslucency"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {} // sprite
		_SecTex ("Collision Texture", 2D) = "black" {} // collision lightmap
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
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			sampler2D _SecTex;

			// float _translucency;
			// float _intensity;
			float _TextureSize;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
		
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float alpha = tex2D(_MainTex, IN.texcoord).a;
					
				float tsize = _TextureSize;

				float2 pos = float2(IN.vertex.x / tsize, IN.vertex.y / tsize);

				float value = tex2D(_SecTex, float2(pos.x, pos.y)).r;

				fixed4 color = fixed4(0, 0, 0, 1); 
				color.r = (1 - IN.color.r) + lerp(1, value , IN.color.a); //;
				color.a = alpha;

				return color;
			}

			ENDCG
		}
	}
}