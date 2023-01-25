using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Depth
{
    public static class Shadow
    {
        public static float direction;
        public static float directionCos;
        public static float directionSin;
        public static float shadowDistance;

        public static Vector2 pointA, pointB, pointAOffset, pointBOffset;

        static public void Begin()
        {
            Lighting2D.Materials.shadow.GetDepthDayShadow().SetPass(0);

            GL.Begin(GL.QUADS);

            direction = -Lighting2D.DayLightingSettings.direction * Mathf.Deg2Rad;
            shadowDistance = Lighting2D.DayLightingSettings.height;

            directionCos = Mathf.Cos(direction);
            directionSin = Mathf.Sin(direction);
        }

        static public void End()
        {
            GL.End();
        }

        static public void Draw(DayLightCollider2D id, Vector2 position)
        {
            if (id.mainShape.height <= 0) // id.shadowTranslucency >= 1
            {
                return;
            }
        
            if (!id.InAnyCamera())
            {
                return;
            }

            float distance = shadowDistance * id.mainShape.height;
            float cosShadow = directionCos * distance;
            float sinShadow = directionSin * distance;

            float depth = (100f + (float)id.GetDepth()) / 255;
            bool depthFalloff = (id.depthFalloff == DayLightCollider2D.DepthFalloff.Enabled);

            Color color = new Color(depth, 0, 0, 1);
            Color empty = new Color(0, 0, 0, 1);

            if (!depthFalloff)
            {
                GL.Color(color);
            }

            DayLightColliderShape shape = id.mainShape;

            if (!shape.isStatic)
            {
                shape.ResetWorld();
            }

            List<Polygon2> polygons = shape.GetPolygonsWorld();

            Vector2 pos = position;

            int polygonCount = polygons.Count;

            for(int p = 0; p < polygonCount; p++)
            {
                Polygon2 polygon = polygons[p];

                int pointsCount = polygon.points.Length;

                for(int i = 0; i < pointsCount; i++ )
                {
                    pointA = polygon.points[i];
                    pointA.x += pos.x;
                    pointA.y += pos.y;

                    pointB = polygon.points[(i + 1) % pointsCount];
                    pointB.x += pos.x;
                    pointB.y += pos.y;

                    pointAOffset.x = pointA.x + cosShadow;
                    pointAOffset.y = pointA.y + sinShadow;
    
                    pointBOffset.x = pointB.x + cosShadow;
                    pointBOffset.y = pointB.y + sinShadow;

                    if (depthFalloff)
                    {
                        GL.Color(color);
                    }

                    GL.Vertex3(pointB.x, pointB.y, 0);
                    GL.Vertex3(pointA.x, pointA.y, 0);

                    if (depthFalloff)
                    {
                        GL.Color(empty);
                    }

                    GL.Vertex3(pointAOffset.x, pointAOffset.y, 0);
                    GL.Vertex3(pointBOffset.x, pointBOffset.y, 0);
                }   
            }
        }

        static public void DrawFill(DayLightCollider2D id, Vector2 position)
        {
            if (id.mainShape.height <= 0) // id.shadowTranslucency >= 1
            {
                return;
            }

            if (!id.InAnyCamera())
            {
                return;
            }

            float depth = (100f + (float)id.GetDepth()) / 255;

            GLExtended.color = new Color(depth, 0, 0, 1);

            Vector2 pos = id.mainShape.transform2D.position;
            pos += position;
            Vector2 scale = id.mainShape.transform2D.scale;
            float rotation = id.mainShape.transform2D.rotation;
        
            DayLightColliderShape shape = id.mainShape;

            if (!shape.isStatic)
            {
                shape.ResetWorld();
            }

            List<Polygon2> polygons = shape.GetPolygonsLocal();

            List<MeshObject> meshes = shape.GetMeshes();

            if (meshes == null)
            {
                return;
            }

            if (meshes.Count < 1)
            {
                return;
            }

            GLExtended.DrawMeshPass(meshes, pos, scale, rotation);
        }
    }
}