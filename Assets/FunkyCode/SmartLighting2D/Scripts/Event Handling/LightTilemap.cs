using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightingSettings;
using FunkyCode.Utilities;

namespace FunkyCode.EventHandling
{
    public class LightTilemap : Base
    {
        public static Vector2[] removePointsColliding = new Vector2[100];
        public static int removePointsCollidingCount = 0;

        public static LightCollision2D[] removeCollisions = new LightCollision2D[100];
        public static int removeCollisionsCount = 0;

        public static List<LightCollision2D> RemoveHiddenPoints(List<LightCollision2D> collisions, Light2D light, EventPreset eventPreset)
        {
            float lightSizeSquared = Mathf.Sqrt(light.size * light.size + light.size * light.size);
            float rotLeft, rotRight;

            Polygon2 testPolygon = GetPolygon();
            Vector2 lightPosition = - light.transform.position;
            int next;

            for(int iid = 0; iid < eventPreset.layerSetting.list.Length; iid++)
            {
                int layerId = eventPreset.layerSetting.list[iid].layerID;

                var tilemapColliderList = LightTilemapCollider2D.GetShadowList(layerId);
                foreach(var id in tilemapColliderList)
                {
                    var tilemapCollider = id.GetCurrentTilemap();
                    int count = tilemapCollider.chunkManager.GetTiles(light.transform2D.WorldRect);

                    for(int t = 0; t < count; t++)
                    {
                        var tile = tilemapCollider.chunkManager.display[t];

                        if (tile.occluded)
                        {
                            continue;
                        }

                        switch(id.shadowTileType)
                        {
                            case ShadowTileType.AllTiles:
                            break;

                            case ShadowTileType.ColliderOnly:

                                if (tile.colliderType == UnityEngine.Tilemaps.Tile.ColliderType.None)
                                    continue;

                            break;
                        }

                        var polygons = tile.GetWorldPolygons(tilemapCollider);
                        if (polygons.Count < 1)
                            continue;

                        Vector2 tilePosition = tile.GetWorldPosition(tilemapCollider) + lightPosition;
                        if (tile.NotInRange(tilePosition, light.size))
                            continue;

                        removePointsCollidingCount = 0;
                        removeCollisionsCount = 0;

                        for(int i = 0; i < polygons.Count; i++)
                        {
                            Vector2[] pointsList = polygons[i].points;
                            int pointsCount = pointsList.Length;

                            for(int x = 0; x < pointsCount; x++)
                            {
                                next = (x + 1) % pointsCount;

                                Vector2 left = pointsList[x];
                                Vector2 right = pointsList[next];

                                edgeLeft.x = left.x + lightPosition.x;
                                edgeLeft.y = left.y + lightPosition.y;

                                edgeRight.x = right.x + lightPosition.x;
                                edgeRight.y = right.y + lightPosition.y;

                                rotLeft = (float)System.Math.Atan2 (edgeLeft.y, edgeLeft.x);
                                rotRight = (float)System.Math.Atan2 (edgeRight.y, edgeRight.x);
                            
                                projectionLeft.x = edgeLeft.x + (float)System.Math.Cos(rotLeft) * lightSizeSquared;
                                projectionLeft.y = edgeLeft.y + (float)System.Math.Sin(rotLeft) * lightSizeSquared;

                                projectionRight.x = edgeRight.x + (float)System.Math.Cos(rotRight) * lightSizeSquared;
                                projectionRight.y = edgeRight.y + (float)System.Math.Sin(rotRight) * lightSizeSquared;

                                testPolygon.points[0] = projectionLeft;
                                testPolygon.points[1] = projectionRight;
                                testPolygon.points[2] = edgeRight;
                                testPolygon.points[3] = edgeLeft;

                                float collisionCount = collisions.Count;
                                for(int c = 0; c < collisionCount; c++)
                                {
                                    var col = collisions[c];
                                    if (col.collider == id)
                                    {
                                        continue;
                                    }

                                    // Check if event handling objects are inside shadow
                                    // Add it to remove list
                                    int pCount = col.points.Count;
                                    for(int p = 0; p < pCount; p++)
                                    {
                                        Vector2 point = col.points[p];

                                        if (Math2D.PointInPoly(point, testPolygon))
                                        {
                                            removePointsColliding[removePointsCollidingCount] = point;
                                            removePointsCollidingCount++;
                                        }
                                    }

                                    // Remove Event Handling points with remove list
                                    for(int p = 0; p < removePointsCollidingCount; p++)
                                    {
                                        col.points.Remove(removePointsColliding[p]);
                                    }

                                    removePointsCollidingCount = 0;

                                    // If there is no points left
                                    // collision object should be removed
                                    if (col.points.Count < 1)
                                    {
                                        removeCollisions[removeCollisionsCount] = col;
                                        removeCollisionsCount ++;
                                    }
                                }

                                for(int p = 0; p < removeCollisionsCount; p++)
                                {
                                    collisions.Remove(removeCollisions[p]);
                                }

                                removeCollisionsCount = 0;
                            }
                        }
                    }
                }
            }
            
            return(collisions);
        }
    }
}