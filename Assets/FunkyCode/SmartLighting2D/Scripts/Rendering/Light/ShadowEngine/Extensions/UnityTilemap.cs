using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light.Shadow
{
    public class UnityTilemap
    {
        static public void Draw(Light2D light, LightTilemapCollider2D id)
        {
            Vector2 lightPosition = -light.transform.position;
            LightTilemapCollider.Base tilemapCollider = id.GetCurrentTilemap();

            int count = tilemapCollider.chunkManager.GetTiles(light.transform2D.WorldRect);

            Vector2 localPosition;

            for(int i = 0; i < count; i++)
            {
                LightTile tile = tilemapCollider.chunkManager.display[i];

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
                        {
                            continue;
                        }
                    break;
                }

                List<Polygon2> polygons = tile.GetWorldPolygons(tilemapCollider);
                Vector2 tilePosition = tile.GetWorldPosition(tilemapCollider);

                localPosition.x = lightPosition.x + tilePosition.x;
                localPosition.y = lightPosition.y + tilePosition.y;

                if (tile.NotInRange(localPosition, light.size))
                {
                    continue;
                }

                ShadowEngine.Draw(polygons, 0, 0, id.shadowTranslucency);
            }
        }
    }
}