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
        [SerializeField] private Light2D highlight;
        [SerializeField] private LightSource lightSource;

        private Token token;
        private bool loaded;
        private bool updateRequired;
        private float angleToAdd;
        private float angleTimer;
        private float originalRotation;

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
                SetRotation(token.Data.lightRotation);
                updateRequired = false;
            }

            if (!token.Selected || token.UI.Editing) return;
            HandleRotation();
        }

        private void HandleRotation()
        {
            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E)) return;

            // Define rotation direction A = left, D = right
            if (Input.GetKey(KeyCode.Q)) angleToAdd += 100.0f * Time.deltaTime;
            if (Input.GetKey(KeyCode.E)) angleToAdd -= 100.0f * Time.deltaTime;

            angleTimer += Time.deltaTime;

            if (angleTimer >= 1.0f / 30.0f)
            {
                angleTimer = 0.0f;
                FinishRotation(angleToAdd);
                angleToAdd = 0.0f;
            }
        }

        private void PreviewRotation(float value)
        {
            lightSource.transform.eulerAngles = new Vector3(0.0f, 0.0f, value);
        }
        public void FinishRotation(float angle, bool addAngle = true)
        {
            float targetAngle = addAngle ? token.Data.lightRotation + angle : angle;
            token.Data.lightRotation = targetAngle;
            PreviewRotation(targetAngle);
            SocketManager.EmitAsync("rotate-token-light", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                token.Data.lightRotation = originalRotation;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Data.id, targetAngle, GameData.User.id);
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
            highlight.enabled = enabled;
        }
        public void SetRotation(float value)
        {
            lightSource.transform.eulerAngles = new Vector3(0.0f, 0.0f, value);
            token.Data.lightRotation = value;
            originalRotation = value;
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
            float feetToUnits = Session.Instance.Grid.CellSize / Session.Instance.Grid.Unit.scale;
            nightSource.size = token.Data.nightRadius * feetToUnits;
            visionSource.size = token.Data.visionRadius * feetToUnits;
            highlight.size = Session.Instance.Grid.CellSize * 0.5f;
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