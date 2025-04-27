using System.Collections;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class TokenEffect : MonoBehaviour
    {
        [SerializeField] private Canvas effectCanvas;
        [SerializeField] private Image effectImage;

        private Token token;
        private bool loaded;
        private bool updateRequired;
        private EffectData data;

        private void OnEnable()
        {
            // Get reference of main class
            if (token == null) token = GetComponent<Token>();

            // Add event listeners
            Events.OnEffectModified.AddListener(ModifyEffect);
            Events.OnEffectRemoved.AddListener(RemoveEffect);
        }
        private void OnDisable()
        {
            // remove event listeners
            Events.OnEffectModified.RemoveListener(ModifyEffect);
            Events.OnEffectRemoved.RemoveListener(RemoveEffect);
        }

        private void Update()
        {
            if (!loaded && Session.Instance.Grid.Grid != null) loaded = true;
            if (updateRequired && loaded)
            {
                LoadData();
                updateRequired = false;
            }
        }

        private void ModifyEffect(string _id, EffectData _data)
        {
            // Return if the effect doesn't affect us
            if (token.Data.effect != _id) return;

            token.Data.effect = _id;
            data = _data;
            Reload();
        }

        private void RemoveEffect(string _id, EffectData _data)
        {
            // Return if the effect doesn't affect us
            if (token.Data.effect != _id) return;

            token.Data.effect = string.Empty;
            data = new EffectData();
            Reload();
        }

        public void Reload()
        {
            updateRequired = true;
        }

        private void LoadData()
        {
            // Remove effect if no effect is set
            if (string.IsNullOrEmpty(token.Data.effect))
            {
                effectCanvas.enabled = false;
                return;
            }

            if (data.id != token.Data.effect) data = EffectManager.Instance.GetEffect(token.Data.effect);

            // Set effect image
            WebManager.Download(data.image, true, async (bytes) =>
            {
                if (bytes == null) return;

                await UniTask.SwitchToMainThread();

                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                effectImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                effectCanvas.enabled = true;
                effectCanvas.sortingLayerName = data.overTokens ? "Effects (Top)" : "Effects (Bottom)";

                float cellSize = Session.Instance.Grid.CellSize;
                ((RectTransform)effectCanvas.transform).sizeDelta = new Vector2(200 * cellSize * ((data.radius + Session.Instance.Grid.Unit.scale * 0.5f) / Session.Instance.Grid.Unit.scale), 200 * cellSize * ((data.radius + Session.Instance.Grid.Unit.scale * 0.5f) / Session.Instance.Grid.Unit.scale));

                effectImage.color = data.color;
                StartAnimation();
            });
        }

        private void StartAnimation()
        {
            StopAllCoroutines();

            if (data.animation.type == EffectAnimationType.Pulse || data.animation.type == EffectAnimationType.Both) StartCoroutine(PulsingEffect());
            if (data.animation.type == EffectAnimationType.Rotate || data.animation.type == EffectAnimationType.Both) StartCoroutine(RotateEffect());
        }

        private IEnumerator PulsingEffect()
        {
            float targetAlpha = data.color.a - (data.color.a * data.animation.pulseStrength * 0.01f);
            Color originalColor = data.color;
            float time = 1.0f / (2.0f * data.animation.pulseFrequency);

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float normalizedTime = t / time;
                Color color = effectImage.color;
                color.a = Mathf.Lerp(originalColor.a, targetAlpha, normalizedTime);
                effectImage.color = color;
                yield return null;
            }

            Color targetColor = effectImage.color;
            targetColor.a = targetAlpha;
            effectImage.color = targetColor;

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float normalizedTime = t / time;
                Color color = effectImage.color;
                color.a = Mathf.Lerp(targetAlpha, originalColor.a, normalizedTime);
                effectImage.color = color;
                yield return null;
            }

            effectImage.color = originalColor;
            StartCoroutine(PulsingEffect());
        }

        private IEnumerator RotateEffect()
        {
            float time = 1.0f / data.animation.rotationSpeed;
            float originalAngle = 0f;
            float targetAngle = 360f;
            float angle;

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float normalizedTime = t / time;
                angle = Mathf.Lerp(originalAngle, targetAngle, normalizedTime);
                effectCanvas.transform.rotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }

            effectCanvas.transform.rotation = Quaternion.Euler(0, 0, targetAngle);
            StartCoroutine(RotateEffect());
        }
    }
}