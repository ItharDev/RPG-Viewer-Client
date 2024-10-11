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
        private float primaryAlpha;
        private float secondaryAlpha;

        public void LoadData(PresetData _data)
        {
            data = _data;

            // Primary
            primary.size = data.primary.radius / Session.Instance.Grid.Unit.scale * Session.Instance.Grid.CellSize + Session.Instance.Grid.CellSize * 0.5f;
            primary.color = data.primary.color;
            primaryAlpha = data.primary.color.a;

            primary.spotAngleInner = Mathf.Clamp(data.primary.angle, 0.0f, 360.0f);
            primary.spotAngleOuter = Mathf.Clamp(data.primary.angle + 10.0f, 0.0f, 360.0f);

            // Secondary
            secondary.size = data.secondary.radius / Session.Instance.Grid.Unit.scale * Session.Instance.Grid.CellSize + Session.Instance.Grid.CellSize * 0.5f;
            secondary.color = data.secondary.color;
            secondaryAlpha = data.secondary.color.a;

            secondary.spotAngleInner = Mathf.Clamp(data.secondary.angle, 0.0f, 360.0f);
            secondary.spotAngleOuter = Mathf.Clamp(data.secondary.angle + 10.0f, 0.0f, 360.0f);

            primaryTimer = TimerHelper.Create();
            secondaryTimer = TimerHelper.Create();

            StopAllCoroutines();
            if (data.primary.effect.frequency > 0 && data.primary.effect.type == 2) StartCoroutine(PrimaryPulse());
            if (data.secondary.effect.frequency > 0 && data.secondary.effect.type == 2) StartCoroutine(SecondaryPulse());
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
                    float tempAlpha = primaryAlpha;
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
                    float tempAlpha = secondaryAlpha;
                    tempAlpha += Random.Range(-data.secondary.effect.strength * 0.01f, data.secondary.effect.strength * 0.01f);
                    secondary.color.a = tempAlpha;
                    secondaryTimer.Reset();
                }
            }
        }

        private IEnumerator PrimaryPulse()
        {
            float originalAlpha = primary.color.a - data.primary.effect.strength * 0.01f;
            float originalColor = primary.color.a;
            float time = data.primary.effect.frequency * 0.5f;

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float normalizedTime = t / time;
                primary.color.a = Mathf.Lerp(originalColor, originalAlpha, normalizedTime);
                yield return null;
            }
            primary.color.a = originalAlpha;

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float normalizedTime = t / time;
                primary.color.a = Mathf.Lerp(originalAlpha, originalColor, normalizedTime);
                yield return null;
            }

            primary.color.a = originalColor;

            StartCoroutine(PrimaryPulse());
        }
        private IEnumerator SecondaryPulse()
        {
            float originalAlpha = secondary.color.a - data.secondary.effect.strength * 0.01f;
            float originalColor = secondary.color.a;
            float time = data.secondary.effect.frequency * 0.5f;

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float normalizedTime = t / time;
                secondary.color.a = Mathf.Lerp(originalColor, originalAlpha, normalizedTime);
                yield return null;
            }
            secondary.color.a = originalAlpha;

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float normalizedTime = t / time;
                secondary.color.a = Mathf.Lerp(originalAlpha, originalColor, normalizedTime);
                yield return null;
            }

            secondary.color.a = originalColor;
            StartCoroutine(SecondaryPulse());
        }
    }
}