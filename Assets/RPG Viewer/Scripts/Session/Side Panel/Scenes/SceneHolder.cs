using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class SceneHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private Image icon;

        public void LoadData(string id, string path)
        {
            SocketManager.EmitAsync("get-scene", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    // Load Data
                    await UniTask.SwitchToMainThread();

                    SceneSettings data = JsonUtility.FromJson<SceneSettings>(callback.GetValue(1).ToString());
                    data.id = id;
                    LoadData(data);
                    return;
                }

                MessageManager.QueueMessage(callback.GetValue().GetString());
            }, id);
        }

        private void LoadData(SceneSettings data)
        {
            header.text = data.data.name;
            WebManager.Download(data.data.image, true, async (bytes) =>
            {
                // Return if image couldn't be loaded
                if (bytes == null) return;

                await UniTask.SwitchToMainThread();

                // Generate texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                icon.color = Color.white;
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(50.0f, 50.0f);
            });
        }
    }
}