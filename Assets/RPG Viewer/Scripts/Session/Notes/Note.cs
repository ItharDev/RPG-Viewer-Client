using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class Note : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private NoteUI noteUI;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;

        private NoteInfo info;
        private NoteData data;
        private NoteUI activeNote;
        private bool dragging;
        private bool loaded;

        private void Update()
        {
            if (!loaded && Session.Instance.Grid.Grid != null)
            {
                loaded = true;
                UpdateData();
            }
            if (dragging)
            {
                // Update our position when being dragged
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0.0f;
                transform.localPosition = mousePos;
            }
        }

        private void UpdateData()
        {
            header.text = data.header;
            transform.position = info.position;
            canvasGroup.alpha = info.IsOwner ? 1.0f : 0.0f;
            canvasGroup.blocksRaycasts = info.IsOwner;

            float cellSize = Session.Instance.Grid.CellSize;

            // Update canvas
            canvas.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);
        }
        public void LoadData(NoteInfo _info, NoteData _data)
        {
            info = _info;
            data = _data;

            loaded = false;
        }
        public void ReloadPosition(Vector2 _position)
        {
            info.position = _position;
            transform.position = _position;
        }
        public void ReloadText(string _text)
        {
            data.text = _text;
        }
        public void ReloadHeader(string _header)
        {
            data.header = _header;
            header.text = _header;

        }
        public void ReloadImage(string _image)
        {
            data.image = _image;
        }
        public void SetGlobal(bool _isGlobal)
        {
            info.global = _isGlobal;
            canvasGroup.alpha = info.IsOwner ? 1.0f : 0.0f;
            canvasGroup.blocksRaycasts = info.IsOwner;
        }

        public void BeginDrag(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            if (pointerData.button != PointerEventData.InputButton.Left) return;

            dragging = true;
        }
        public void EndDrag(BaseEventData eventData)
        {
            dragging = false;
            NoteInfo newInfo = info;
            newInfo.position = transform.localPosition;
            SocketManager.EmitAsync("move-note", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    transform.localPosition = newInfo.position;
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, info.id, JsonUtility.ToJson(newInfo));
        }
        public void OpenNote(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;
            if (ToolHandler.Instance.ActiveTool == Tool.Notes_Delete)
            {
                DeleteNote();
                return;
            }
            if (pointerData.dragging) return;

            ShowNote();
            if (Input.GetKey(KeyCode.LeftShift)) activeNote.ShowNote();
        }

        public void ShowNote()
        {
            if (activeNote != null) return;
            activeNote = Instantiate(noteUI);
            activeNote.transform.SetParent(UICanvas.Instance.transform);
            activeNote.transform.SetAsLastSibling();
            activeNote.transform.localPosition = Vector3.zero;

            activeNote.LoadData(info, data);
            Session.Instance.NoteManager.OpenNote(data.id, activeNote);
        }

        private void DeleteNote()
        {
            // Close any active notes
            if (activeNote != null) activeNote.Close();

            SocketManager.EmitAsync("remove-note", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, info.id);
        }
    }
}