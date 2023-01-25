Shader "Light2D/Sprites/Bump/Bumped_Pass2"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _NormalMap("NormalMap", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Lit ("Lit", Range(0, 1)) = 1
		
		_Specular ("Specular", Range(0, 1)) = 1
        _OcclusionOffset ("Occlusion Offset", Range(0.01, 1)) = 1
        _OcclusionDepth("Occlusion Depth;", Range(1, 20)) = 1
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
			#include "../../../LitShaders/SL2D_ShaderFast.cginc"
			
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

			fixed4 _Color;
	
			sampler2D _MainTex;

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
				fixed4 spritePixel = tex2D (_MainTex, IN.texcoord) * IN.color;

				spritePixel.rgb *= lerp(SL2D_FAST_BUMP_PASS_2(IN.worldPos, IN.texcoord), 1, 1 - _Lit) * spritePixel.a;

				return spritePixel;
			}

		    ENDCG
		}
	}
}