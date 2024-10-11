using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class TokenDrag : MonoBehaviour
    {
        [SerializeField] private Image icon;
        private TokenData data;
        private Action<Vector2> callback;

        private void Update()
        {
            transform.position = Input.mousePosition;
            if (Input.GetMouseButtonUp(0)) EndDrag();
        }

        private void EndDrag()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            callback?.Invoke(mousePosition);
            Destroy(gameObject);
        }

        public void LoadData(TokenData _data, Sprite sprite, Action<Vector2> onEndDrag)
        {
            data = _data;
            icon.sprite = sprite;
            callback = onEndDrag;
        }
    }
}