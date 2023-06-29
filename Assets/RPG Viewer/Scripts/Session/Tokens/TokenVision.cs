using Cysharp.Threading.Tasks;
using FunkyCode;
using Networking;
using UnityEngine;

namespace RPG
{
    public class TokenVision : MonoBehaviour
    {
        [SerializeField] private Light2D nightSource;
        [SerializeField] private Light2D visionSource;
        [SerializeField] private LightSource lightSource;

        private Token token;
        private bool loaded;
        private bool updateRequired;

        private void OnEnable()
        {
            // Get reference of main class
            if (token == null) token = GetComponent<Token>();

            // Add event listeners
            Events.OnPresetModified.AddListener(ModifyPreset);
        }
        private void OnDisable()
        {
            // remove event listeners
            Events.OnPresetModified.RemoveListener(ModifyPreset);
        }

        private void Update()
        {
            if (!loaded && Session.Instance.Grid.Grid != null) loaded = true;
            if (updateRequired && loaded)
            {
                LoadVision();
                LoadLighting();
                ApplyVisibility();
                updateRequired = false;
            }
        }

        private void ModifyPreset(string _id, PresetData _data)
        {
            // Return if the effect doesn't affect us
            if (token.Lighting.id != _id) return;

            token.Lighting = _data;
            LoadLighting();
        }
        private void ApplyVisibility()
        {
            ToggleVision(token.Visibility.visible && token.Enabled);
            ToggleLight(token.Visibility.visible);
        }

        public void ToggleVision(bool enabled)
        {
            nightSource.enabled = enabled;
            visionSource.enabled = enabled;
        }
        public void ToggleLight(bool enabled)
        {
            lightSource.Toggle(enabled);
        }

        public void Reload()
        {
            updateRequired = true;
        }
        private void LoadVision()
        {
            float feetToUnits = Session.Instance.Grid.CellSize * 0.2f;
            nightSource.size = token.Data.nightRadius * feetToUnits;
            visionSource.size = token.Data.visionRadius * feetToUnits;
        }
        private void LoadLighting()
        {
            SocketManager.EmitAsync("get-light", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    PresetData data = JsonUtility.FromJson<PresetData>(callback.GetValue(1).ToString());
                    token.Lighting = data;
                    token.Lighting.id = token.Data.light;
                    lightSource.LoadData(data);
                    lightSource.Toggle(token.Data.enabled);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Data.light);
        }
    }
}