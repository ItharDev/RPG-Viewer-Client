using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class ResolutionButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private Toggle toggle;

        private Resolution resolution;

        public void LoadResolution(Resolution _resolution, ToggleGroup toggleGroup, bool selected)
        {
            resolution = _resolution;
            toggle.group = toggleGroup;

            header.text = $"{resolution.width} x {resolution.height}";

            if (!selected) return;
            toggle.isOn = true;
        }

        public void SetResolution(bool selected)
        {
            if (!selected) return;
            Debug.Log(resolution);

            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

            PlayerPrefs.SetInt("screen_width", resolution.width);
            PlayerPrefs.SetInt("screen_height", resolution.height);
            PlayerPrefs.SetString("is_fullscreen", Screen.fullScreen.ToString());
        }
    }
}