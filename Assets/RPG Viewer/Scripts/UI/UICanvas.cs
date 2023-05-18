using UnityEngine;

namespace RPG
{
    public class UICanvas : MonoBehaviour
    {
        public static UICanvas Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
    }
}