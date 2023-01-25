using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Universal
{
	public class Sprite : Base
	{
        static VirtualSpriteRenderer virtualSpriteRenderer = new VirtualSpriteRenderer();

		public static class Pass
		{
			static public void Draw(SpriteMeshObject spriteMeshObject, SpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation)
			{
				virtualSpriteRenderer.Set(spriteRenderer);
			
				Pass.Draw(spriteMeshObject, virtualSpriteRenderer, position, scale, rotation);
			}

			static public void Draw(SpriteMeshObject spriteMeshObject, VirtualSpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation)
			{
				SpriteTransform spriteTransform = new SpriteTransform(spriteRenderer, position, scale, rotation);

				Texture.Quad.Sprite.DrawPass(spriteTransform.position, spriteTransform.scale, spriteTransform.uv, rotation);
			}
		}

		public static class MultiPass
		{
			static public void Draw(SpriteMeshObject spriteMeshObject, SpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation)
			{
				virtualSpriteRenderer.Set(spriteRenderer);
			
				MultiPass.Draw(spriteMeshObject, virtualSpriteRenderer, position, scale, rotation);
			}

			static public void Draw(SpriteMeshObject spriteMeshObject, VirtualSpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation)
			{
				SpriteTransform spriteTransform = new SpriteTransform(spriteRenderer, position, scale, rotation);

				Texture.Quad.Sprite.DrawMultiPass(spriteTransform.position, spriteTransform.scale, spriteTransform.uv, rotation);
			}
		}

		static  public void Draw(SpriteMeshObject spriteMeshObject, SpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation)
		{
			if (spriteRenderer == null)
			{
				return;
			}

			if (spriteRenderer.sprite == null)
			{
				return;
			}
			
			//if (spriteRenderer.sprite.packingMode == SpritePackingMode.Tight) {
				// FullRect.Draw(spriteMeshObject, material, spriteRenderer, position, scale,  rotation);
			//} else {
				FullRect.Draw(spriteMeshObject, spriteRenderer, position, scale,  rotation);
			//}
		}

		public class Tight {
			// ??
		}

		public class FullRect {

			public class Simple {
				
				static public void Draw(SpriteMeshObject spriteMeshObject, SpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation) {
					virtualSpriteRenderer.Set(spriteRenderer);
					
					Draw(spriteMeshObject, virtualSpriteRenderer, position, scale, rotation);
				}

				static public void Draw(SpriteMeshObject spriteMeshObject, VirtualSpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation) {
					SpriteTransform spriteTransform = new SpriteTransform(spriteRenderer, position, scale, rotation);

					Texture.Quad.Sprite.Draw(spriteTransform.position, spriteTransform.scale, spriteTransform.uv, rotation);
				}
			}

			public class Tiled {
				static public void Draw(SpriteMeshObject spriteMeshObject, SpriteRenderer spriteRenderer, Vector2 pos, Vector2 size, float rotation) {
					GLExtended.DrawMesh(spriteMeshObject.GetTiledMesh().GetMesh(spriteRenderer), pos, size, rotation);
				}
			}

			static public void Draw(SpriteMeshObject spriteMeshObject, SpriteRenderer spriteRenderer, Vector2 pos, Vector2 size, float rotation) {
				if (spriteRenderer.drawMode == SpriteDrawMode.Tiled && spriteRenderer.tileMode == SpriteTileMode.Continuous) {
					Tiled.Draw(spriteMeshObject, spriteRenderer, pos, size, rotation);
				} else {
					Simple.Draw(spriteMeshObject, spriteRenderer, pos, size, rotation);
				}
			}

			static public void DrawPass(SpriteMeshObject spriteMeshObject, SpriteRenderer spriteRenderer, Vector2 pos, Vector2 size, float rotation) {
				if (spriteRenderer.drawMode == SpriteDrawMode.Tiled && spriteRenderer.tileMode == SpriteTileMode.Continuous) {
					Tiled.Draw(spriteMeshObject, spriteRenderer, pos, size, rotation);
				} else {
					Simple.Draw(spriteMeshObject, spriteRenderer, pos, size, rotation);
				}
			}
		}	
    }
}