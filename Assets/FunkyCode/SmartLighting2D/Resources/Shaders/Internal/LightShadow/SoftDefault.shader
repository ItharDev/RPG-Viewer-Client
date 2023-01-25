
Shader "Light2D/Internal/Shadow/SoftDefault"
{        
	Properties
	{
		_LightVolume ("Light Volume", Range(0, 10)) = 1
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
		BlendOp Max

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"
		
			float _LightVolume;

			struct vertice
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
			};

			struct pixel
			{
				float4 vertex   : SV_POSITION;
				float4 penumbras : TEXCOORD1;
				float projectionVecDirFactor : TEXCOORD2;
				float translucency : TEXCOORD3;
			};

			float2x2 invert2x2(float2 basisX, float2 basisY)
			{
				float2x2 m = float2x2(basisX, basisY);
				return float2x2(m._m11, -m._m10, -m._m01, m._m00) / determinant(m);
			}

			float2 PointToLine(float2 pixelPos, float2 vA, float2 vB) 
			{
				// Aalgorithm
				float2 v0 = vB - vA;
				v0 /= sqrt(v0.x * v0.x + v0.y * v0.y);

				float t = dot(v0, pixelPos - vA);

				if (t <= 0) {
					return(vA);
				} else if (t >= distance(vA, vB)) {
					return(vB);
				} else {
					return(vA + v0 * t);
				}
			}

			pixel vert(vertice v)
			{
				pixel o;

				float2 segStartPos = v.color.xy;
				float2 segEndPos = v.color.zw;
				
				float2 currentPos = lerp(segStartPos, segEndPos, v.vertex.x);

				float2 A = _LightVolume * float2(-1, 1) * normalize(segStartPos).yx;
				float2 B = _LightVolume * float2(1, -1) * normalize(segEndPos).yx;

				float2 projectionOffset = lerp(A, B, v.vertex.x);
				float2 projectionVec = currentPos - projectionOffset;

				float2 seVec = segEndPos - segStartPos;
				float2 seNormal = seVec.yx * float2(-1.0, 1.0);

				if (v.vertex.y == 1)
				{
					o.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(currentPos - projectionOffset, 0, 0.0001)));
					o.projectionVecDirFactor = dot(seNormal, currentPos - projectionOffset); 

					float2 penumbraA = mul(invert2x2(A, segStartPos), projectionVec); 
					float2 penumbraB = mul(invert2x2(B, segEndPos), projectionVec);
					o.penumbras = float4(penumbraA, penumbraB);
				}
					else
				{
					o.vertex = UnityObjectToClipPos(float3(currentPos.x, currentPos.y, 0));
					o.projectionVecDirFactor = 0;
					
					float2 penumbraA = mul(invert2x2(A, segStartPos), currentPos - segStartPos);
					float2 penumbraB = mul(invert2x2(B, segEndPos), currentPos - segEndPos);
					o.penumbras = float4(penumbraA, penumbraB);
				}

				float2 pt = PointToLine(float2(0, 0), segStartPos, segEndPos);
			
				float dmin = distance(float2(0, 0), pt);

				o.translucency = dmin - _LightVolume;

				o.translucency = min(o.translucency, 1);
				o.translucency = max(o.translucency, 0);

				o.translucency *= (1 - v.vertex.z);

				return o;
			}
		
			fixed4 frag(pixel i) : SV_Target
			{
				float2 p = clamp(i.penumbras.xz / i.penumbras.yw, -1.0, 1.0);

				p = p * (3.0 - p * p) * 0.25 + 0.5;

				float2 value = lerp(p, 1.0, step(i.penumbras.yw, 0.0));

				float occlusion = (value.x + value.y - 1.0);

				return occlusion * step(i.projectionVecDirFactor, 0) * i.translucency;
			}

			ENDCG
		}
	}
}