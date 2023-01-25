using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Depth
{
    public static class SpriteRendererShadow
    {
        static VirtualSpriteRenderer virtualSpriteRenderer = new VirtualSpriteRenderer();

        public static Texture2D currentTexture;
        public static Material material;

        public static Vector2 cameraOffset;
        public static float direction;
        public static float shadowDistance;

        static public void Begin(Vector2 offset)
        {
            material = Lighting2D.Materials.shadow.GetDepthDayShadow();
            material.mainTexture = null;

            SpriteRendererShadow.currentTexture = null;

            cameraOffset = offset;
            direction = -Lighting2D.DayLightingSettings.direction * Mathf.Deg2Rad;
            shadowDistance = Lighting2D.DayLightingSettings.height;
        }

        static public void End()
        {
            GL.End();

            material.mainTexture = null;
            SpriteRendererShadow.currentTexture = null;
        }

        static public void DrawOffset(DayLightCollider2D id)
        {
            if (!id.InAnyCamera())
            {
                return;
            }

            Vector2 scale = new Vector2(id.transform.lossyScale.x, id.transform.lossyScale.y);

            DayLightColliderShape shape = id.mainShape;
        
            SpriteRenderer spriteRenderer = shape.spriteShape.GetSpriteRenderer();
            
            if (spriteRenderer == null)
            {
                return;
            }
            
            virtualSpriteRenderer.sprite = spriteRenderer.sprite;
            virtualSpriteRenderer.flipX = spriteRenderer.flipX;
            virtualSpriteRenderer.flipY = spriteRenderer.flipY;

            if (virtualSpriteRenderer.sprite == null)
            {
                return;
            }

            Texture2D texture = virtualSpriteRenderer.sprite.texture;

            if (texture == null)
            {
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
        
            Vector2 position = new Vector2(id.transform.position.x + cameraOffset.x, id.transform.position.y + cameraOffset.y);
            position.x += Mathf.Cos(direction) * id.mainShape.height * shadowDistance;
            position.y += Mathf.Sin(direction) * id.mainShape.height * shadowDistance;

            float depth = (100f + (float)id.GetDepth()) / 255;

            GLExtended.color = new Color(depth, 0, 0, 1 - id.shadowTranslucency);

            Universal.Sprite.Pass.Draw(id.spriteMeshObject, virtualSpriteRenderer, position, scale, id.transform.rotation.eulerAngles.z);
        }
    }
}