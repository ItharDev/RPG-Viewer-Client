using System.Collections.Generic;
using System.Linq;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class NoteManager : MonoBehaviour
    {
        [SerializeField] private NoteHolder notePrefab;
        [SerializeField] private Transform noteParent;

        private List<NoteHolder> notes = new List<NoteHolder>();
        [SerializeField] private StateManager stateManager;
        private Session session;

        private void Update()
        {
            if (stateManager.ToolState == ToolState.Notes)
            {
                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    CreateNote(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
            }
        }

        private async void CreateNote(Vector2 pos)
        {
            var data = new NoteData()
            {
                owner = "",
                header = "",
                text = "",
                image = "",
                isPublic = false,
                position = pos,
            };

            await SocketManager.Socket.EmitAsync("create-note", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data));
        }

        public void UnloadNotes()
        {
            for (int i = 0; i < notes.Count; i++)
            {
                Destroy(notes[i].Panel);
                Destroy(notes[i].gameObject);
            }

            notes.Clear();
        }

        public void AddNote(NoteData data)
        {
            var note = Instantiate(notePrefab, noteParent);
            note.LoadData(data, this);
            notes.Add(note);
        }
        public void ModifyText(string id, string newText)
        {
            var note = notes.FirstOrDefault(x => x.Data.id == id);
            if (note == null) return;

            note.UpdateText(newText);
        }
        public void ModifyHeader(string id, string newText)
        {
            var note = notes.FirstOrDefault(x => x.Data.id == id);
            if (note == null) return;

            note.UpdateHeader(newText);
        }
        public void ModifyImage(string id, string newImage)
        {
            var note = notes.FirstOrDefault(x => x.Data.id == id);
            if (note == null) return;

            note.UpdateImage(newImage);
        }
        public void Move(string id, Vector2 pos)
        {
            var note = notes.FirstOrDefault(x => x.Data.id == id);
            if (note == null) return;

            note.transform.localPosition = new Vector3(pos.x, pos.y, -1);
        }
        public void SetPublic(string id, bool newState)
        {
            var note = notes.FirstOrDefault(x => x.Data.id == id);
            if (note == null) return;

            note.SetPublic(newState);
        }
        public void Remove(string id)
        {
            var note = notes.FirstOrDefault(x => x.Data.id == id);
            if (note == null) return;

            notes.Remove(note);
            Destroy(note.Panel);
            Destroy(note.confirmPanel);
            Destroy(note.gameObject);
        }
        public void Show(string id)
        {
            var note = notes.FirstOrDefault(x => x.Data.id == id);
            if (note == null) return;

            note.Show();
        }
    }
}