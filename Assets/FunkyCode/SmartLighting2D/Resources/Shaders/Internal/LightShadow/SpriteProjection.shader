Shader "Light2D/Internal/SpriteProjection" {
        
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
		// Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Max //, Max

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"

			sampler2D _MainTex;

			struct appdata_t
			{
				float4 vertex	: POSITION;
				float4 color	: COLOR;
				float2 texcoord	: TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex	: SV_POSITION;
				fixed4 color    : COLOR;
				float2 uv		: TEXCOORD0;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.texcoord;
				OUT.color = IN.color;
		
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = tex2D (_MainTex, IN.uv);
				color.r  = IN.color.r;
				color.g  = IN.color.r;
				color.b  = IN.color.r;
	
				color.rgb *= color.a;

				float shadow = (1 - color.r) * color.a;
				
				return float4(shadow, 0, 0, 0);
			}

			ENDCG
		}
	}
}