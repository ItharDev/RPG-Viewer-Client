using System.Net.Mime;
using System;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [Range(1, 200)]
    public int targetFPS;

    [Range(0, 4)]
    public int vSyncCount;

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
}