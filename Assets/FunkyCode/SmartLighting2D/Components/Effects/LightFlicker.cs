using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode
{
    public class LightFlicker : MonoBehaviour
    {
        public float flickersPerSecond = 15f;
        public float flickerRangeMin = -0.1f;
        public float flickerRangeMax = 0.1f;

        Light2D lightSource;
        float lightAlpha;
        TimerHelper timer;

        private void Start()
        {
            lightSource = GetComponent<Light2D>();
            lightAlpha = lightSource.color.a;

            timer = TimerHelper.Create();
        }

        private void Update()
        {
            if (timer == null)
            {
                timer = TimerHelper.Create();
                return;
            }

            if (timer.GetMillisecs() > 1000f / flickersPerSecond)
            {
                float tempAlpha = lightAlpha;
                tempAlpha = tempAlpha + Random.Range(flickerRangeMin, flickerRangeMax);
                lightSource.color.a = tempAlpha;
                timer.Reset();
            }
        }
    }
}