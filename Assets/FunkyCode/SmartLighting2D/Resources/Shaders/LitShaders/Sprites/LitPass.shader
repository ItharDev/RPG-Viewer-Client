Shader "Light2D/Sprites/LitPass"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Lit ("Lit", Range(0, 1)) = 1

		[HideInInspector] _PassId ("Pass Id", Int) = 1
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
		
			#pragma multi_compile SL2D_PASS_0 SL2D_PASS_1 SL2D_PASS_2 SL2D_PASS_3 SL2D_PASS_4 SL2D_PASS_5 SL2D_PASS_6 SL2D_PASS_7 SL2D_PASS_8 

			#include "UnityCG.cginc"
			#include "../../LitShaders/SL2D_ShaderPassLibrary.cginc"
			
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
				float4 color    : COLOR;
                float2 worldPos : TEXCOORD1;
			};
			
			sampler2D _MainTex;
			float4 _Color;
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
				float4 spritePixel = tex2D (_MainTex, IN.texcoord) * IN.color;

				float3 lights = lerp(float3(1, 1, 1), SL2D_Light(IN.worldPos), _Lit);

				spritePixel.rgb *= spritePixel.a * lights;

				return spritePixel;
			}

		    ENDCG
		}
	}

	CustomEditor "FunkyCode.PassShader"
}