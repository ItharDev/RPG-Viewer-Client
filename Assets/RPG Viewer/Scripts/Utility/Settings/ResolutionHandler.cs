using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class ResolutionHandler : MonoBehaviour
    {
        [SerializeField] private ResolutionButton resolutionButton;
        [SerializeField] private Transform buttonParent;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private Toggle fullscreenToggle;

        private List<Resolution> resolutions = new List<Resolution>();

        public void LoadResolutions()
        {
            resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToList();
            resolutions.Reverse();

            int width = PlayerPrefs.GetInt("screen_width", Screen.currentResolution.width);
            int height = PlayerPrefs.GetInt("screen_height", Screen.currentResolution.height);
            bool isFullscreen = bool.Parse(PlayerPrefs.GetString("is_fullscreen", Screen.fullScreen.ToString()));
            Debug.Log(width);
            Debug.Log(height);
            Debug.Log(isFullscreen);
            Screen.SetResolution(width, height, isFullscreen);

            for (int i = 0; i < resolutions.Count; i++)
            {
                // Only allow 16:9 resolutions
                if (resolutions[i].width / resolutions[i].height - 16 / 9 != 0) continue;

                ResolutionButton button = Instantiate(resolutionButton, buttonParent);
                bool selected = resolutions[i].width == width && resolutions[i].height == height;
                button.LoadResolution(resolutions[i], toggleGroup, selected);
            }

            fullscreenToggle.isOn = Screen.fullScreen;
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
    }
}