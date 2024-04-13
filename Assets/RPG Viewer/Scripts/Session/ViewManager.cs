using FunkyCode;
using UnityEngine;

namespace RPG
{
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] private LightingManager2D lightingManager;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnViewChanged.AddListener(ChangeView);
            Events.OnSceneLoaded.AddListener(LoadView);
            Events.OnLightingChanged.AddListener(ChangeLighting);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnViewChanged.RemoveListener(ChangeView);
            Events.OnSceneLoaded.RemoveListener(LoadView);
            Events.OnLightingChanged.RemoveListener(ChangeLighting);
        }

        private void ChangeView(GameView view)
        {
            CameraSettings settings = lightingManager.cameras.Get(0);
            settings.cameraType = view == GameView.Clear ? CameraSettings.CameraType.Custom : CameraSettings.CameraType.MainCamera;
            lightingManager.cameras.Set(0, settings);

            // Return if fog is disabled
            if (!Session.Instance.Settings.darkness.enabled)
            {
                LoadClear();
                return;
            }

            switch (view)
            {
                case GameView.Player:
                    LoadPlayer();
                    return;
                case GameView.Vision:
                    LoadVision();
                    return;
                case GameView.Clear:
                    LoadClear();
                    return;
            }
        }
        private void LoadView(SceneData data)
        {
            CameraSettings settings = lightingManager.cameras.Get(0);
            settings.cameraType = SettingsHandler.Instance.LastView == GameView.Clear ? CameraSettings.CameraType.Custom : CameraSettings.CameraType.MainCamera;
            lightingManager.cameras.Set(0, settings);

            // Return if fog is disabled
            if (!data.darkness.enabled)
            {
                LoadClear();
                return;
            }

            switch (SettingsHandler.Instance.LastView)
            {
                case GameView.Player:
                    Lighting2D.LightmapPresets[0].darknessColor = data.darkness.color;
                    Lighting2D.LightmapPresets[0].darknessColor.a = data.darkness.globalLighting ? 0.0f : data.darkness.color.a;
                    Lighting2D.LightmapPresets[1].darknessColor = data.darkness.color;
                    return;
                case GameView.Vision:
                    Lighting2D.LightmapPresets[0].darknessColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                    Lighting2D.LightmapPresets[1].darknessColor = data.darkness.color;
                    Lighting2D.LightmapPresets[1].darknessColor.a = 0.9f;
                    return;
                case GameView.Clear:
                    Lighting2D.LightmapPresets[0].darknessColor.a = 0.0f;
                    Lighting2D.LightmapPresets[1].darknessColor.a = 0.0f;
                    return;
            }
        }
        private void LoadPlayer()
        {
            Lighting2D.LightmapPresets[0].darknessColor = Session.Instance.Settings.darkness.color;
            Lighting2D.LightmapPresets[0].darknessColor.a = Session.Instance.Settings.darkness.globalLighting ? 0.0f : Session.Instance.Settings.darkness.color.a;
            Lighting2D.LightmapPresets[1].darknessColor = Session.Instance.Settings.darkness.color;
        }
        private void LoadVision()
        {
            Lighting2D.LightmapPresets[0].darknessColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            Lighting2D.LightmapPresets[1].darknessColor = Session.Instance.Settings.darkness.color;
            Lighting2D.LightmapPresets[1].darknessColor.a = 0.9f;
        }
        private void LoadClear()
        {
            CameraSettings settings = lightingManager.cameras.Get(0);
            settings.cameraType = CameraSettings.CameraType.Custom;
            lightingManager.cameras.Set(0, settings);

            Lighting2D.LightmapPresets[0].darknessColor.a = 0.0f;
            Lighting2D.LightmapPresets[1].darknessColor.a = 0.0f;
        }

        private void ChangeLighting(LightingSettings data, bool globalUpdate)
        {
            CameraSettings settings = lightingManager.cameras.Get(0);
            settings.cameraType = SettingsHandler.Instance.LastView == GameView.Clear ? CameraSettings.CameraType.Custom : CameraSettings.CameraType.MainCamera;
            lightingManager.cameras.Set(0, settings);

            // Return if fog is disabled
            if (!data.enabled)
            {
                LoadClear();
                return;
            }

            switch (SettingsHandler.Instance.LastView)
            {
                case GameView.Player:
                    Lighting2D.LightmapPresets[0].darknessColor = data.color;
                    Lighting2D.LightmapPresets[0].darknessColor.a = data.globalLighting ? 0.0f : data.color.a;
                    Lighting2D.LightmapPresets[1].darknessColor = data.color;
                    return;
                case GameView.Vision:
                    Lighting2D.LightmapPresets[0].darknessColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                    Lighting2D.LightmapPresets[1].darknessColor = data.color;
                    Lighting2D.LightmapPresets[1].darknessColor.a = 0.9f;
                    return;
                case GameView.Clear:
                    Lighting2D.LightmapPresets[0].darknessColor.a = 0.0f;
                    Lighting2D.LightmapPresets[1].darknessColor.a = 0.0f;
                    return;
            }
        }
    }

    public enum GameView
    {
        Player,
        Vision,
        Clear
    }
}