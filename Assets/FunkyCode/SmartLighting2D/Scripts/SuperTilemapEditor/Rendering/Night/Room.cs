using UnityEngine;

#if (SUPER_TILEMAP_EDITOR)

    namespace FunkyCode.SuperTilemapEditorSupport.Lightmap
    {
        public class Room
        {
            public static void DrawTiles(Camera camera, LightTilemapRoom2D id, Material material)
            {
                Vector2 cameraPosition = -camera.transform.position;

                float cameraRadius = CameraTransform.GetRadius(camera);

                if (id.superTilemapEditor.tilemap == null) {
                    return;
                }

                if (id.superTilemapEditor.tilemap.Tileset != null) {
                    material.mainTexture = id.superTilemapEditor.tilemap.Tileset.AtlasTexture;
                }

                GLExtended.color = id.color;
                LightTilemapCollider.Base tilemapCollider = id.GetCurrentTilemap();

                material.SetPass (0); 
                GL.Begin (GL.QUADS);

                int count = id.superTilemapEditor.chunkManager.GetTiles( CameraTransform.GetWorldRect(camera) );

                for(int i = 0; i < count; i++) {
                    LightTile tile = id.superTilemapEditor.chunkManager.display[i];

                    Vector2 tilePosition = tile.GetWorldPosition(tilemapCollider);
                    
                    tilePosition += cameraPosition;

                    if (tile.NotInRange(tilePosition, cameraRadius)) {
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

    namespace FunkyCode.SuperTilemapEditorSupport.Lightmap
    {
        public class Room {
            public static void DrawTiles(Camera camera, LightTilemapRoom2D id, Material materia) {}
        }
    }

#endif