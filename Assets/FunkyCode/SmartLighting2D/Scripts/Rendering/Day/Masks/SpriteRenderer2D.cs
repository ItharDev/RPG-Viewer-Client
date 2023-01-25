using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Day
{
    public static class SpriteRenderer2D
	{
		static public Texture2D currentTexture = null;

		static public void Draw(DayLightCollider2D id, Vector2 offset)
		{
			if (!id.InAnyCamera())
			{
				return;
			}

			Material material = Lighting2D.Materials.mask.GetDayMask();

			GLExtended.color = DayMaskColor.Get(id);

			DayLightColliderShape shape = id.mainShape;

			UnityEngine.SpriteRenderer spriteRenderer = shape.spriteShape.GetSpriteRenderer();

			if (spriteRenderer == null || spriteRenderer.sprite == null) {
				return;
			}

			Texture2D texture = spriteRenderer.sprite.texture;

			if (texture == null) {
				return;
			}

			if (currentTexture != texture)
			{
				if (currentTexture != null)
				{
					GL.End();
				}

				currentTexture = texture;
				material.mainTexture = currentTexture;

				material.SetPass(0);
				GL.Begin(GL.QUADS);
			}
				
			Vector2 position = shape.transform2D.position;
			position.x += offset.x;
			position.y += offset.y;

			Universal.Sprite.Pass.Draw(id.spriteMeshObject, spriteRenderer, position, shape.transform2D.scale, shape.transform2D.rotation);
		}

		static VirtualSpriteRenderer virtualSpriteRenderer = new VirtualSpriteRenderer();
    
		static public void DrawTilemap(DayLightTilemapCollider2D id, Vector2 offset)
		{
			//if (id.InAnyCamera() == false) {
			//	return;
			//}

			if (id.rectangle.maskType != LightTilemapCollider.MaskType.Sprite)
			{
				return;
			}

			LightTilemapCollider.Base tilemap = id.GetCurrentTilemap();

			Vector2 scale = tilemap.TileWorldScale();
            float rotation = id.transform.eulerAngles.z;

			Material material = Lighting2D.Materials.mask.GetMask(); // why not day mask?

            foreach(LightTile tile in id.rectangle.MapTiles)
			{
                if (tile.GetSprite() == null)
				{
                    return;
                }

				tile.UpdateTransform(tilemap);

                Vector2 tilePosition = tile.GetWorldPosition(tilemap);

                tilePosition += offset;

               // if (tile.NotInRange(tilePosition, light.size)) {
                 //   continue;
                //}

                virtualSpriteRenderer.sprite = tile.GetSprite();

                GLExtended.color = Color.white;
				
                material.mainTexture = virtualSpriteRenderer.sprite.texture;

				if (id.maskLit == DayLightTilemapCollider2D.MaskLit.Lit) {
					GLExtended.color = Color.white;
				} else {
					GLExtended.color = Color.black;
				}

				material.SetPass(0);
    
                Universal.Sprite.FullRect.Simple.Draw(tile.spriteMeshObject, virtualSpriteRenderer, tilePosition, scale, rotation);
                
                material.mainTexture = null;
            }
		}

		static public void DrawBumped(DayLightCollider2D id, Vector2 offset)
		{
			if (!id.InAnyCamera())
			{
				return;
			}

			Texture bumpTexture = id.normalMapMode.GetBumpTexture();

			if (bumpTexture == null)
			{
				return;
			}
			
			float dayLightRotation = -(Lighting2D.DayLightingSettings.direction - 180) * Mathf.Deg2Rad;
			float dayLightHeight = Lighting2D.DayLightingSettings.bumpMap.height;
			float dayLightStrength = Lighting2D.DayLightingSettings.bumpMap.strength;

			Material material = Lighting2D.Materials.bumpMask.GetBumpedDaySprite();
			material.SetFloat("_LightRZ", -dayLightHeight);
			material.SetTexture("_Bump", bumpTexture);

			DayLightColliderShape shape = id.mainShape;
		
			UnityEngine.SpriteRenderer spriteRenderer = shape.spriteShape.GetSpriteRenderer();

			if (spriteRenderer == null || spriteRenderer.sprite == null) {
				return;
			}

			float rotation = dayLightRotation - shape.transform2D.rotation * Mathf.Deg2Rad;
			material.SetFloat("_LightRX", Mathf.Cos(rotation) * dayLightStrength);
			material.SetFloat("_LightRY", Mathf.Sin(rotation) * dayLightStrength);
				
			Vector2 objectOffset = shape.transform2D.position + offset;

			material.mainTexture = spriteRenderer.sprite.texture;

			material.SetPass(0);

			Universal.Sprite.FullRect.Draw(id.spriteMeshObject, spriteRenderer, objectOffset, id.transform.lossyScale, shape.transform2D.rotation);
		}
	}
}