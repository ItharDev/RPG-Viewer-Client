using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightSettings;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light
{
    public class Tile {
		public static VirtualSpriteRenderer virtualSpriteRenderer = new VirtualSpriteRenderer();

       	static public void MaskSprite(LightTile tile, LayerSetting layerSetting, Material material, LightTilemapCollider2D tilemap, float lightSizeSquared) {
			virtualSpriteRenderer.sprite = tile.GetSprite();

			if (virtualSpriteRenderer.sprite == null) {
				return;
			}

			LightTilemapCollider.Base tilemapBase = tilemap.GetCurrentTilemap();

			Vector2 tilePosition = tile.GetWorldPosition(tilemapBase) - ShadowEngine.light.transform2D.position;

			GLExtended.color = LayerSettingColor.Get(tilePosition, layerSetting, MaskLit.Lit, 1, 1); // 1?

			material.mainTexture = virtualSpriteRenderer.sprite.texture;

			Vector2 scale = tile.worldScale * tile.scale;

			GLExtended.color = Color.white;

			tilePosition.x += ShadowEngine.drawOffset.x;
			tilePosition.y += ShadowEngine.drawOffset.y;

			material.SetPass(0);

			Universal.Sprite.Pass.Draw(tile.spriteMeshObject, virtualSpriteRenderer, tilePosition, scale, tile.worldRotation);
			
			material.mainTexture = null;
		}
    }
}