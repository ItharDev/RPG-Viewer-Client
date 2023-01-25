using UnityEngine;

 #if (SUPER_TILEMAP_EDITOR)

    namespace FunkyCode.SuperTilemapEditorSupport.Light.Mask
    { 
        public static class Grid
        {
            static public void Draw(Light2D light, LightTilemapCollider2D id)
            {
                if (id.superTilemapEditor.maskTypeSTE != SuperTilemapEditorSupport.TilemapCollider2D.MaskType.Grid)
                {
                    return;
                }

                Vector2 lightPosition = -light.transform.position;
                MeshObject tileMesh = LightTile.Rectangle.GetStaticMesh();
          
                GL.Color(Color.white);

                LightTilemapCollider.Base tilemapBase = id.GetCurrentTilemap();

                foreach(LightTile tile in id.superTilemapEditor.MapTiles)
                {
                    Vector2 tilePosition = tile.GetWorldPosition(tilemapBase);
                    tilePosition += lightPosition;
                    
                    if (tile.NotInRange(tilePosition, light.size))
                    {
                        continue;
                    }

                    GLExtended.DrawMeshPass(tileMesh, tilePosition, tile.worldScale, tile.worldRotation);		
                }
            }

        }
    }

#else  

    namespace FunkyCode.SuperTilemapEditorSupport.Light.Mask
    { 
        public class Grid
        {
            static public void Draw(Light2D light, LightTilemapCollider2D id) {}
        }
    }

#endif