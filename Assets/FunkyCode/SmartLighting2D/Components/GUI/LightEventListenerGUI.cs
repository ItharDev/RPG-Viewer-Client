using UnityEngine;

namespace FunkyCode
{
    [ExecuteInEditMode]
    public class LightEventListenerGUI : MonoBehaviour
    {
        static private Texture pointTexture;

        private LightEventListener lightEventReceiver;

        private void OnEnable()
        {
            lightEventReceiver = GetComponent<LightEventListener>();
        }
    
        static private Texture GetPointTexture()
        {
            if (pointTexture == null)
            {
                pointTexture = Resources.Load<Texture>("Textures/dot");
            }

            return(pointTexture);
        }

        void OnGUI()
        {
            if (Camera.main == null)
            {
                return;
            }
            
            Vector2 middlePoint = Camera.main.WorldToScreenPoint(transform.position);

            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            
            string display = ((int)(lightEventReceiver.visability * 100)).ToString();

            GUIStyle style = new GUIStyle();
            int size = Screen.height / 20;
            style.fontSize = size;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            int pointSize = Screen.height / 80;

            GUI.Label(new Rect(middlePoint.x - 50, Screen.height - middlePoint.y - 50, 100, 100), display + "%", style);

            if (lightEventReceiver.CollisionInfo == null)
            {
                return;
            }

            if (lightEventReceiver.CollisionInfo.Value.points != null)
            {
                foreach(Vector2 point in lightEventReceiver.CollisionInfo.Value.points)
                {
                    Vector2 pos = lightEventReceiver.CollisionInfo.Value.light.transform.position;
                    Vector2 screenPoint = Camera.main.WorldToScreenPoint(point + pos);

                    GUI.DrawTexture(new Rect(screenPoint.x - pointSize, Screen.height - screenPoint.y - pointSize, pointSize * 2, pointSize * 2), GetPointTexture());
                }
            }
        }
    }
}