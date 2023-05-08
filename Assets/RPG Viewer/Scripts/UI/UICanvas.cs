using UnityEngine;

namespace RPG
{
    public class UICanvas : MonoBehaviour
    {
        public static UICanvas Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
    }
}