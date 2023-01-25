using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light.Shadow
{
    public static class SoftDistance
    { 
        public static Pair2 pair = Pair2.Zero();
        public static Vector2 edgeAWorld, edgeBWorld, edgeALocal, edgeBLocal;
        public static Vector2 projectedMiddle, projectedLeft, projectedRight, outerLeft, outerRight;

        public static Vector2 edgeAVector, edgeBVector;

        public static float sqrt;

        public static void Draw(List<Polygon2> polygons, float shadowDistanceMin, float shadowDistanceMax, float translucency)
        {
            if (polygons == null)
            {
                return;
            }

            Vector2 position = ShadowEngine.lightOffset + ShadowEngine.objectOffset;

            int PolygonCount = polygons.Count;

            Vector2 draw = ShadowEngine.drawOffset;

            bool finiteShadows = true;

            if (shadowDistanceMin <= 0 && shadowDistanceMax <= 0)
            {
                shadowDistanceMin = ShadowEngine.lightSize;
                shadowDistanceMax = ShadowEngine.lightSize;

                finiteShadows = false;
            }

            for(int i = 0; i < PolygonCount; i++)
            {
                Vector2[] pointsList = polygons[i].points;
                int pointsCount = pointsList.Length;

                if (ShadowEngine.ignoreInside)
                {
                    // change to sides of vertices, improve performance
                    if (Math2D.PointInPoly(-position, polygons[i]))
                    { 
                        continue;
                    }
                } 
                    else if (ShadowEngine.dontdrawInside)
                {
                    if (Math2D.PointInPoly(-position, polygons[i]))
                    { 
                        ShadowEngine.continueDrawing = false;
                        return;
                    }
                }

                for(int x = 0; x < pointsCount; x++)
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

                    GL.Color(new Color(1, 0, translucency, 0));

                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                    GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                    GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);

                    GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);
                    GL.Vertex3(draw.x + edgeBWorld.x, draw.y + edgeBWorld.y, 0);
                    GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);

                    if (finiteShadows)
                    {
                        DrawLine(projectedRight, projectedLeft, 0, translucency);
                        DrawLine(edgeBWorld, projectedLeft, -1, translucency);
                        DrawLine(edgeAWorld, projectedRight, 1, translucency);
                    }
                        else
                    {
                        // Detailed Shadow
                        Vector2 closestPoint = Math2D.ClosestPointOnLine(projectedLeft, projectedRight);

                        sqrt = Mathf.Sqrt(closestPoint.x * closestPoint.x + closestPoint.y * closestPoint.y);
                        closestPoint.x = closestPoint.x / sqrt;
                        closestPoint.y = closestPoint.y / sqrt;

                        projectedMiddle.x = mx + closestPoint.x * shadowLength;
                        projectedMiddle.y = my + closestPoint.y *shadowLength;                        
                                    
                        // Middle Fin
                        GL.Vertex3(draw.x + projectedLeft.x, draw.y + projectedLeft.y, 0);
                        GL.Vertex3(draw.x + projectedRight.x, draw.y + projectedRight.y, 0);
                        GL.Vertex3(draw.x + projectedMiddle.x, draw.y + projectedMiddle.y, 0); 

                        DrawLine(edgeBWorld, projectedLeft, -11, translucency);
                        DrawLine(edgeAWorld, projectedRight, 11, translucency);
                    }
                }
            }
        }

        public static void DrawLine(Vector2 point, Vector2 nextPoint, int type, float translucency)
        {
            float stepNext = nextPoint.magnitude / ShadowEngine.lightSize;
            float sizePointNext = Mathf.Lerp(ShadowEngine.light.shadowDistanceClose, ShadowEngine.light.shadowDistanceFar, stepNext);

            float step = point.magnitude / ShadowEngine.lightSize;
            float sizePoint = Mathf.Lerp(ShadowEngine.light.shadowDistanceClose, ShadowEngine.light.shadowDistanceFar, step);

            sizePoint = sizePoint < 0 ? 0 : sizePoint * ShadowEngine.lightSize * 0.01f;
            sizePointNext = sizePointNext < 0 ? 0 : sizePointNext * ShadowEngine.lightSize * 0.01f;

            float direction = point.Atan2(nextPoint);
            
            Vector2 p1 = point;
            Vector2 p2 = nextPoint;
            Vector2 p3 = nextPoint;
            Vector2 p4 = point;

            switch(type)
            {
                case -1: // left penumbra

                    p3.x += Mathf.Cos(direction - Mathf.PI / 2) * sizePointNext;
                    p3.y += Mathf.Sin(direction - Mathf.PI / 2) * sizePointNext;

                    GL.Color(new Color(0, 0, translucency, 0));

                    GL.TexCoord3(0, 0, 1);
                    GL.Vertex3(p1.x, p1.y, 0);

                    GL.TexCoord3(1, 1, 1);
                    GL.Vertex3(p2.x, p2.y, 0);

                    GL.TexCoord3(0, 1, 1);
                    GL.Vertex3(p3.x, p3.y, 0);

                    // dot

                    GL.Color(new Color(0, 1, translucency, 0));

                    GL.TexCoord3(0, 0, 0);
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y - sizePointNext, 0);

                    GL.TexCoord3(1, 0, 0);
                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y - sizePointNext, 0);

                    GL.TexCoord3(1, 1, 0);
                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y + sizePointNext, 0);

                    GL.TexCoord3(1, 1, 0);
                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y + sizePointNext, 0);

                    GL.TexCoord3(0, 1, 0);
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y + sizePointNext, 0);

                    GL.TexCoord3(0, 0, 0);
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y - sizePointNext, 0);

                break;

                case 1: // right penumbra

                    p3.x += Mathf.Cos(direction + Mathf.PI / 2) * sizePointNext;
                    p3.y += Mathf.Sin(direction + Mathf.PI / 2) * sizePointNext;
                    
                    GL.Color(new Color(0, 0, translucency, 0));

                    GL.TexCoord3(0, 0, 1);
                    GL.Vertex3(p1.x, p1.y, 0);

                    GL.TexCoord3(1, 1, 1);
                    GL.Vertex3(p2.x, p2.y, 0);

                    GL.TexCoord3(0, 1, 1);
                    GL.Vertex3(p3.x, p3.y, 0);

                break;

                case -11: // left penumbra

                    p3.x += Mathf.Cos(direction - Mathf.PI / 2) * sizePointNext;
                    p3.y += Mathf.Sin(direction - Mathf.PI / 2) * sizePointNext;

                    GL.Color(new Color(0, 0, translucency, 0));

                    GL.TexCoord3(0, 0, 1);
                    GL.Vertex3(p1.x, p1.y, 0);

                    GL.TexCoord3(1, 1, 1);
                    GL.Vertex3(p2.x, p2.y, 0);

                    GL.TexCoord3(0, 1, 1);
                    GL.Vertex3(p3.x, p3.y, 0);

                break;

                case 11: // right penumbra

                    p3.x += Mathf.Cos(direction + Mathf.PI / 2) * sizePointNext;
                    p3.y += Mathf.Sin(direction + Mathf.PI / 2) * sizePointNext;
                    
                    GL.Color(new Color(0, 0, translucency, 0));

                    GL.TexCoord3(0, 0, 1);
                    GL.Vertex3(p1.x, p1.y, 0);

                    GL.TexCoord3(1, 1, 1);
                    GL.Vertex3(p2.x, p2.y, 0);

                    GL.TexCoord3(0, 1, 1);
                    GL.Vertex3(p3.x, p3.y, 0);

                break;

                // soft line

                case 0:

                    p3.x -= Mathf.Cos(direction + Mathf.PI / 2) * sizePointNext;
                    p3.y -= Mathf.Sin(direction + Mathf.PI / 2) * sizePointNext;

                    p4.x -= Mathf.Cos(direction + Mathf.PI / 2) * sizePoint;
                    p4.y -= Mathf.Sin(direction + Mathf.PI / 2) * sizePoint;
                    
                    GL.Color(new Color(0, 0, translucency, 1));

                    GL.TexCoord3(0, 0, 0);

                    GL.Vertex3(p1.x, p1.y, 0);
                    GL.Vertex3(p2.x, p2.y, 0);

                    GL.TexCoord3(1, 0, 0);
                    GL.Vertex3(p3.x, p3.y, 0);

                    GL.Vertex3(p3.x, p3.y, 0);
                    GL.Vertex3(p4.x, p4.y, 0);

                    GL.TexCoord3(0, 0, 0);
                    GL.Vertex3(p1.x, p1.y, 0);

                break;
            }
        }
    }
}