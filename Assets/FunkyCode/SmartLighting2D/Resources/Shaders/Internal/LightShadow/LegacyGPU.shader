Shader "Light2D/Internal/Shadow/LegacyGPU"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
        //Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
        // Blend One OneMinusSrcAlpha

		BlendOp Max //, Max
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
           
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
 
            // Vertex Data
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            // Pixel Data
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color    : COLOR;
                float3 worldPosition : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = float4(1, 1, 1, 1);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float2 ClosestPointOnLine (float2 vA, float2 vB) {
                float vx = (vB.x - vA.x);
		        float vy = (vB.y - vA.y);

                float distance = sqrt(vx * vx + vy * vy);
                vx = vx / distance;
                vy = vy / distance;

                float t = vx * -vA.x + vy * -vA.y;  
            
                if (t <= 0) {
                    return vA;
                }
            
                float xs = vA.x - vB.x;
                float ys = vA.y - vB.y;

                bool dist = (t * t) >= (xs * xs + ys * ys);

                if (dist) { 
                    return vB;
                }

                vA.x += vx * t;
                vA.y += vy * t;
                
                return vA;
            }

            #if !SHADER_API_METAL

                [maxvertexcount(15)]
                void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
                {
                    v2f vertice0 = (v2f)0; // Left
                    v2f vertice1 = (v2f)0; // Right
                    v2f vertice3 = (v2f)0; // Left Outer
                    v2f vertice4 = (v2f)0; // Right Outer
                    v2f vertice5 = (v2f)0; // Middle Projected

                    v2f v0 = input[0]; // Left
                    v2f v1 = input[1]; // Right
                    v2f v2 = input[2]; // Center - Data

                    // Data
                    float shadowDistance = v2.worldPosition.x;
                    float outerAngle = v2.worldPosition.y;
                    float translucency = v2.worldPosition.z;
                    float depth = v0.worldPosition.z;

                    fixed4 blackColor = float4(0, depth, 0, 1 - translucency);
                    fixed4 whiteColor = float4(1, depth, 0, 1 - translucency);

                    float3 worldPos;

                    float2 left, right, leftOuter, rightOuter, middle, projectedMiddle, closestPoint;

                    // V0 Left
                    vertice0.vertex = v0.vertex; 
                    worldPos = v0.worldPosition.xyz;

                    float2 leftEdge = float2(worldPos.x, worldPos.y);
                    float2 leftAngle = atan2(worldPos.y, worldPos.x);

                    left.x = leftEdge.x + cos(leftAngle) * shadowDistance;
                    left.y = leftEdge.y + sin(leftAngle) * shadowDistance;

                    vertice0.vertex = UnityObjectToClipPos(float4(left.x, left.y, 0, 0));

                    // V1 Right
                    vertice1.vertex = v1.vertex;
                    worldPos = v1.worldPosition.xyz;
                    
                    float2 rightEdge = float2(worldPos.x, worldPos.y);
                    float2 rightAngle = atan2(worldPos.y, worldPos.x);

                    right.x = rightEdge.x + cos(rightAngle) * shadowDistance;
                    right.y = rightEdge.y + sin(rightAngle) * shadowDistance;

                    vertice1.vertex = UnityObjectToClipPos(float4(right.x, right.y, 0, 0));
                
                    // V3 Left Outer
                    float leftOuterAngle = leftAngle + outerAngle;

                    leftOuter.x = leftEdge.x + cos(leftOuterAngle) * shadowDistance;
                    leftOuter.y = leftEdge.y + sin(leftOuterAngle) * shadowDistance;

                    // V4 Right Outer
                    float rightOuterAngle = rightAngle - outerAngle;

                    rightOuter.x = rightEdge.x + cos(rightOuterAngle) * shadowDistance;
                    rightOuter.y = rightEdge.y + sin(rightOuterAngle) * shadowDistance;

                    // 5 Projected Middle
                    closestPoint = ClosestPointOnLine(float2(leftOuter.x, leftOuter.y), float2(rightOuter.x, rightOuter.y));

                    float square = sqrt(closestPoint.x * closestPoint.x + closestPoint.y * closestPoint.y);
                    closestPoint.x = closestPoint.x / square;
                    closestPoint.y = closestPoint.y / square;

                    middle = float2(leftEdge.x + rightEdge.x, leftEdge.y + rightEdge.y) / 2;

                    projectedMiddle.x = middle.x + closestPoint.x * shadowDistance;
                    projectedMiddle.y = middle.y + closestPoint.y * shadowDistance;

                    vertice5.vertex = UnityObjectToClipPos(float4(projectedMiddle.x, projectedMiddle.y, 0, 0));

                    // Inner Triangles         

                    vertice0.color = blackColor;
                    vertice1.color = blackColor;
                    vertice5.color = blackColor;
                    v0.color = blackColor;
                    v1.color = blackColor;

                    OutputStream.Append(vertice1);
                    OutputStream.Append(vertice0);
                    OutputStream.Append(vertice5);

                    OutputStream.RestartStrip();
                
                    OutputStream.Append(vertice0);
                    OutputStream.Append(v0);
                    OutputStream.Append(vertice1);

                    OutputStream.RestartStrip();

                    OutputStream.Append(vertice1);
                    OutputStream.Append(v0);
                    OutputStream.Append(v1);

                    // Outer Triangles

                    if (outerAngle > 0.1f)
                    {
                        OutputStream.RestartStrip();

                        vertice3.vertex = UnityObjectToClipPos(float4(leftOuter.x, leftOuter.y, 0, 0));
                        vertice4.vertex = UnityObjectToClipPos(float4(rightOuter.x, rightOuter.y, 0, 0));

                        vertice0.color = whiteColor;
                        vertice3.color = whiteColor;
                        v0.color = whiteColor;

                        vertice4.color = whiteColor;
                        v1.color = whiteColor;
                        vertice1.color = whiteColor;

                        vertice3.uv = float2(1, 0);
                        vertice0.uv = float2(0, 1);
                        v0.uv = float2(0, 0); 

                        vertice4.uv = float2(1, 0);
                        vertice1.uv = float2(0, 1);
                        v1.uv = float2(0, 0); 
        
                        OutputStream.Append(vertice0);
                        OutputStream.Append(vertice3);
                        OutputStream.Append(v0);

                        OutputStream.RestartStrip();

                        OutputStream.Append(v1);
                        OutputStream.Append(vertice4);
                        OutputStream.Append(vertice1); 
                    }
                }

            #endif
           
            fixed4 frag (v2f p) : SV_Target
            {
                float color = tex2D(_MainTex, p.uv);

                float shadow = (1 - color) * p.color.a;

                return fixed4(shadow, 0, 0, 1);
            }
            ENDCG
        }
    }
}