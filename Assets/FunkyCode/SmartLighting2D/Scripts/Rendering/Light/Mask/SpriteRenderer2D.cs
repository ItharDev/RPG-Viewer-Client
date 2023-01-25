using UnityEngine;
using FunkyCode.LightSettings;

namespace FunkyCode.Rendering.Light
{
    public static class SpriteRenderer2D
    {
        static public Texture2D currentTexture = null;

        public static void Mask(Light2D light, LightCollider2D id, Material material, LayerSetting layerSetting)
        {
            if (!id.InLight(light))
                return;

            var shape = id.mainShape;
            var spriteRenderer = shape.spriteShape.GetSpriteRenderer();

            var sprite = shape.spriteShape.GetOriginalSprite();
            if (!sprite || !spriteRenderer)
                return;

            var texture = sprite.texture;
            if (texture == null)
                return;

            var localPosition = new Vector2()
            {
                x = shape.transform2D.Position.x - light.transform2D.position.x,
                y = shape.transform2D.Position.y - light.transform2D.position.y
            };

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

            GLExtended.color = LayerSettingColor.Get(shape, localPosition, layerSetting, id.maskLit, 1, id.maskLitCustom);
            
            Rendering.Universal.Sprite.Pass.Draw(id.spriteMeshObject, spriteRenderer, localPosition, shape.transform2D.Scale, shape.transform2D.Rotation);	
		}

        public static void MaskBumped(Light2D light, LightCollider2D id, Material material, LayerSetting layerSetting)
        {
            if (!id.InLight(light))
                return;

            var normalTexture = id.bumpMapMode.GetBumpTexture();
            if (!normalTexture)
                return;

            float rotation;

            material.SetTexture("_Bump", normalTexture);

            var shape = id.mainShape;
            var spriteRenderer = shape.spriteShape.GetSpriteRenderer();
            if (spriteRenderer == null)
            {
                return;
            }

            var sprite = shape.spriteShape.GetOriginalSprite();
            if (sprite == null)
                return;

            if (sprite.texture == null)
                return;
            
            Vector2 position = shape.transform2D.Position - light.transform2D.position;

            material.mainTexture = sprite.texture;
            GLExtended.color = LayerSettingColor.Get(position, layerSetting, id.maskLit, 1, id.maskLitCustom);
            
            float color = GLExtended.color.r;
            
            switch(id.bumpMapMode.type)
            {
                case NormalMapType.ObjectToLight:

                    rotation = Mathf.Atan2(light.transform2D.position.y - shape.transform2D.Position.y, light.transform2D.position.x - shape.transform2D.Position.x);
                    rotation -= Mathf.Deg2Rad * (shape.transform2D.Rotation);
                    
                    material.SetFloat("_LightRX", Mathf.Cos(rotation) * 2);
                    material.SetFloat("_LightRY", Mathf.Sin(rotation) * 2);
                    material.SetFloat("_LightColor",  color);

                break;

                case NormalMapType.PixelToLight:

                    material.SetFloat("_LightColor",  color);
                
                    rotation = shape.transform2D.Rotation * Mathf.Deg2Rad;

                    Vector2 sc = shape.transform2D.Scale.normalized;

                    material.SetFloat("_LightX", Mathf.Cos(rotation) * sc.x);
                    material.SetFloat("_LightY", Mathf.Cos(rotation) * sc.y);

                    material.SetFloat("_Depth", id.bumpMapMode.depth);

                    float invertX = id.bumpMapMode.invertX ? -1 : 1;
                    material.SetFloat("_InvertX", invertX);

                    float invertY = id.bumpMapMode.invertY ? -1 : 1;
                    material.SetFloat("_InvertY", invertY);
          
                break;
            }

            material.SetPass(0);
    
            Rendering.Universal.Sprite.Draw(id.spriteMeshObject, spriteRenderer, position, shape.transform2D.Scale, shape.transform2D.Rotation); 
        }
    }
}