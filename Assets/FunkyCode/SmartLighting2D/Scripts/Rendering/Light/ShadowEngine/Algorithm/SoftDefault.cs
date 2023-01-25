using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light.Shadow
{
    public static class SoftDefault
    {
        public static Pair2 pair = Pair2.Zero();
        public static Vector2 edgeAWorld, edgeBWorld;

        public static void Draw(List<Polygon2> polygons, float translucency)
        {
            if (polygons == null)
            {
                return;
            }

            Vector2 position = ShadowEngine.lightOffset;
            position.x += ShadowEngine.objectOffset.x;
            position.y += ShadowEngine.objectOffset.y; 
  
            int PolygonCount = polygons.Count;

            for(int i = 0; i < PolygonCount; i++)
            {
                Vector2[] pointsList = polygons[i].points;
                int pointsCount = pointsList.Length;

                if (ShadowEngine.ignoreInside)
                {
                    // change to sides of vertices?
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

                SoftShadowSorter.Set(polygons[i], ShadowEngine.light);

                pair.A.x = SoftShadowSorter.minPoint.x + position.x;
                pair.A.y = SoftShadowSorter.minPoint.y + position.y;

                pair.B.x = SoftShadowSorter.maxPoint.x + position.x;
                pair.B.y = SoftShadowSorter.maxPoint.y + position.y;

                Color segmentData = new Color(pair.B.x, pair.B.y, pair.A.x, pair.A.y);

                GL.Color(segmentData);

                GL.Vertex3(0, 0, translucency);
                GL.Vertex3(1, 0, translucency);
                GL.Vertex3(0, 1, translucency);
     
                GL.Vertex3(1, 0, translucency);
                GL.Vertex3(1, 1, translucency);
                GL.Vertex3(0, 1, translucency);

                for(int x = 0; x < pointsCount; x++)
                {
                    int next = (x + 1) % pointsCount;
                    
                    pair.A = pointsList[x];
                    pair.B = pointsList[next];
 
                    edgeAWorld.x = pair.A.x + position.x;
                    edgeAWorld.y = pair.A.y + position.y;

                    edgeBWorld.x = pair.B.x + position.x;
                    edgeBWorld.y = pair.B.y + position.y;

                    segmentData = new Color(edgeAWorld.x, edgeAWorld.y, edgeBWorld.x, edgeBWorld.y);

                    GL.Color(segmentData);

                    GL.Vertex3(0, 0, translucency);
                    GL.Vertex3(1, 0, translucency);
                    GL.Vertex3(0, 1, translucency);
     
                    GL.Vertex3(1, 0, translucency);
                    GL.Vertex3(1, 1, translucency);
                    GL.Vertex3(0, 1, translucency);
                }
            }
        }
    }
}