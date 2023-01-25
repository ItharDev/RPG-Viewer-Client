using UnityEngine;

namespace FunkyCode
{
    [ExecuteInEditMode]
    public class LightEventListenerCountGUI : MonoBehaviour
    {
        static private Texture pointTexture;

        private LightEventListenerCount lightEventReceiver;

        private void OnEnable()
        {
            lightEventReceiver = GetComponent<LightEventListenerCount>();
        }

        void OnGUI()
        {
            if (Camera.main == null)
            {
                return;
            }
            
            Vector2 middlePoint = Camera.main.WorldToScreenPoint(transform.position);

            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            
            string display = lightEventReceiver.lights.Count.ToString();

            int size = Screen.height / 20;

            GUIStyle style = new GUIStyle();
            style.fontSize = size;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            int pointSize = Screen.height / 80;

            GUI.Label(new Rect(middlePoint.x - 50, Screen.height - middlePoint.y - 50, 100, 100), display, style);
        }
    }
}