using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light.Shadow
{
    public static class Fast
    {
        public static Pair2 pair = Pair2.Zero();
        public static Color segmentData;

        public static void Draw(List<Polygon2> polygons, float translucency)
        {
            if (polygons == null)
            {
                return;
            }

            Vector2 position = ShadowEngine.lightOffset;
  
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

                for(int x = 0; x < pointsCount; x++)
                {
                    int next = (x + 1) % pointsCount;
                    
                    pair.A = pointsList[x];
                    pair.B = pointsList[next];

                    segmentData = new Color(pair.A.x + position.x, pair.A.y + position.y, pair.B.x + position.x, pair.B.y + position.y);

                    GL.Color(segmentData);

                    GL.Vertex3(0, 0, translucency);
                    GL.Vertex3(1, 0, translucency);
                    GL.Vertex3(1, 1, translucency);
                    GL.Vertex3(0, 1, translucency);
                }
            }
        }
    }
}