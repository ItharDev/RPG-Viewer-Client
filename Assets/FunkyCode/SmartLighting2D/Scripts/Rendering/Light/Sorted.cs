using UnityEngine;
using FunkyCode.LightTilemapCollider;

namespace FunkyCode.Rendering.Light
{
    public static class Sorted
    {
        public static void Draw(Pass pass)
        {
            for(int i = 0; i < pass.sortPass.sortList.Count; i ++)
            {
                pass.sortPass.sortObject = pass.sortPass.sortList.List[i];
                var sortObject = pass.sortPass.sortObject;
                var lightObject = sortObject.LightObject;

                if (lightObject is LightCollider2D lightCollider)
                    DrawCollider(lightCollider, pass);
                else if (lightObject is LightTile lightTile)
                    DrawTile(lightTile, sortObject.Tilemap,pass);
                else if (lightObject is LightTilemapCollider2D lightTilemap)
                    DrawTileMap(lightTilemap, pass);
            }
        }

        private static void DrawCollider(LightCollider2D collider, Pass pass) 
        {
            Material material;

            if (collider.shadowLayer == pass.layerID && pass.drawShadows)
            {	
                if (!collider.ShadowDisabled())
                {
                    ShadowEngine.GetMaterial().SetPass(0);

                    GL.Begin(GL.TRIANGLES);

                    FunkyCode.Rendering.Light.Shadow.Shape.Draw(pass.light, collider);
        
                    GL.End();
                }

            }

            // Masking
            if (collider.maskLayer == pass.layerID && pass.drawMask)
            {
                switch(collider.mainShape.maskType)
                {
                    case LightCollider2D.MaskType.SpritePhysicsShape:
                    case LightCollider2D.MaskType.Collider2D:
                    case LightCollider2D.MaskType.Collider3D:
                    case LightCollider2D.MaskType.CompositeCollider2D:

                        GLExtended.color = Color.white;
                        pass.materialMask.SetPass(0);

                        GL.Begin(GL.TRIANGLES);
                            Shape.Mask(pass.light, collider, pass.layer);
                        GL.End();

                    break;

                    case LightCollider2D.MaskType.Sprite:

                        SpriteRenderer2D.currentTexture = null;
                        
                        SpriteRenderer2D.Mask(pass.light, collider, pass.materialMask, pass.layer);

                        if (SpriteRenderer2D.currentTexture != null)
                        {
                            SpriteRenderer2D.currentTexture = null;
                            GL.End();
                        }
                        
                        break;

                    case LightCollider2D.MaskType.BumpedSprite:
                
                        material = collider.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                        SpriteRenderer2D.MaskBumped(pass.light, collider, material, pass.layer);
                        
                        break;

                    case LightCollider2D.MaskType.MeshRenderer:

                        GLExtended.color = Color.white;
                        pass.materialMask.SetPass(0);

                        GL.Begin(GL.TRIANGLES);
                            Mesh.Mask(pass.light, collider, pass.materialMask, pass.layer);
                        GL.End();

                    break;

                    case LightCollider2D.MaskType.BumpedMeshRenderer:

                        material = collider.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                        material.SetPass(0);

                        GL.Begin(GL.TRIANGLES);
                            Mesh.MaskNormalMap(pass.light, collider, material, pass.layer);
                        GL.End();

                        break;

                    case LightCollider2D.MaskType.SkinnedMeshRenderer:

                        pass.materialMask.SetPass(0);

                        GL.Begin(GL.TRIANGLES);
                            SkinnedMesh.Mask(pass.light, collider, pass.materialMask, pass.layer);
                        GL.End();

                    break;
                }
            }
        }

        private static void DrawTile(LightTile tile, LightTilemapCollider2D tilemap, Pass pass)
        {
            bool masksDisabled = tilemap.MasksDisabled();
            bool shadowsDisabled = tilemap.ShadowsDisabled();
            
            if (tilemap.shadowLayer == pass.layerID && pass.drawShadows && shadowsDisabled == false)
            {

                ShadowEngine.GetMaterial().SetPass(0);

                GL.Begin(GL.TRIANGLES);

                Shadow.Tile.Draw(pass.light, tile, tilemap);
    
                GL.End();
            }

            // sprite mask - but what about shape mask?
            if (tilemap.maskLayer == pass.layerID && pass.drawMask && masksDisabled == false)
            {
                GL.Begin(GL.QUADS);

                Tile.MaskSprite(tile, pass.layer, pass.materialMask, tilemap, pass.lightSizeSquared);
                
                GL.End();
            }       
        }

