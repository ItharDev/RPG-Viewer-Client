
Shader "Light2D/Internal/Shadow/Fast"
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
		BlendOp Max

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
				float translucency : TEXCOORD3;
			};

			pixel vert(vertice v)
			{
				pixel o;

				float2 segStartPos = v.color.xy;
				float2 segEndPos = v.color.zw;
				
				float2 currentPos = lerp(segStartPos, segEndPos, v.vertex.x);

				if (v.vertex.y == 1)
				{
					o.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(currentPos, 0, 0.0001)));
				}
					else
				{
					o.vertex = UnityObjectToClipPos(float3(currentPos.x, currentPos.y, 0));
				}

				o.translucency = 1 - v.vertex.z;

				return o;
			}
		
			fixed4 frag(pixel i) : SV_Target
			{
				return i.translucency;
			}

			ENDCG
		}
		
	}
}