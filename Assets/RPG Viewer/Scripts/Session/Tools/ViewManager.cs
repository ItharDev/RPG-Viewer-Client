using FunkyCode;
using UnityEngine;

namespace RPG
{
    public class ViewManager : MonoBehaviour
    {
        private void OnEnable()
        {
            // Add event listeners
            Events.OnViewChanged.AddListener(ChangeView);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnViewChanged.RemoveListener(ChangeView);
        }

        private void ChangeView(GameView view)
        {
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
        private void LoadPlayer()
        {
            Lighting2D.LightmapPresets[0].darknessColor = Session.Instance.Settings.darkness.color;
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
            Lighting2D.LightmapPresets[0].darknessColor.a = 0.0f;
            Lighting2D.LightmapPresets[1].darknessColor.a = 0.0f;
        }
    }

    public enum GameView
    {
        Player,
        Vision,
        Clear
    }
}