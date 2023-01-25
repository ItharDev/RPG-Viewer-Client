using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightSettings;

namespace FunkyCode.Rendering.Light
{
    public class Pass
    {
        public Light2D light;
        public LayerSetting layer;
        public int layerID;

        public float lightSizeSquared;

        public List<LightCollider2D> colliderList;
        public List<LightCollider2D> layerShadowList;
        public List<LightCollider2D> layerMaskList;

        public List<LightTilemapCollider2D> tilemapShadowList;
        public List<LightTilemapCollider2D> tilemapMaskList;
        public List<LightTilemapCollider2D> tilemapList;

        public bool drawMask = false;
        public bool drawShadows = false;

        public Material materialMask;
        public Material materialNormalMap_PixelToLight;
        public Material materialNormalMap_ObjectToLight;

        public Sorting.SortPass sortPass = new Sorting.SortPass();

        public bool Setup(Light2D light, LayerSetting setLayer)
        {
            // Layer ID
            layerID = setLayer.GetLayerID();

            if (layerID < 0)
            {
                return(false);
            }

            layer = setLayer;

            // Calculation Setup
            this.light = light;
            lightSizeSquared = Mathf.Sqrt(light.size * light.size + light.size * light.size);
        
            colliderList = LightCollider2D.List;

            layerShadowList = LightCollider2D.GetShadowList(layerID);
            layerMaskList = LightCollider2D.GetMaskList(layerID);

            tilemapList = LightTilemapCollider2D.List;

            tilemapShadowList = LightTilemapCollider2D.GetShadowList(layerID);
            tilemapMaskList = LightTilemapCollider2D.GetMaskList(layerID);
    
            // Draw Mask & Shadows?
            drawMask = (layer.type != LightLayerType.ShadowOnly);
            drawShadows = (layer.type != LightLayerType.MaskOnly);

            // Materials

            if (light.translucentLayer > 0)
            {
                materialMask = Lighting2D.Materials.mask.GetMaskTranslucency();

                if (light.Buffer.translucencyTexture != null)
                {
                    materialMask?.SetTexture("_SecTex", light.Buffer.translucencyTexture.renderTexture);

                    materialMask?.SetFloat("_TextureSize", light.GetTextureSize().x);
                }
            }
                else
            {
                materialMask = Lighting2D.Materials.mask.GetMask();
            }
           
            materialNormalMap_PixelToLight = Lighting2D.Materials.bumpMask.GetNormalMapSpritePixelToLight();

            if (materialNormalMap_PixelToLight != null)
            {
                if (light.translucentLayer > 0 && light.Buffer.translucencyTexture != null)
                {
                    materialNormalMap_PixelToLight.SetTexture("_SecTex", light.Buffer.translucencyTexture.renderTexture);
                    materialNormalMap_PixelToLight.SetFloat("_TextureSize", light.GetTextureSize().x);
                }
                    else
                {
                    materialNormalMap_PixelToLight.SetTexture("_SecTex", null);
                }
            }

            materialNormalMap_ObjectToLight = Lighting2D.Materials.bumpMask.GetNormalMapSpriteObjectToLight();

            if (materialNormalMap_ObjectToLight != null)
            {
                if (light.translucentLayer > 0 && light.Buffer.translucencyTexture != null)
                {
                    materialNormalMap_ObjectToLight.SetTexture("_SecTex", light.Buffer.translucencyTexture.renderTexture);
                    materialNormalMap_ObjectToLight.SetFloat("_TextureSize", light.GetTextureSize().x);
                }
                    else
                {
                    materialNormalMap_ObjectToLight.SetTexture("_SecTex", null); 
                }
            }

            materialNormalMap_PixelToLight.SetFloat("_LightSize", light.size);
            materialNormalMap_PixelToLight.SetFloat("_LightIntensity", light.bumpMap.intensity);
            materialNormalMap_PixelToLight.SetFloat("_LightZ", light.bumpMap.depth);

            materialNormalMap_ObjectToLight.SetFloat("_LightSize", light.size);
            materialNormalMap_ObjectToLight.SetFloat("_LightIntensity", light.bumpMap.intensity);
            materialNormalMap_ObjectToLight.SetFloat("_LightZ", light.bumpMap.depth);

            sortPass.pass = this;
            
            // sort
            sortPass.Clear();

            return(true);
        }
    }
}