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

        private void OnValidate()
        {
            // Get reference of main class
            if (token == null) token = GetComponent<Token>();
        }

        public void DisableVision()
        {

        }
        public void DisableLight()
        {

        }

        public void Reload()
        {
            // TODO: Load lighting
        }
    }
}