using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    [Range(1, 200)]
    public int targetFPS;

    [Range(0, 4)]
    public int vSyncCount;

    private TMP_InputField fpsInput;

    private int lastFPS;
    private int lastVSync;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        UpdateSettings();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            if (fpsInput == null)
            {
                fpsInput = GameObject.Find("Fps Input").GetComponent<TMP_InputField>();
                fpsInput.text = PlayerPrefs.GetString("target-fps");
                UpdateFPS();
            }
        }

        if (Input.GetKeyDown(KeyCode.F12)) Screen.fullScreenMode = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? FullScreenMode.Windowed : FullScreenMode.ExclusiveFullScreen;
        if (lastFPS != targetFPS)
        {
            lastFPS = targetFPS;
            UpdateSettings();
        }
        if (lastVSync != vSyncCount)
        {
            lastVSync = vSyncCount;
            UpdateSettings();
        }
    }

    private void UpdateSettings()
    {
        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = vSyncCount;
    }

    public void UpdateFPS()
    {
        if (string.IsNullOrEmpty(fpsInput.text)) fpsInput.text = targetFPS.ToString();
        targetFPS = int.Parse(fpsInput.text);

        PlayerPrefs.SetString("target-fps", fpsInput.text);
    }
}