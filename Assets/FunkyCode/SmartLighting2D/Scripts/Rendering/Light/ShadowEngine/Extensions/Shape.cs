using System.Collections.Generic;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light.Shadow
{
    public class Shape
    {
        public static void Draw(Light2D light, LightCollider2D lightCollider)
        {
            if (!lightCollider.InLight(light))
            {
                return;
            }

            if (light.eventPresetId > 0)
            {
                // optimize - only if event handling enabled
                // used to update light when light collider leaves light bounds
                light.AddCollider(lightCollider); 
            }

            float shadowMin = lightCollider.shadowDistanceMin;
            float shadowMax = lightCollider.shadowDistanceMax;

            if (lightCollider.shadowDistance == LightCollider2D.ShadowDistance.Infinite)
            {
                shadowMin = 0;
                shadowMax = 0;
            }

            LightColliderShape shape = lightCollider.mainShape;

            List<Polygon2> polygons = shape.GetPolygonsWorld();
            
            ShadowEngine.Draw(polygons, shadowMin, shadowMax, lightCollider.shadowTranslucency);
        }
    }
}