using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Day
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
            Material material = Lighting2D.Materials.shadow.GetDayCPUShadow();
            material.SetColor("_Darkness", Lighting2D.DayLightingSettings.ShadowColor);
            
            material.SetPass(0);

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
            if (id.mainShape.height <= 0 || id.shadowTranslucency >= 1)
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

            DayLightColliderShape shape = id.mainShape;

           // if (!shape.isStatic)
           // {
            //    shape.ResetWorld();//????????????
           // }

            List<Polygon2> polygons = shape.GetPolygonsWorld();

            Vector2 pos = position;

            int polygonCount = polygons.Count;

            bool softness = id.shadowSoftness > 0 && id.shadowEffect == DayLightCollider2D.ShadowEffect.Softness;

            int falloff = id.shadowEffect == DayLightCollider2D.ShadowEffect.Falloff ? 2 : 1;

            for(int p = 0; p < polygonCount; p++)
            {
                Polygon2 polygon = polygons[p];

                int pointsCount = polygon.points.Length;

                for(int i = 0; i < pointsCount; i++)
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

                    // add: pass coordinates via color
                    // add: projection via vertex shader
                    // color(px, py, nx, ny)
                    // gl.tex(translucency, 0, 0)
                    // shader _direction + cossinn values
                    // gl.vertex3(0, 0, -1) -1 project

                    if (id.shadowDistance > 0)
                    {
                        GL.Color(new Color(0, 0, falloff, id.shadowTranslucency));

                        GL.Vertex3(pointA.x, pointA.y, 0);
                        GL.Vertex3(pointAOffset.x, pointAOffset.y, 1);
                        GL.Vertex3(pointBOffset.x, pointBOffset.y, 1);
                        GL.Vertex3(pointB.x, pointB.y, 0);
                    }

                    if (softness)
                    {
                        // only when soft
                        DrawLine(pointAOffset, pointBOffset, 0, id.shadowTranslucency, id.shadowSoftness);
                        DrawLine(pointA, pointAOffset, 0, id.shadowTranslucency, id.shadowSoftness);
                        DrawLine(pointBOffset, pointB, 0, id.shadowTranslucency, id.shadowSoftness);
                        DrawLine(pointA, pointB, 0, id.shadowTranslucency, id.shadowSoftness);

                        DrawLine(pointA, pointA, 1, id.shadowTranslucency, id.shadowSoftness);
                        DrawLine(pointAOffset, pointAOffset, 1, id.shadowTranslucency, id.shadowSoftness);
                    }
                }   
            }
        }

        public static void DrawLine(Vector2 point, Vector2 nextPoint, int type, float translucency, float softness)
        {
            float sizePoint = softness;
            float sizePointNext = softness;

            float direction = point.Atan2(nextPoint);
            
            Vector2 p1 = point;
            Vector2 p2 = nextPoint;
            Vector2 p3 = nextPoint;
            Vector2 p4 = point;

            switch(type)
            {
                case 0:

                    p3.x -= Mathf.Cos(direction + Mathf.PI / 2) * sizePointNext;
                    p3.y -= Mathf.Sin(direction + Mathf.PI / 2) * sizePointNext;

                    p4.x -= Mathf.Cos(direction + Mathf.PI / 2) * sizePoint;
                    p4.y -= Mathf.Sin(direction + Mathf.PI / 2) * sizePoint;
                    
                    GL.Color(new Color(1, 0, 0, translucency));
                    GL.Vertex3(p1.x, p1.y, 0);
                    GL.Vertex3(p2.x, p2.y, 0);
                    GL.Vertex3(p3.x, p3.y, 1);
                    GL.Vertex3(p4.x, p4.y, 1);

                break;

                case 1:

                    GL.Color(new Color(0, 1, 0, translucency));
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y - sizePointNext, 0);
                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y - sizePointNext, 1);
                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y + sizePointNext, 2);
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y + sizePointNext, 3);

                break;
            }
        }

        public static void DrawLineTri(Vector2 point, Vector2 nextPoint, int type, float translucency, float softness)
        {
            float sizePoint = softness;
            float sizePointNext = softness;

            float direction = point.Atan2(nextPoint);
            
            Vector2 p1 = point;
            Vector2 p2 = nextPoint;
            Vector2 p3 = nextPoint;
            Vector2 p4 = point;

            switch(type)
            {
                case 0:

                    p3.x -= Mathf.Cos(direction + Mathf.PI / 2) * sizePointNext;
                    p3.y -= Mathf.Sin(direction + Mathf.PI / 2) * sizePointNext;

                    p4.x -= Mathf.Cos(direction + Mathf.PI / 2) * sizePoint;
                    p4.y -= Mathf.Sin(direction + Mathf.PI / 2) * sizePoint;
                    
                    GL.Color(new Color(1, 0, 0, translucency));
                    GL.Vertex3(p1.x, p1.y, 0);
                    GL.Vertex3(p2.x, p2.y, 0);
                    GL.Vertex3(p3.x, p3.y, 1);

                    GL.Vertex3(p3.x, p3.y, 1);
                    GL.Vertex3(p4.x, p4.y, 1);
                    GL.Vertex3(p1.x, p1.y, 0);

                break;

                case 1:

                    GL.Color(new Color(0, 1, 0, translucency));
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y - sizePointNext, 0);
                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y - sizePointNext, 1);
                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y + sizePointNext, 2);

                    GL.Vertex3(nextPoint.x + sizePointNext, nextPoint.y + sizePointNext, 2);
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y + sizePointNext, 3);
                    GL.Vertex3(nextPoint.x - sizePointNext, nextPoint.y - sizePointNext, 0);

                break;
            }
        }

        static public void DrawFill(DayLightCollider2D id, Vector2 position)
        {
            if (!id.InAnyCamera())
            {
                return;
            }

            GLExtended.color = new Color(0, 0, 1, id.shadowTranslucency);

            Vector2 pos = id.mainShape.transform.position;
            pos.x += position.x;
            pos.y += position.y;

            Vector2 scale = id.mainShape.transform2D.scale;
            float rotation = id.mainShape.transform2D.rotation;
        
            DayLightColliderShape shape = id.mainShape;

            if (!shape.isStatic)
            {
                shape.ResetWorld();
            }

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

            bool softness = id.shadowSoftness > 0 && id.shadowEffect == DayLightCollider2D.ShadowEffect.Softness;

            if (softness)
            {
                List<Polygon2> polygons = shape.GetPolygonsWorld();

                if (polygons == null)
                {
                    return;
                }

                int polygonCount = polygons.Count;

                for(int p = 0; p < polygonCount; p++)
                {
                    Polygon2 polygon = polygons[p];

                    int pointsCount = polygon.points.Length;

                    for(int i = 0; i < pointsCount; i++)
                    {
                        pointA = polygon.points[i];
                        pointA.x += position.x;
                        pointA.y += position.y;

                        pointB = polygon.points[(i + 1) % pointsCount];
                        pointB.x += position.x;
                        pointB.y += position.y;

                        DrawLineTri(pointA, pointB, 0, id.shadowTranslucency, id.shadowSoftness);
                        DrawLineTri(pointA, pointA, 1, id.shadowTranslucency, id.shadowSoftness);
                        DrawLineTri(pointA, pointA, 1, id.shadowTranslucency, id.shadowSoftness);
                    }
                }
            }
        }

        static public void DrawTilemap(DayLightTilemapCollider2D id, Vector2 position, Camera camera)
        {
            //if (id.InAnyCamera() == false) {
            //     continue;
            //}

            if (id.height <= 0)
            {
               // return;
            }

            float distance = shadowDistance * id.height;
            float cosShadow = directionCos * distance;
            float sinShadow = directionSin * distance;

            foreach(DayLightingTile dayTile in id.dayTiles)
            {
                if (!dayTile.InCamera(camera))
                {
                    continue;
                }
                
                List<Polygon2> polygons = dayTile.polygons;

                foreach(Polygon2 polygon in polygons)
                {
                    int pointsCount = polygon.points.Length;

                    for(int i = 0; i < pointsCount; i++ )
                    {
                        pointA = polygon.points[i];
                        pointA.x += position.x;
                        pointA.y += position.y;

                        pointB = polygon.points[(i + 1) % pointsCount];
                        pointB.x += position.x;
                        pointB.y += position.y;

                        pointAOffset.x = pointA.x + cosShadow;
                        pointAOffset.y = pointA.y + sinShadow;
       
                        pointBOffset.x = pointB.x + cosShadow;
                        pointBOffset.y = pointB.y + sinShadow;

                        // soft shadows?
                        // translucency?

                        GL.Color(new Color(0, 0, 1, id.shadowTranslucency));

                        GL.Vertex3(pointA.x, pointA.y, 0);
                        GL.Vertex3(pointAOffset.x, pointAOffset.y, 0);
                        GL.Vertex3(pointBOffset.x, pointBOffset.y, 0);
                        GL.Vertex3(pointB.x, pointB.y, 0);

                        if (id.shadowSoftness > 0)
                        {
                            // only when soft
                            DrawLine(pointAOffset, pointBOffset, 0, id.shadowTranslucency, id.shadowSoftness);
                            DrawLine(pointA, pointAOffset, 0, id.shadowTranslucency, id.shadowSoftness);
                            DrawLine(pointBOffset, pointB, 0, id.shadowTranslucency, id.shadowSoftness);
                            DrawLine(pointA, pointB, 0, id.shadowTranslucency, id.shadowSoftness);

                            DrawLine(pointA, pointA, 1, id.shadowTranslucency, id.shadowSoftness);
                            DrawLine(pointAOffset, pointAOffset, 1, id.shadowTranslucency, id.shadowSoftness);
                        }
                    }   
                }
            }
        }
    }
}