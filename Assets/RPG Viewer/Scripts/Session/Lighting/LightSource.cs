using System.Collections;
using FunkyCode;
using FunkyCode.Utilities;
using UnityEngine;

namespace RPG
{
    public class LightSource : MonoBehaviour
    {
        private PresetData data;
        [SerializeField] private Light2D primary;
        [SerializeField] private Light2D secondary;
        private TimerHelper primaryTimer;
        private TimerHelper secondaryTimer;
        private float lightAlpha;

        public void LoadData(PresetData _data)
        {
            data = _data;

            // Primary
            primary.size = data.primary.radius / Session.Instance.Grid.Unit.scale * Session.Instance.Grid.CellSize + Session.Instance.Grid.CellSize * 0.5f;
            primary.color = data.primary.color;
            lightAlpha = data.primary.color.a;

            primary.spotAngleInner = Mathf.Clamp(data.primary.angle, 0.0f, 360.0f);
            primary.spotAngleOuter = Mathf.Clamp(data.primary.angle + 10.0f, 0.0f, 360.0f);

            StopAllCoroutines();
            primaryTimer = TimerHelper.Create();
            if (data.primary.effect.frequency > 0 && data.primary.effect.type == 2) StartCoroutine(PulseCoroutine());

            // Secondary
            secondary.size = data.secondary.radius / Session.Instance.Grid.Unit.scale * Session.Instance.Grid.CellSize + Session.Instance.Grid.CellSize * 0.5f;
            secondary.color = data.secondary.color;
            lightAlpha = data.secondary.color.a;

            secondary.spotAngleInner = Mathf.Clamp(data.secondary.angle, 0.0f, 360.0f);
            secondary.spotAngleOuter = Mathf.Clamp(data.secondary.angle + 10.0f, 0.0f, 360.0f);

            StopAllCoroutines();
            secondaryTimer = TimerHelper.Create();
            if (data.secondary.effect.frequency > 0 && data.secondary.effect.type == 2) StartCoroutine(PulseCoroutine());
        }
        public void Toggle(bool enabled)
        {
            secondary.enabled = enabled;
            primary.enabled = enabled;
        }

        private void Update()
        {
            // Primary
            if (primaryTimer == null)
            {
                primaryTimer = TimerHelper.Create();
                return;
            }
            if (data.primary.effect.type == 1 && data.primary.effect.frequency > 0)
            {
                if (primaryTimer.GetMillisecs() > 1000f / data.primary.effect.frequency)
                {
                    float tempAlpha = lightAlpha;
                    tempAlpha += Random.Range(-data.primary.effect.strength * 0.01f, data.primary.effect.strength * 0.01f);
                    primary.color.a = tempAlpha;
                    primaryTimer.Reset();
                }
            }

            // Secondary
            if (secondaryTimer == null)
            {
                secondaryTimer = TimerHelper.Create();
                return;
            }
            if (data.secondary.effect.type == 1 && data.secondary.effect.frequency > 0)
            {
                if (secondaryTimer.GetMillisecs() > 1000f / data.secondary.effect.frequency)
                {
                    float tempAlpha = lightAlpha;
                    tempAlpha += Random.Range(-data.secondary.effect.strength * 0.01f, data.secondary.effect.strength * 0.01f);
                    secondary.color.a = tempAlpha;
                    secondaryTimer.Reset();
                }
            }
        }

        private IEnumerator PulseCoroutine()
        {
            // primary
            float primaryAlpha = primary.color.a - data.primary.effect.strength * 0.01f;
            float originalPrimary = primary.color.a;
            float primaryTime = data.primary.effect.frequency * 0.5f;

            for (float t = 0f; t < primaryTime; t += Time.deltaTime)
            {
                float normalizedTime = t / primaryTime;
                primary.color.a = Mathf.Lerp(originalPrimary, primaryAlpha, normalizedTime);
                yield return null;
            }
            primary.color.a = primaryAlpha;

            for (float t = 0f; t < primaryTime; t += Time.deltaTime)
            {
                float normalizedTime = t / primaryTime;
                primary.color.a = Mathf.Lerp(primaryAlpha, originalPrimary, normalizedTime);
                yield return null;
            }

            primary.color.a = originalPrimary;

            // Secondary
            float secondaryAlpha = secondary.color.a - data.secondary.effect.strength * 0.01f;
            float originalSecondary = secondary.color.a;
            float secondaryTime = data.secondary.effect.frequency * 0.5f;

            for (float t = 0f; t < secondaryTime; t += Time.deltaTime)
            {
                float normalizedTime = t / secondaryTime;
                secondary.color.a = Mathf.Lerp(originalSecondary, secondaryAlpha, normalizedTime);
                yield return null;
            }
            secondary.color.a = secondaryAlpha;

            for (float t = 0f; t < secondaryTime; t += Time.deltaTime)
            {
                float normalizedTime = t / secondaryTime;
                secondary.color.a = Mathf.Lerp(secondaryAlpha, originalSecondary, normalizedTime);
                yield return null;
            }

            secondary.color.a = originalSecondary;
            StartCoroutine(PulseCoroutine());
        }
    }
}