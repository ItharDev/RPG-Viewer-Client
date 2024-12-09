using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class SessionCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image image;

        private string id;

        public void SetData(string id, string name, string imageId)
        {
            this.id = id;
            nameText.text = name;

            WebManager.Download(imageId, true, async (bytes) =>
            {
                // Check if landing page exists
                if (bytes == null)
                {
                    MessageManager.QueueMessage("Failed to load landing page, please try again", MessageType.Error);
                    return;
                }

                await UniTask.SwitchToMainThread();

                // Create landing page texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                image.sprite = sprite;
            });
        }
    }
}