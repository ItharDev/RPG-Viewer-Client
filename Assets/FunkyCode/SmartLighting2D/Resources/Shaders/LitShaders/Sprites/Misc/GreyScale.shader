Shader "Light2D/Sprites/Misc/LitGreyScale"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Lit ("Lit", Range(0,1)) = 1

		[HideInInspector] _PassId ("Pass Id", Int) = 1
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
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../../../LitShaders/SL2D_ShaderLibrary.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color    : COLOR;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD0;
				fixed4 color    : COLOR;
                float2 worldPos : TEXCOORD1;
			};
			
			sampler2D _MainTex;
			fixed4 _Color;
			float _Lit;

			v2f vert(appdata_t IN)
			{
				v2f OUT;

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
                
                OUT.worldPos = mul (unity_ObjectToWorld, IN.vertex);

				return OUT;
			}
        
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 spritePixel =  tex2D (_MainTex, IN.texcoord) * IN.color; 
				float greyscale = (spritePixel.r + spritePixel.g + spritePixel.b) / 3;

				spritePixel = float4(greyscale, greyscale, greyscale, spritePixel.a);

				fixed3 lightPixel = lerp(SL2D_Light(IN.worldPos), fixed3(1, 1, 1), 1 - _Lit);

                spritePixel.rgb *= spritePixel.a * lightPixel;

				return spritePixel;
			}

		    ENDCG
		}
	}
}