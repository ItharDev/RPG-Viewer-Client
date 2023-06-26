using FunkyCode;
using UnityEngine;

namespace RPG
{
    [RequireComponent(typeof(Light2D))]
    public class LightSource : MonoBehaviour
    {
        private PresetData data;
        private Light2D source;

        private void Awake()
        {
            source = GetComponent<Light2D>();
        }

        public void LoadData(PresetData _data)
        {
            data = _data;
            source.size = data.radius * 0.2f * Session.Instance.Grid.CellSize;
            source.color = data.color;
        }
        public void Toggle(bool enabled)
        {
            source.enabled = enabled;
        }
    }
}