        private static void DrawTileMap(LightTilemapCollider2D tilemap, Pass pass)
        {
            bool masksDisabled = tilemap.MasksDisabled();
            bool shadowsDisabled = tilemap.ShadowsDisabled();
            
            if (tilemap.shadowLayer == pass.layerID && pass.drawShadows && shadowsDisabled == false)
            {	
                ShadowEngine.GetMaterial().SetPass(0);
                                    
                GL.Begin(GL.TRIANGLES);

                    switch(tilemap.mapType)
                    {
                    case MapType.UnityRectangle:

                        switch(tilemap.rectangle.shadowType)
                        {
                            case ShadowType.Grid:
                            case ShadowType.SpritePhysicsShape:
                                Shadow.UnityTilemap.Draw(pass.light, tilemap);
                            break;

                            case ShadowType.CompositeCollider:
                                Shadow.TilemapCollider.Rectangle.Draw(pass.light, tilemap);
                            break;
                        }
                        
                    break;

                    case MapType.UnityIsometric:

                        switch(tilemap.isometric.shadowType)
                        {
                            case ShadowType.Grid:
                            case ShadowType.SpritePhysicsShape:
                                    Shadow.UnityTilemap.Draw(pass.light, tilemap);
                            break;
                        }
                        
                    break;

                    case MapType.UnityHexagon:

                        switch(tilemap.hexagon.shadowType)
                        {
                            case ShadowType.Grid:
                            case ShadowType.SpritePhysicsShape:
                                    Shadow.UnityTilemap.Draw(pass.light, tilemap);
                            break;
                        }
                        
                    break;

                    case MapType.SuperTilemapEditor:

                        switch(tilemap.superTilemapEditor.shadowTypeSTE)
                        {

                            case SuperTilemapEditorSupport.TilemapCollider2D.ShadowType.TileCollider:
                            case SuperTilemapEditorSupport.TilemapCollider2D.ShadowType.Grid:
                                    SuperTilemapEditorSupport.Light.Shadow.Grid.Draw(pass.light, tilemap);
                                break;
                                
                            case SuperTilemapEditorSupport.TilemapCollider2D.ShadowType.Collider:
                                    SuperTilemapEditorSupport.Light.Shadow.Collider.Draw(pass.light, tilemap);
                                break;
                            }
                        
                    break;
                }

                GL.End();  
            }

            if (tilemap.maskLayer == pass.layerID && pass.drawMask && masksDisabled == false)
            {
                switch(tilemap.mapType)
                {
                    case MapType.UnityRectangle:

                        switch(tilemap.rectangle.maskType)
                        {
                            case LightTilemapCollider.MaskType.Sprite:
                                UnityTilemap.Sprite(pass.light, tilemap, pass.materialMask, pass.layer);
                            break;
                            
                            case LightTilemapCollider.MaskType.BumpedSprite:

                                Material material = tilemap.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                                UnityTilemap.BumpedSprite(pass.light, tilemap, material, pass.layer);
                        
                            break;
                        }
                        
                    break;

                    case MapType.UnityIsometric:

                        switch(tilemap.isometric.maskType)
                        {
                            case MaskType.Sprite:
                                UnityTilemap.Sprite(pass.light, tilemap, pass.materialMask, pass.layer);
                            break;
                        }
                        
                    break;

                    case MapType.UnityHexagon:

                        switch(tilemap.hexagon.maskType)
                        {
                            case MaskType.Sprite:
                            //    TilemapHexagon.MaskSprite(pass.buffer, tilemap, pass.materialMask, pass.z);
                            break;
                        }
                        
                    break;

                    case MapType.SuperTilemapEditor:

                        switch(tilemap.superTilemapEditor.maskTypeSTE)
                        {
                            case SuperTilemapEditorSupport.TilemapCollider.MaskType.Sprite:

                                SuperTilemapEditorSupport.Light.Mask.SpriteRenderer2D.Sprite(pass.light, tilemap, pass.materialMask);
                            
                            break;
                            
                            case SuperTilemapEditorSupport.TilemapCollider.MaskType.BumpedSprite:

                                Material material = tilemap.bumpMapMode.SelectMaterial(pass.materialNormalMap_PixelToLight, pass.materialNormalMap_ObjectToLight);
                                SuperTilemapEditorSupport.Light.Mask.SpriteRenderer2D.BumpedSprite(pass.light, tilemap, material);
                        
                            break;
                        }

                    break;
                }   
            }
        }
    }
}