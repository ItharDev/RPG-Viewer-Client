using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GUIFPS : MonoBehaviour
{
    int fps = 0;
    float timer = 0;
    int fpsResult = 0;
    Text text;

    void OnGUI() {
        GUIStyle style = new GUIStyle();
        int size = Screen.height / 10;
        style.fontSize = size;
        style.normal.textColor = Color.black;
        style.alignment = TextAnchor.UpperRight;

        
        
        GUI.Label(new Rect(Screen.width - 200 - 20, 0, 200, 200), Get(), style);

      
        style.fontSize = size + 2;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperRight;
        
        GUI.Label(new Rect(Screen.width - 200 + 2 - 20, 0 + 2, 200, 200), Get(),style);

        
    }

    public string Get() {
        if (Application.targetFrameRate > 0) {
             return(fpsResult.ToString() + " / " + Application.targetFrameRate);
        } else {
             return(fpsResult.ToString());
        }
    }

    void OnRenderObject() {
        fps += 1;

        if (Time.realtimeSinceStartup > timer + 1){
            timer = Time.realtimeSinceStartup;
            fpsResult = fps;
            fps = 0;
        }
    }
}
