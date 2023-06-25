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
        }
        private void Update()
        {
            if (!loaded && Session.Instance.Grid.Grid != null) loaded = true;
            if (updateRequired && loaded)
            {
                LoadVision();
                EnableVision(token.Enabled);
                LoadLighting();
                updateRequired = false;
            }
        }

        public void DisableVision()
        {
            nightSource.enabled = false;
            visionSource.enabled = false;
        }
        public void DisableLight()
        {
            lightSource.Toggle(false);
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
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Data.light);
        }

        public void EnableVision(bool selected)
        {
            nightSource.enabled = token.Data.nightRadius > 0.0f && selected;
            visionSource.enabled = token.Data.visionRadius > 0.0f && selected;
        }
    }
}