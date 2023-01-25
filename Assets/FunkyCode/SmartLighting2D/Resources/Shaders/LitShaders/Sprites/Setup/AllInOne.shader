Shader "Light2D/Sprites/Setup/AllInOne"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        _NormalMap("NormalMap", 2D) = "white" {}
		_HeightMap("HeightMap", 2D) = "grey" {}

		_Color ("Color", Color) = (1, 1, 1, 1)

		_Lit ("Day Lit", Range(0, 1)) = 1
		_LightIntensity("Day Intensity", Range(0, 2)) = 1
		_Specular("Day Specular", Range(0, 1)) = 1

		_ReceiveDepth("Receive Shadows Depth", Range(-100, 100)) = 0
		_ReceiveStrength("Receive Shadows Strength", Range(0, 1)) = 0.5
		_ShadowsOffset("Receive Shadows Offset", Range(-1, 1)) = 0
		_ReceiveHeightMap("Receive Height Map", Range(0, 1)) = 1

		_OcclusionLit ("Occlusion Lit", Range(0, 1)) = 1
		_OcclusionOffset ("Occlusion Offset", Range(0.05, 2)) = 1
        _OcclusionDepth("Occlusion Depth", Range(1, 20)) = 1

		_LightMapLit ("Lightmap Lit", Range(0, 1)) = 1
	}

	SubShader
	{
		Tags
		{ 
			"DisableBatching" = "True"
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

			#include "../../../LitShaders/SL2D_ShaderLibrary.cginc"
			// #include "Assets/FunkyCode/SmartLighting2D/Resources/Shaders/LitShaders/SL2D_ShaderLibrary.cginc"

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
			fixed _ShadowsOffset;
			fixed _ReceiveHeightMap;
			fixed _LightMapLit;
			float _ReceiveDepth;
			float _ReceiveStrength;
	
			sampler2D _MainTex;
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
				// pass 1 = day lighting shadows depth
				// pass 2 = light source shadows
				// pass 3 = occlusion & normal mapping

				fixed4 spritePixel = tex2D (_MainTex, IN.texcoord) * IN.color;

				fixed3 dayBump = lerp(fixed3(1, 1, 1), SL2D_DayBump(IN.texcoord), _Lit);

				float2 heightValue = lerp(0, SL2D_DayOffsetByHeightMap(IN.texcoord), _ReceiveHeightMap);

				float2 dayOffset = SL2D_DayOffset(_ShadowsOffset);

				float depth = SL2D_Depth_Pass(1, IN.worldPos + heightValue + dayOffset);

				fixed3 lightSources = lerp(fixed3(1, 1, 1), SL2D_Pass_Black(2, IN.worldPos) + float3(1, 1, 1), _LightMapLit);

				if (depth > _ReceiveDepth)
				{
					float str = min(1, (1 * lightSources.r - _ReceiveStrength));
					spritePixel.rgb *= str;
				}
				
				fixed3 lightingNormal = lerp(fixed3(1, 1, 1), SL2D_Bump_Pass(3, IN.worldPos, IN.texcoord), _OcclusionLit);

				if (!InGameCamera(2, IN.worldPos) && !InSceneCamera(2, IN.worldPos)) {
					lightingNormal = float3(1, 1, 1);
				}

				spritePixel.rgb *= dayBump * lightSources * lightingNormal;
				spritePixel.rgb *= spritePixel.a; 

				return spritePixel;
			}

		    ENDCG
		}
	}
}