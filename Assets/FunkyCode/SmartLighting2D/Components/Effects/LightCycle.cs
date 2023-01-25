using UnityEngine;

namespace FunkyCode
{
    [System.Serializable]
    public class LightCycleBuffer
    {
        public Gradient gradient = new Gradient();
    }

    [System.Serializable]
    public class LightDayProperties
    {
        [Range(0, 360)]
        public float shadowOffset = 0;

        public AnimationCurve shadowHeight = new AnimationCurve();

        public AnimationCurve shadowAlpha = new AnimationCurve();  
    }

    [ExecuteInEditMode]
    public class LightCycle : MonoBehaviour
    {
        [Range(0, 1)]
        public float time = 0;

        public LightDayProperties dayProperties = new LightDayProperties();

        public LightCycleBuffer[] nightProperties = new LightCycleBuffer[1]; // lightmap

        public void SetTime(float setTime)
        {
            time = setTime;
        }

        void LateUpdate()
        {
            LightingSettings.LightmapPresetList lightmapPresets = Lighting2D.Profile.lightmapPresets;

            if (lightmapPresets == null)
            {
                return;
            }
        
            /*
            if (Input.GetMouseButton(0)&& Input.touchCount > 1) { // 
                time += Time.deltaTime * 0.05f;

                time = time % 1;
            }*/
            
            float time360 = (time * 360);

            // Day Lighting Properties
            float height = dayProperties.shadowHeight.Evaluate(time);
            float alpha = dayProperties.shadowAlpha.Evaluate(time);

            if (height < 0.01f)
            {
                height = 0.01f;
            }

            if (alpha < 0)
            {
                alpha = 0;
            }

            Lighting2D.DayLightingSettings.height = height;
            Lighting2D.DayLightingSettings.ShadowColor.a = alpha;
            Lighting2D.DayLightingSettings.direction = time360 + dayProperties.shadowOffset;

            // Dynamic Properties
            for(int i = 0; i < nightProperties.Length; i++) {
                if (i >= lightmapPresets.list.Length) {
                    return;
                }

                LightCycleBuffer buffer = nightProperties[i];

                if (buffer == null) {
                    continue;
                }

                Color color = buffer.gradient.Evaluate(time);

                LightingSettings.LightmapPreset lightmapPreset = lightmapPresets.list[i];
                lightmapPreset.darknessColor = color;
            }
        }
    }
}