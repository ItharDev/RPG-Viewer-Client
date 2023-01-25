using UnityEngine;
using UnityEngine.SceneManagement;

public class MainDemoManager : MonoBehaviour
{
    public int scenesCount = 1;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        UnityEngine.SceneManagement.SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        int size = Screen.height / 25;
        style.fontSize = size;

        style.alignment = TextAnchor.UpperLeft;

        float sizeH = Screen.height / 15;

        for(int i = 0; i < scenesCount; i++)
        {
            if (GUI.Button(new Rect(10, i * (sizeH * 1.1f) + 10, sizeH * 10, sizeH), NameFromIndex(i+1), style))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(i+1, LoadSceneMode.Single);
            }
        }
    }

    private static string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');

        return name.Substring(0, dot);
    }
    
}
