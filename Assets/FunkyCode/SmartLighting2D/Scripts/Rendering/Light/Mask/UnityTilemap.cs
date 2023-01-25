using UnityEngine;
using FunkyCode.LightSettings;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light
{
    public class UnityTilemap
    {
        public static VirtualSpriteRenderer virtualSpriteRenderer = new VirtualSpriteRenderer();
        
        static public void Sprite(Light2D light, LightTilemapCollider2D id, Material material, LayerSetting layerSetting) {
            Vector2 lightPosition = -light.transform.position;

            LightTilemapCollider.Base tilemap = id.GetCurrentTilemap();
            Vector2 scale = tilemap.TileWorldScale();
            Vector2 localScale = Vector2.zero;
            float rotation = id.transform.eulerAngles.z;
            
            int count = tilemap.chunkManager.GetTiles(light.transform2D.WorldRect);

            Texture2D currentTexture = null;
            
            for(int i = 0; i < count; i++) {
                LightTile tile = tilemap.chunkManager.display[i];

                if (tile.GetSprite() == null) {
                    return;
                }

                Vector2 tilePosition = tile.GetWorldPosition(tilemap);

                tilePosition.x += lightPosition.x;
                tilePosition.y += lightPosition.y;

                if (tile.NotInRange(tilePosition, light.size)) {
                    continue;
                }

                virtualSpriteRenderer.sprite = tile.GetSprite();

                Texture2D texture = virtualSpriteRenderer.sprite.texture;

                if (texture == null) {
                    continue;
                }
                
                if (currentTexture != texture) {
                    if (currentTexture != null) {
                        GL.End();
                    }

                    currentTexture = texture;
                    material.mainTexture = currentTexture;

                    material.SetPass(0);
                    GL.Begin(GL.QUADS);
                }

                GLExtended.color = LayerSettingColor.Get(tilePosition, layerSetting, MaskLit.Lit, 1, 1);

                localScale.x = scale.x * tile.scale.x;
                localScale.y = scale.y * tile.scale.y;
    
                Universal.Sprite.Pass.Draw(tile.spriteMeshObject, virtualSpriteRenderer, tilePosition, localScale, rotation + tile.rotation);
            }

            if (currentTexture != null) {
                GL.End();
            }

            material.mainTexture = null;
        }

        static public void BumpedSprite(Light2D light, LightTilemapCollider2D id, Material material, LayerSetting layerSetting) {
            Texture bumpTexture = id.bumpMapMode.GetBumpTexture();

            if (bumpTexture == null) {
                return;
            }

            material.SetTexture("_Bump", bumpTexture);

            Vector2 lightPosition = -light.transform.position;

            LightTilemapCollider.Base tilemap = id.GetCurrentTilemap();
            Vector2 scale = tilemap.TileWorldScale();

            Texture2D currentTexture = null;

            GL.Begin(GL.QUADS);

            // Optimize Bumped Sprites?

            int count = tilemap.chunkManager.GetTiles(light.transform2D.WorldRect);

            for(int i = 0; i < count; i++) {
                LightTile tile = tilemap.chunkManager.display[i];

                if (tile.GetSprite() == null) {
                    return;
                }

                Vector2 tilePosition = tilemap.TileWorldPosition(tile);

                tilePosition += lightPosition;

                if (tile.NotInRange(tilePosition, light.size)) {
                    continue;
                }

                virtualSpriteRenderer.sprite = tile.GetSprite();

                if (virtualSpriteRenderer.sprite.texture == null) {
                    continue;
                }

                if (currentTexture != virtualSpriteRenderer.sprite.texture) {
                    currentTexture = virtualSpriteRenderer.sprite.texture;
                    material.mainTexture = currentTexture;

                    material.SetPass(0);
                }

                GLExtended.color = LayerSettingColor.Get(tilePosition, layerSetting, MaskLit.Lit, 1, 1); // 1
    
                Universal.Sprite.FullRect.Simple.Draw(tile.spriteMeshObject, virtualSpriteRenderer, tilePosition, scale, tile.worldRotation);
            }

            GL.End();

            material.mainTexture = null;
        }

        static public void MaskShape(Light2D light, LightTilemapCollider2D id, LayerSetting layerSetting) {
            Vector2 lightPosition = -light.transform.position;
         
            LightTilemapCollider.Base tilemap = id.GetCurrentTilemap();

            bool isGrid = !tilemap.IsPhysicsShape();

            Vector2 scale = tilemap.TileWorldScale();

            float rotation = id.transform.eulerAngles.z;

            MeshObject tileMesh = null;	
            
            if (isGrid) {
                tileMesh = LightTile.GetStaticMesh(tilemap);
            }

            int count = tilemap.chunkManager.GetTiles(light.transform2D.WorldRect);

            for(int i = 0; i < count; i++) {
                LightTile tile = tilemap.chunkManager.display[i];

                Vector2 tilePosition = tilemap.TileWorldPosition(tile);

                tilePosition += lightPosition;
                
                if (tile.NotInRange(tilePosition, light.size)) {
                    continue;
                }

                if (isGrid == false) {
                    tileMesh = null;
                    tileMesh = tile.GetDynamicMesh();
                }

                if (tileMesh == null) {
                    continue;
                }

                GLExtended.color = LayerSettingColor.Get(tilePosition, layerSetting, MaskLit.Lit, 1, 1); // 1?

                GLExtended.DrawMeshPass(tileMesh, tilePosition, scale, rotation + tile.rotation);		
            }

            GL.Color(Color.white);

        }
    }
}