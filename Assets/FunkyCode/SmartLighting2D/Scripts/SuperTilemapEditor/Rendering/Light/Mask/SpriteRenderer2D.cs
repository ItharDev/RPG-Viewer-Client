using UnityEngine;

 #if (SUPER_TILEMAP_EDITOR)

    namespace FunkyCode.SuperTilemapEditorSupport.Light.Mask
    {
        public static class SpriteRenderer2D
        {   
            static public void Sprite(Light2D light, LightTilemapCollider2D id, Material material)
            {
                Vector2 lightPosition = -light.transform.position;
                LightTilemapCollider.Base tilemapCollider = id.GetCurrentTilemap();

                if (id.superTilemapEditor.tilemap != null)
                {
                    if (id.superTilemapEditor.tilemap.Tileset != null)
                    {
                        material.mainTexture = id.superTilemapEditor.tilemap.Tileset.AtlasTexture;
                    }
                }
            
                material.SetPass (0); 
                GL.Begin (GL.QUADS);
    
                int count = tilemapCollider.chunkManager.GetTiles(light.transform2D.WorldRect);

                for(int i = 0; i < count; i++)
                {
                    LightTile tile = tilemapCollider.chunkManager.display[i];

                    tile.UpdateTransform(tilemapCollider);
                    
                    Vector2 tilePosition = tile.GetWorldPosition(tilemapCollider);
                    tilePosition += lightPosition;

                    if (tile.NotInRange(tilePosition, light.size))
                    {
                        continue;
                    }

                    Vector2 scale = tile.worldScale * 0.5f * tile.scale;
                
                    Rendering.Universal.Texture.Quad.STE.DrawPass(tilePosition, scale, tile.uv, tile.worldRotation);
                }

                GL.End ();

                material.mainTexture = null;
            }

            static public void BumpedSprite(Light2D light, LightTilemapCollider2D id, Material material)
            {
                Texture bumpTexture = id.bumpMapMode.GetBumpTexture();

                if (bumpTexture == null)
                {
                    return;
                }

                material.SetTexture("_Bump", bumpTexture);
                
                Vector2 lightPosition = -light.transform.position;
                LightTilemapCollider.Base tilemapCollider = id.GetCurrentTilemap();

                if (id.superTilemapEditor.tilemap != null)
                {
                    if (id.superTilemapEditor.tilemap.Tileset != null)
                    {
                        material.mainTexture = id.superTilemapEditor.tilemap.Tileset.AtlasTexture;
                    }
                }
            
                material.SetPass (0); 
                GL.Begin (GL.QUADS);
    
                foreach(LightTile tile in id.superTilemapEditor.MapTiles)
                {
                    tile.UpdateTransform(tilemapCollider);
                    
                    Vector2 tilePosition = tile.GetWorldPosition(tilemapCollider);
                    tilePosition += lightPosition;

                    if (tile.NotInRange(tilePosition, light.size))
                    {
                        continue;
                    }

                    Vector2 scale = tile.worldScale * 0.5f * tile.scale;

                    Rendering.Universal.Texture.Quad.STE.DrawPass(tilePosition, scale, tile.uv, tile.worldRotation);
                }

                GL.End ();

                material.mainTexture = null;
            }
        }
    }

#else  

    namespace FunkyCode.SuperTilemapEditorSupport.Light.Mask
    { 
        public class SpriteRenderer2D
        {
            static public void Sprite(Light2D light, LightTilemapCollider2D id, Material material) {}
            static public void BumpedSprite(Light2D light, LightTilemapCollider2D id, Material material) {}
        }
    }

#endif