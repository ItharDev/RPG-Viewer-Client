using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light.Shadow
{
    public static class LegacyCPU
    {
        public static Pair2 pair = Pair2.Zero();
        public static Vector2 projectedMiddle, projectedLeft, projectedRight, outerLeft, outerRight;
        public static Vector2 edgeAWorld, edgeBWorld, edgeALocal, edgeBLocal;
        public static Vector2 closestPoint;
        public static Vector2 edgeAVector, edgeBVector;

        public static float angleA, angleB;
        public static float rotA, rotB;

        public static Vector2 middle;

        public static float sqrt;
 
        public static void Draw(List<Polygon2> polygons, float shadowDistanceMin, float shadowDistanceMax, float translucency)
        {
            if (polygons == null)
            {
                return;
            }

            Light2D light = ShadowEngine.light;
            Vector2 position = ShadowEngine.lightOffset + ShadowEngine.objectOffset;

            // float shadowDistance = ShadowEngine.lightSize;
            float lightSizeSquare = (ShadowEngine.lightSize * ShadowEngine.lightSize) * 0.5f;

            float outerAngle = Mathf.Deg2Rad * light.outerAngle;

            bool ignoreInside = ShadowEngine.ignoreInside;
            bool dontDrawInside = ShadowEngine.dontdrawInside;

            int PolygonCount = polygons.Count;

            Vector2 draw = ShadowEngine.drawOffset;

            if (translucency > 0)
            {
                GL.Color(new Color(1 - translucency, 0, 0, 0));
            }

            if (shadowDistanceMin <= 0 && shadowDistanceMax <= 0)
            {
                shadowDistanceMin = ShadowEngine.lightSize;
                shadowDistanceMax = ShadowEngine.lightSize;
            }
                else
            {
                // shadowDistance = distance;
                outerAngle = 0;
            }

            for(int i = 0; i < PolygonCount; i++)
            {
                Vector2[] pointsList = polygons[i].points;
                int pointsCount = pointsList.Length;

                if (ignoreInside)
                {
                    // change to sides of vertices, improve performance
                    if (Math2D.PointInPoly(-position, polygons[i]))
                    { 
                        continue;
                    }
                } 
                    else if (dontDrawInside)
                {
                    if (Math2D.PointInPoly(-position, polygons[i]))
                    { 
                        ShadowEngine.continueDrawing = false;
                        return;
                    }
                }
            
                for(int x = 0; x < pointsCount - 1; x++)
                {
                    int next = (x + 1) % pointsCount;
                    
                    pair.A = pointsList[x];
                    pair.B = pointsList[next];

                    edgeALocal.x = pair.A.x;
                    edgeALocal.y = pair.A.y;

                    edgeBLocal.x = pair.B.x;
                    edgeBLocal.y = pair.B.y;

                    edgeAWorld.x = edgeALocal.x + position.x;
                    edgeAWorld.y = edgeALocal.y + position.y;

                    edgeBWorld.x = edgeBLocal.x + position.x;
                    edgeBWorld.y = edgeBLocal.y + position.y;
  
                    float mx = (edgeAWorld.x + edgeBWorld.x) / 2;
                    float my = (edgeAWorld.y + edgeBWorld.y) / 2;

                    float step = Mathf.Sqrt(mx * mx + my * my) / ShadowEngine.lightSize;
                    float shadowLength = Mathf.Lerp(shadowDistanceMin, shadowDistanceMax, step);

                    // Need to test it
                    closestPoint = Math2D.ClosestPointOnLine(edgeAWorld, edgeBWorld);
                
                    if (closestPoint.x * closestPoint.x > lightSizeSquare || closestPoint.y * closestPoint.y > lightSizeSquare)
                    {
                        continue;
                    }

                    middle.x = (edgeAWorld.x + edgeBWorld.x) / 2;
                    middle.y = (edgeAWorld.y + edgeBWorld.y) / 2;

                    // Edge Rotation to the light
                    //float lightDirection = (float)Math.Atan2(middle.x, middle.y) * Mathf.Rad2Deg;
                    //float EdgeDirection = (float)Math.Atan2(edgeALocal.y - edgeBLocal.y, edgeALocal.x - edgeBLocal.x) * Mathf.Rad2Deg - 180;

                    //lightDirection -= EdgeDirection;
                    //lightDirection = (lightDirection + 720) % 360;
                    
                    // Culling only if outside
                    /*
                    if (culling && drawInside == false) {
                        if (lightDirection < 180) {
                            // Failing
                            //continue;
                        }
                    }
                    */

                    sqrt = Mathf.Sqrt(edgeAWorld.sqrMagnitude);
                    edgeAVector.x = edgeAWorld.x / sqrt;
                    edgeAVector.y = edgeAWorld.y / sqrt;

                    sqrt = Mathf.Sqrt(edgeBWorld.sqrMagnitude);
                    edgeBVector.x = edgeBWorld.x / sqrt;
                    edgeBVector.y = edgeBWorld.y / sqrt;

                    projectedRight.x = edgeAWorld.x + edgeAVector.x * shadowLength;
                    projectedRight.y = edgeAWorld.y + edgeAVector.y * shadowLength;

                    projectedLeft.x = edgeBWorld.x + edgeBVector.x * shadowLength;
                    projectedLeft.y = edgeBWorld.y + edgeBVector.y * shadowLength;

                    if (outerAngle > 0)
                    {
                        angleA = (float)System.Math.Atan2 (edgeAWorld.y, edgeAWorld.x);
                        angleB = (float)System.Math.Atan2 (edgeBWorld.y, edgeBWorld.x);

                        rotA = angleA - outerAngle;
                        rotB = angleB + outerAngle; 

                        outerRight.x = edgeAWorld.x + Mathf.Cos(rotA) * shadowLength;
                        outerRight.y = edgeAWorld.y + Mathf.Sin(rotA) * shadowLength;

                        outerLeft.x = edgeBWorld.x + Mathf.Cos(rotB) * shadowLength;
                        outerLeft.y = edgeBWorld.y + Mathf.Sin(rotB) * shadowLength;

                        // right penumbra
                        GL.TexCoord3(0, 0, 0);
                        GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);

                        GL.TexCoord3(1, 0, 0);
                        GL.Vertex3(draw.x + outerRight.x, draw.y + outerRight.y, 0);
                        
                        GL.TexCoord3(0, 1, 0);
                        GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                        
                        // left penumbra
                        GL.TexCoord3(0, 0, 0);
                        GL.Vertex3(draw.x + edgeBWorld.x, draw.y + edgeBWorld.y, 0);

                        GL.TexCoord3(1, 0, 0);
                        GL.Vertex3(draw.x + outerLeft.x, draw.y + outerLeft.y, 0);
                        
                        GL.TexCoord3(0, 1, 0);
                        GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                    }

                    GL.TexCoord3(0, 1, 0);

                    // Right Fin
                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                    GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                    GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);
                    
                    // Left Fin
                    GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);
                    GL.Vertex3(draw.x + edgeBWorld.x, draw.y + edgeBWorld.y, 0);
                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);

                    // Detailed Shadow
                    closestPoint = Math2D.ClosestPointOnLine(projectedLeft, projectedRight);

                    sqrt = Mathf.Sqrt(closestPoint.x * closestPoint.x + closestPoint.y * closestPoint.y);
                    closestPoint.x = closestPoint.x / sqrt;
                    closestPoint.y = closestPoint.y / sqrt;

                    projectedMiddle.x = middle.x + closestPoint.x * shadowLength;
                    projectedMiddle.y = middle.y + closestPoint.y *shadowLength;                        
                                
                    // Middle Fin
                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                    GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                    GL.Vertex3(draw.x + projectedMiddle.x, draw.y + projectedMiddle.y, 0); 
                }
            }

            if (translucency > 0)
            {
                GL.Color(new Color(1, 0, 1, 1));
            }
        }
    }
}