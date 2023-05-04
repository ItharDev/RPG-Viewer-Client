using TMPro;
using UnityEngine;

public class Message : MonoBehaviour
{
    public string message;

    [SerializeField] private TMP_Text text;

    public void Load(string msg)
    {
        message = msg;
        text.text = msg;
    }
}