using UnityEngine;

namespace FunkyCode
{
    [ExecuteInEditMode]
    public class LightSpriteSample : MonoBehaviour
    {
        public Sprite sprite;

        public Scriptable.LightSprite2D lightSprite;
    
        void Start() {
            Scriptable.LightSprite2D light = new Scriptable.LightSprite2D();

            light.SetActive(true);

            light.Sprite = sprite;

            light.Position = Vector3.zero;
            light.Scale = Vector3.one;
            light.Rotation = 0;

            lightSprite = light;
        }
    }
}