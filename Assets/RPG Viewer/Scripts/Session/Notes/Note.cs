using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class Note : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private NoteUI noteUI;

        private NoteInfo info;
        private NoteData data;
        private NoteUI activeNote;

        public void LoadData(NoteInfo _info, NoteData _data)
        {
            info = _info;
            data = _data;

            header.text = data.header;
        }
        public void ReloadInfo(NoteInfo _info)
        {

        }
        public void ReloadData(NoteData _data)
        {

        }

        public void OpenNote(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.dragging || activeNote != null) return;

            activeNote = Instantiate(noteUI);
            activeNote.transform.SetParent(UICanvas.Instance.transform);
            activeNote.transform.SetAsLastSibling();
            activeNote.transform.localPosition = Vector3.zero;

            activeNote.LoadData(info, data);
        }
    }
}