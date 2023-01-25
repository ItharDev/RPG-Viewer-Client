using UnityEngine;
using FunkyCode;
using FunkyCode.Utilities;
using System.Collections;

namespace RPG
{
    public class LightHandler : MonoBehaviour
    {
        public Light2D LightSource;
        private float lightAlpha;
        private float flickerFrequency = 15f;
        private float flickerRangeMin = -0.1f;
        private float flickerRangeMax = 0.1f;

        private float pulseInterval;
        private float pulseAmount;

        private LightEffect lightEffect;
        private TimerHelper timer;

        public void Init(LightEffect _effect, Color _color, float _intensity, float _flickerFrequency, float _flickerAmount, float _pulseInterval, float _pulseAmount)
        {
            StopAllCoroutines();
            lightEffect = _effect;
            LightSource = GetComponent<Light2D>();
            timer = TimerHelper.Create();
            LightSource.color = _color;
            LightSource.color.a = _intensity;
            lightAlpha = LightSource.color.a;

            flickerFrequency = _flickerFrequency;
            flickerRangeMin = -_flickerAmount;
            flickerRangeMax = _flickerAmount;
            pulseInterval = _pulseInterval;
            pulseAmount = _pulseAmount;

            switch (_effect)
            {
                case LightEffect.No_source:
                    LightSource.color.a = 0;
                    break;
                case LightEffect.Torch:
                    break;
                case LightEffect.Wakka_nut:
                    if (pulseInterval > 0) StartCoroutine(PulseCoroutine());
                    break;
                case LightEffect.Oil_lamp:
                    break;
                case LightEffect.Candle:
                    break;
                case LightEffect.Pulsing:
                    if (pulseInterval > 0) StartCoroutine(PulseCoroutine());
                    break;
                case LightEffect.Flickering:
                    break;
                case LightEffect.No_effect:
                    break;
            }
        }

        private void Update()
        {
            if (timer == null)
            {
                timer = TimerHelper.Create();
                return;
            }

            if ((lightEffect == LightEffect.Torch || lightEffect == LightEffect.Oil_lamp || lightEffect == LightEffect.Flickering) && flickerFrequency > 0)
            {
                if (timer.GetMillisecs() > 1000f / flickerFrequency)
                {
                    float tempAlpha = lightAlpha;
                    tempAlpha += Random.Range(flickerRangeMin, flickerRangeMax);
                    LightSource.color.a = tempAlpha;
                    timer.Reset();
                }
            }            
        }

        private IEnumerator PulseCoroutine()
        {
            float targetAlpha = LightSource.color.a - pulseAmount;
            float originalAlpha = LightSource.color.a;
            float targetTime = pulseInterval * 0.5f;

            for (float t = 0f; t < targetTime; t += Time.fixedDeltaTime)
            {
                float normalizedTime = t / targetTime;
                LightSource.color.a = Mathf.Lerp(originalAlpha, targetAlpha, normalizedTime);
                yield return null;
            }
            LightSource.color.a = targetAlpha;

            for (float t = 0f; t < targetTime; t += Time.fixedDeltaTime)
            {
                float normalizedTime = t / targetTime;
                LightSource.color.a = Mathf.Lerp(targetAlpha, originalAlpha, normalizedTime);
                yield return null;
            }

            StartCoroutine(PulseCoroutine());
        }
    }

    [System.Serializable]
    public enum LightEffect
    {
        No_source,
        Torch,
        Wakka_nut,
        Oil_lamp,
        Candle,
        Pulsing,
        Flickering,
        No_effect,
    }
}