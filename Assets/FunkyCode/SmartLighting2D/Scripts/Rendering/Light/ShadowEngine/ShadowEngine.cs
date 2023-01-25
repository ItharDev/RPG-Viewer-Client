using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightSettings;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light
{
    public struct UVRect
    {
        public float x0;
        public float y0;
        public float x1;
        public float y1;

        public UVRect(float x0, float y0, float x1, float y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }
    }

    public static class ShadowEngine
    {
        public static Light2D light;

        public static Vector2 lightOffset = Vector2.zero;
        public static Vector2 drawOffset = Vector2.zero;

        public static float lightSize = 0;

        public static bool continueDrawing = false;
        public static bool ignoreInside = false;
        public static bool dontdrawInside = false;

        public static Vector2 objectOffset = Vector2.zero;
        public static bool flipX = false;
        public static bool flipY = false;

        public static Sprite spriteProjection = null;

        public static bool perpendicularIntersection;
        public static int effectLayer = 0;

        // Layer Effect
        public static List<List<Polygon2>> effectPolygons = new List<List<Polygon2>>();

        // public static float shadowDistance;
        
        public static bool softShadow = false;

        public static int drawMode = 0;

        public const int DRAW_MODE_LEGACY_CPU = 0;
        public const int DRAW_MODE_LEGACY_GPU = 4;
        public const int DRAW_MODE_SOFT_CONVEX = 1;
        public const int DRAW_MODE_SOFT_VERTEX = 5;
        public const int DRAW_MODE_SOFT_DISTANCE = 6;
        public const int DRAW_MODE_SOFT_DEFAULT = 7;
        public const int DRAW_MODE_PERPENDICULAR = 2;
        public const int DRAW_MODE_SPRITEPROJECTION = 3;
        public const int DRAW_MODE_FAST = 8;

        public static Material GetMaterial()
        {
            Material material = null;

            switch(drawMode)
            {
                case DRAW_MODE_LEGACY_CPU:

                    material = Lighting2D.Materials.shadow.GetLegacyCPUShadow();
                    material.mainTexture = Lighting2D.Materials.shadow.GetPenumbraSprite().texture;
                    
                break;

                case DRAW_MODE_LEGACY_GPU:

                    material = Lighting2D.Materials.shadow.GetLegacyGPUShadow();

                break;

                case DRAW_MODE_SOFT_CONVEX:
                case DRAW_MODE_SOFT_VERTEX:

                    material = Lighting2D.Materials.shadow.GetSoftShadow();
                    material.SetFloat("_CoreSize", light.coreSize);
                    material.SetFloat("_FallOff", light.falloff);
                    
                break;

                case DRAW_MODE_SOFT_DEFAULT:

                    material = Lighting2D.Materials.shadow.GetSoftShadowDefault();
                    material.SetFloat("_LightVolume", light.lightRadius);

                break;

                case DRAW_MODE_SOFT_DISTANCE:

                    material = Lighting2D.Materials.shadow.GetSoftDistanceShadow();

                break;

                case DRAW_MODE_PERPENDICULAR:
                case DRAW_MODE_SPRITEPROJECTION:

                    material = Lighting2D.Materials.shadow.GetAlphaShadow();

                break;

                case DRAW_MODE_FAST:

                    material = Lighting2D.Materials.shadow.GetFastShadow();

                break;
            }

            return(material);
        }
        
        public static void Draw(List<Polygon2> polygons, float shadowDistanceMin, float shadowDistanceMax, float shadowTranslucency)
        {
            if (!continueDrawing)
            {
                return;
            }

            switch(ShadowEngine.drawMode)
            {
                case DRAW_MODE_LEGACY_CPU:

                    Shadow.LegacyCPU.Draw(polygons, shadowDistanceMin, shadowDistanceMax, shadowTranslucency);

                break;

                case DRAW_MODE_LEGACY_GPU:

                    Shadow.LegacyGPU.Draw(polygons, shadowDistanceMin, shadowTranslucency);

                break;

                case DRAW_MODE_SOFT_DEFAULT:

                    Shadow.SoftDefault.Draw(polygons, shadowTranslucency);

                break;
            
                case DRAW_MODE_SOFT_CONVEX:
                case DRAW_MODE_SOFT_VERTEX:

                    // does not support shadow distance
                    Shadow.Soft.Draw(polygons, shadowTranslucency); 

                break;

                case DRAW_MODE_SOFT_DISTANCE:

                    Shadow.SoftDistance.Draw(polygons, shadowDistanceMin, shadowDistanceMax, shadowTranslucency);

                break;

                case DRAW_MODE_PERPENDICULAR:

                    // does not support translucency + shadow Distance after intersection)
                    Shadow.PerpendicularIntersection.Draw(polygons, shadowDistanceMin);

                break;

                case DRAW_MODE_SPRITEPROJECTION:

                    Shadow.SpriteProjection.Draw(polygons, shadowDistanceMin, shadowDistanceMax, shadowTranslucency);

                break;

                case DRAW_MODE_FAST:

                    Shadow.Fast.Draw(polygons, shadowTranslucency);
                    
                break;
            }
        }

        public static void SetPass(Light2D lightObject, LayerSetting layer)
        {
            light = lightObject;
            lightSize = Mathf.Sqrt(light.size * light.size + light.size * light.size);
            lightOffset = -light.transform2D.position;

            effectLayer = layer.shadowEffectLayer;

            objectOffset = Vector2.zero;

            effectPolygons.Clear();

            softShadow = layer.shadowEffect == LightLayerShadowEffect.SoftConvex || layer.shadowEffect == LightLayerShadowEffect.SoftVertex;

            if (lightObject.IsPixelPerfect())
            {
                Camera camera = Camera.main;

                Vector2 pos = LightingPosition.GetPosition2D(-camera.transform.position);

                drawOffset = light.transform2D.position + pos;
            }
                else
            {
                drawOffset = Vector2.zero;
            }

            switch(layer.shadowEffect)
            {
                case LightLayerShadowEffect.PerpendicularProjection:
                    drawMode = DRAW_MODE_PERPENDICULAR;
                    GenerateEffectLayers();
                break;

                case LightLayerShadowEffect.SoftConvex:
                    drawMode = DRAW_MODE_SOFT_CONVEX;
                break;

                case LightLayerShadowEffect.SoftVertex:
                    drawMode = DRAW_MODE_SOFT_VERTEX;
                break;

                case LightLayerShadowEffect.Default:
                    drawMode = DRAW_MODE_SOFT_DISTANCE;
                break;

                case LightLayerShadowEffect.SpriteProjection:
                    drawMode = DRAW_MODE_SPRITEPROJECTION;
                break;

                case LightLayerShadowEffect.LegacyCPU:
                    drawMode = DRAW_MODE_LEGACY_CPU;
                break;

                case LightLayerShadowEffect.LegacyGPU:
                    drawMode = DRAW_MODE_LEGACY_GPU;
                break;

                case LightLayerShadowEffect.Soft:
                    drawMode = DRAW_MODE_SOFT_DEFAULT;
                break;

                case LightLayerShadowEffect.Fast:
                    drawMode = DRAW_MODE_FAST;
                break;
            }
        }

        public static void GenerateEffectLayers()
        {
            int layerID = (int)ShadowEngine.effectLayer;

            foreach(LightCollider2D c in LightCollider2D.GetShadowList((layerID)))
            {
                List<Polygon2> polygons = c.mainShape.GetPolygonsWorld();

                if (polygons == null)
                {
                    continue;
                }
    
                if (c.InLight(light))
                {
                    effectPolygons.Add(polygons);
                }
            }
        }
        
        public static void Prepare(Light2D light)
        {
            continueDrawing = true;
            ignoreInside = light.whenInsideCollider == Light2D.WhenInsideCollider.Ignore;
            dontdrawInside = light.whenInsideCollider == Light2D.WhenInsideCollider.DontDraw;
        }
    }
}