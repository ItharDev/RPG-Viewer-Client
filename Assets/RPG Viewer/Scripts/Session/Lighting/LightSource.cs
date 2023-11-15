using System.Collections;
using FunkyCode;
using FunkyCode.Utilities;
using UnityEngine;

namespace RPG
{
    [RequireComponent(typeof(Light2D))]
    public class LightSource : MonoBehaviour
    {
        private PresetData data;
        private Light2D source;
        private TimerHelper timer;
        private float lightAlpha;

        private void Awake()
        {
            source = GetComponent<Light2D>();
        }

        public void LoadData(PresetData _data)
        {
            data = _data;
            source.size = data.radius * Session.Instance.Grid.Unit.scale * Session.Instance.Grid.CellSize;
            source.color = data.color;
            lightAlpha = data.color.a;

            source.spotAngleInner = Mathf.Clamp(data.angle - 10.0f, 0.0f, 360.0f);
            source.spotAngleOuter = Mathf.Clamp(data.angle, 0.0f, 360.0f);

            StopAllCoroutines();
            timer = TimerHelper.Create();
            if (data.effect.frequency > 0 && data.effect.type == 2) StartCoroutine(PulseCoroutine());
        }
        public void Toggle(bool enabled)
        {
            source.enabled = enabled;
        }

        private void Update()
        {
            if (timer == null)
            {
                timer = TimerHelper.Create();
                return;
            }

            if (data.effect.type == 1 && data.effect.frequency > 0)
            {
                if (timer.GetMillisecs() > 1000f / data.effect.frequency)
                {
                    float tempAlpha = lightAlpha;
                    tempAlpha += Random.Range(-data.effect.strength * 0.01f, data.effect.strength * 0.01f);
                    source.color.a = tempAlpha;
                    timer.Reset();
                }
            }
        }

        private IEnumerator PulseCoroutine()
        {
            float targetAlpha = source.color.a - data.effect.strength * 0.01f;
            float originalAlpha = source.color.a;
            float targetTime = data.effect.frequency * 0.5f;

            for (float t = 0f; t < targetTime; t += Time.deltaTime)
            {
                float normalizedTime = t / targetTime;
                source.color.a = Mathf.Lerp(originalAlpha, targetAlpha, normalizedTime);
                yield return null;
            }
            source.color.a = targetAlpha;

            for (float t = 0f; t < targetTime; t += Time.deltaTime)
            {
                float normalizedTime = t / targetTime;
                source.color.a = Mathf.Lerp(targetAlpha, originalAlpha, normalizedTime);
                yield return null;
            }

            source.color.a = originalAlpha;
            StartCoroutine(PulseCoroutine());
        }

        public void Rotate(float angle)
        {

        }
    }
}