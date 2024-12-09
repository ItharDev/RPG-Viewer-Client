using System.Linq;
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
            Events.OnLightingChanged.AddListener(HandleEvents);
        }
        private void OnDisable()
        {
            // remove event listeners
            Events.OnPresetModified.RemoveListener(ModifyPreset);
            Events.OnLightingChanged.RemoveListener(HandleEvents);
        }

        private void HandleEvents(LightingSettings data, bool globalUpdate)
        {
            if (globalUpdate)
            {
                float feetToUnits = Session.Instance.Grid.CellSize / Session.Instance.Grid.Unit.scale;
                visionSource.size = (data.visionRange == 0.0f ? token.Data.visionRadius : data.visionRange) * feetToUnits;
            }
        }

        private void Update()
        {
            if (!loaded && Session.Instance.Grid.Grid != null) loaded = true;
            if (updateRequired && loaded)
            {
                LoadVision();
                LoadLighting();
                ApplyVisibility();
                HandleEvents(Session.Instance.Settings.darkness, true);
                SetRotation(token.Data.lightRotation);
                updateRequired = false;
            }

            if (!token.Selected || token.UI.Editing) return;

            HandleRotation();
            HandleLight();
        }

        private void HandleLight()
        {
            if (Input.GetKeyDown(KeyCode.Space)) ToggleLight();
        }

        private void ToggleLight()
        {
            SocketManager.EmitAsync("toggle-token-light", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, token.Data.id, !token.Data.lightEnabled);
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
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
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
            Permission isPlayer = token.Data.permissions.FirstOrDefault(value => value.type == PermissionType.Controller);
            EnableHighlight(!string.IsNullOrEmpty(isPlayer.user) && token.Enabled && token.Visibility.visible);
            ToggleVision(token.Enabled && token.Visibility.visible && (token.Data.enabled || ConnectionManager.Info.isMaster) && token.Permission.type != PermissionType.None);
            ToggleLight(token.Visibility.visible && token.Data.enabled && token.Data.lightEnabled);
        }

        public void ToggleVision(bool enabled)
        {
            nightSource.enabled = enabled;
            visionSource.enabled = enabled;
        }
        public void EnableHighlight(bool enabled)
        {
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
            visionSource.size = (Session.Instance.Settings.darkness.visionRange == 0.0f ? token.Data.visionRadius : Session.Instance.Settings.darkness.visionRange) * feetToUnits;
            highlight.size = Session.Instance.Grid.CellSize * (token.Data.dimensions.x / Session.Instance.Grid.Unit.scale) * 0.5f;
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
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, token.Data.light);
        }
    }
}