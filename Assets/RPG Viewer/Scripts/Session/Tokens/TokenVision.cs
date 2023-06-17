using FunkyCode;
using UnityEngine;

namespace RPG
{
    public class TokenVision : MonoBehaviour
    {
        [SerializeField] private Light2D nightSource;
        [SerializeField] private Light2D visionSource;
        [SerializeField] private Light2D lightSource;

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
            lightSource.enabled = false;
        }

        public void Reload()
        {
            updateRequired = true;
        }
        private void LoadLighting()
        {
            float feetToUnits = Session.Instance.Grid.CellSize * 0.2f;
            nightSource.enabled = token.Data.nightRadius > 0.0f;
            visionSource.enabled = token.Data.visionRadius > 0.0f;
            nightSource.size = token.Data.nightRadius * feetToUnits;
            visionSource.size = token.Data.visionRadius * feetToUnits;
        }
    }
}