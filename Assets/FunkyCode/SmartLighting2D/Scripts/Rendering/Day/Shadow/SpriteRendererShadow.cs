using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Day
{
    public static class SpriteRendererShadow
    {
        static VirtualSpriteRenderer virtualSpriteRenderer = new VirtualSpriteRenderer();

        public static Texture2D currentTexture;
        public static Material material;

        public static Vector2 cameraOffset;
        public static float direction;
        public static float shadowDistance;

        public static Pair2 pair = Pair2.Zero();

        static public void Begin(Vector2 offset)
        {
            material = Lighting2D.Materials.shadow.GetSpriteShadow();
            material.SetColor("_Darkness", Lighting2D.DayLightingSettings.ShadowColor);
            
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

            GLExtended.color = new Color(0, 0, 0, 1 - id.shadowTranslucency);

            Universal.Sprite.Pass.Draw(id.spriteMeshObject, virtualSpriteRenderer, position, scale, id.transform.rotation.eulerAngles.z);
        }

        static public void DrawProjection(DayLightCollider2D id)
        {
            if (!id.InAnyCamera())
            {
                return;
            }

            Vector2 pos = new Vector2(id.transform.position.x + cameraOffset.x, id.transform.position.y + cameraOffset.y);
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

            Sprite sprite = virtualSpriteRenderer.sprite;

            if (sprite == null)
            {
                return;
            }

            Texture2D texture = sprite.texture;

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
        
            SpriteTransform spriteTransform = new SpriteTransform(virtualSpriteRenderer, pos, scale, id.transform.rotation.eulerAngles.z);

            Rect uv = spriteTransform.uv;

            float pivotY = (float)sprite.pivot.y / sprite.texture.height;
            pivotY = uv.y + pivotY;

            float pivotX = (float)sprite.pivot.x / sprite.texture.width;

            pair.A = Vector2.zero;
            pair.B = Vector2.zero;
     
            pair.A = pos + pair.A.Push(direction + Mathf.PI / 2, id.shadowThickness);
            pair.B = pos + pair.B.Push(direction - Mathf.PI / 2, id.shadowThickness);

            if (Lighting2D.DayLightingSettings.direction < 180)
            {
                float uvx = uv.x;
                uv.x = uv.width;
                uv.width = uvx;
            }

            Vector2 v1 = pair.A;
            Vector2 v2 = pair.A;
            Vector2 v3 = pair.B;
            Vector2 v4 = pair.B;

            v2 = v2.Push(direction, id.shadowDistance * shadowDistance);
            v3 = v3.Push(direction, id.shadowDistance * shadowDistance);

            GL.Color(new Color(0, 0, 0, 1 - id.shadowTranslucency));

            GL.TexCoord3 (uv.x, pivotY, 0);
            GL.Vertex3 (v1.x, v1.y, 0);

            GL.TexCoord3 (uv.x, uv.height, 0);
            GL.Vertex3 (v2.x, v2.y, 0);

            GL.TexCoord3 (uv.width, uv.height, 0);
            GL.Vertex3 (v3.x, v3.y, 0);

            GL.TexCoord3 (uv.width, pivotY, 0);
            GL.Vertex3 (v4.x, v4.y, 0);
        }

        static public void DrawProjectionShape(DayLightCollider2D id)
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

            Sprite sprite = virtualSpriteRenderer.sprite;

            if (sprite == null)
            {
                return;
            }

            Texture2D texture = sprite.texture;

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

            List<Polygon2> polygons = shape.GetPolygonsWorld();

            if (polygons.Count < 1)
            {
                return;
            }

            Polygon2 polygon = polygons[0];
        
            SpriteTransform spriteTransform = new SpriteTransform(virtualSpriteRenderer, Vector2.zero, scale, id.transform.rotation.eulerAngles.z);

            Rect uv = spriteTransform.uv;

            float pivotY = (float)sprite.pivot.y / sprite.texture.height;
            pivotY = uv.y + pivotY;

            float pivotX = (float)sprite.pivot.x / sprite.texture.width;
            
            pair = Polygon2Helper.GetAxis(polygon, direction);
            pair.A += cameraOffset;
            pair.B += cameraOffset;

            if (Lighting2D.DayLightingSettings.direction > 180)
            {
                float uvx = uv.x;
                uv.x = uv.width;
                uv.width = uvx;
            }

            if (virtualSpriteRenderer.flipX)
            {
                float uvx = uv.x;
                uv.x = uv.width;
                uv.width = uvx;
            }

            Vector2 v1 = pair.A;
            Vector2 v2 = pair.A;
            Vector2 v3 = pair.B;
            Vector2 v4 = pair.B;

            v2 = v2.Push(direction, id.shadowDistance * shadowDistance);
            v3 = v3.Push(direction, id.shadowDistance * shadowDistance);

            GL.Color(new Color(0, 0, 0, 1 - id.shadowTranslucency));

            GL.TexCoord3 (uv.x, pivotY, 0);
            GL.Vertex3 (v1.x, v1.y, 0);

            GL.TexCoord3 (uv.x, uv.height, 0);
            GL.Vertex3 (v2.x, v2.y, 0);

            GL.TexCoord3 (uv.width, uv.height, 0);
            GL.Vertex3 (v3.x, v3.y, 0);

            GL.TexCoord3 (uv.width, pivotY, 0);
            GL.Vertex3 (v4.x, v4.y, 0);
        }
    }
}