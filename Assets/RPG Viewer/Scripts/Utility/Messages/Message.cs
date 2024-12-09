using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class Message : MonoBehaviour
    {
        public string message;

        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Icons")]
        [SerializeField] private Image icon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite warningIcon;
        [SerializeField] private Sprite infoIcon;
        [SerializeField] private Sprite successIcon;

        [Header("Colors")]
        [SerializeField] private Image outline;
        [SerializeField] private Color errorColor;
        [SerializeField] private Color warningColor;
        [SerializeField] private Color infoColor;
        [SerializeField] private Color successColor;

        public void Load(string msg, MessageType type)
        {
            message = msg;
            text.text = msg;

            switch (type)
            {
                case MessageType.Error:
                    icon.sprite = errorIcon;
                    icon.color = errorColor;
                    outline.color = errorColor;
                    break;
                case MessageType.Warning:
                    icon.sprite = warningIcon;
                    icon.color = warningColor;
                    outline.color = warningColor;
                    break;
                case MessageType.Info:
                    icon.sprite = infoIcon;
                    icon.color = infoColor;
                    outline.color = infoColor;
                    break;
                case MessageType.Success:
                    icon.sprite = successIcon;
                    icon.color = successColor;
                    outline.color = successColor;
                    break;
            }
        }

        public void Close()
        {
            canvasGroup.LeanAlpha(0, 0.5f).setOnComplete(() => Destroy(gameObject));
        }
    }

    public enum MessageType
    {
        Error,
        Warning,
        Info,
        Success
    }
}