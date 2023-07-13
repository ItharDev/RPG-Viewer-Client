using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class NoteManager : MonoBehaviour
    {
        [SerializeField] private Note notePrefab;
        [SerializeField] private Transform noteParent;

        private Dictionary<string, Note> notes = new Dictionary<string, Note>();
        private Dictionary<string, NoteUI> openNotes = new Dictionary<string, NoteUI>();

        private void OnEnable()
        {
            // Add event listeners
            Events.OnNoteCreated.AddListener(CreateNote);
            Events.OnNoteMoved.AddListener(MoveNote);
            Events.OnNoteTextModified.AddListener(ModifyText);
            Events.OnNoteHeaderModified.AddListener(ModifyHeader);
            Events.OnNoteImageModified.AddListener(ModifyImage);
            Events.OnNoteSetToGlobal.AddListener(SetGlobal);
            Events.OnNoteRemoved.AddListener(RemoveNote);
            Events.OnNoteShowed.AddListener(ShowNote);
            Events.OnStateChanged.AddListener(ReloadNotes);
            Events.OnSceneLoaded.AddListener(LoadNotes);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnNoteCreated.RemoveListener(CreateNote);
            Events.OnNoteMoved.RemoveListener(MoveNote);
            Events.OnNoteTextModified.RemoveListener(ModifyText);
            Events.OnNoteHeaderModified.RemoveListener(ModifyHeader);
            Events.OnNoteImageModified.RemoveListener(ModifyImage);
            Events.OnNoteSetToGlobal.RemoveListener(SetGlobal);
            Events.OnNoteRemoved.RemoveListener(RemoveNote);
            Events.OnNoteShowed.RemoveListener(ShowNote);
            Events.OnStateChanged.RemoveListener(ReloadNotes);
            Events.OnSceneLoaded.RemoveListener(LoadNotes);
        }

        private void CreateNote(NoteInfo info, NoteData data)
        {
            // Instantiate note and load its data
            Note note = Instantiate(notePrefab, noteParent);
            note.LoadData(info, data);

            // Store note to dictionary
            notes.Add(info.id, note);
        }
        private void GetNote(NoteInfo info)
        {
            SocketManager.EmitAsync("get-note", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    NoteData data = JsonUtility.FromJson<NoteData>(callback.GetValue(1).ToString());
                    data.id = info.id;
                    CreateNote(info, data);

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, info.id);
        }
        private void MoveNote(string id, Vector2 position)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            // Load new info
            note.ReloadPosition(position);
        }
        private void ModifyText(string id, string text, string uid)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            // Load new info
            note.ReloadText(text);
            if (openNotes.ContainsKey(id)) openNotes[id].ReloadText(text, uid != GameData.User.id);
        }
        private void ModifyHeader(string id, string header, string uid)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            // Load new info
            note.ReloadHeader(header);
            Debug.Log(uid);
            if (openNotes.ContainsKey(id)) openNotes[id].ReloadHeader(header, uid != GameData.User.id);
        }
        private void ModifyImage(string id, string image)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            // Load new info
            note.ReloadImage(image);
            if (openNotes.ContainsKey(id)) openNotes[id].ReloadImage(image);
        }
        private void SetGlobal(string id, bool isGlobal)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            // Load new info
            note.SetGlobal(isGlobal);
            if (openNotes.ContainsKey(id)) openNotes[id].SetGlobal(isGlobal);
        }
        private void RemoveNote(string id)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            notes.Remove(id);
            Destroy(note.gameObject);

            if (!openNotes.ContainsKey(id)) return;
            Destroy(openNotes[id]);
            RemoveNote(id);
        }
        private void ShowNote(string id)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            note.ShowNote();
        }

        private void ReloadNotes(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadNotes();
            }
            else
            {
                // Unload notes if syncing was disabled
                if (!newState.synced)
                {
                    UnloadNotes();
                    return;
                }

                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadNotes();
            }
        }
        private void UnloadNotes()
        {
            // Loop through each note
            foreach (var item in notes)
            {
                // Continue if note is null
                if (item.Value == null) continue;
                Destroy(item.Value.gameObject);
            }

            // Clear list
            notes.Clear();
        }
        private void LoadNotes(SceneData settings)
        {
            // Get lighting data
            Dictionary<string, NoteInfo> list = settings.notes;

            // Generate lights
            for (int i = 0; i < list.Count; i++)
            {
                GetNote(list.ElementAt(i).Value);
            }
        }

        public void OpenNote(string id, NoteUI note)
        {
            if (openNotes.ContainsKey(id) || openNotes.ContainsValue(note)) return;
            openNotes.Add(id, note);
        }
        public void CloseNote(string id)
        {
            if (!openNotes.ContainsKey(id)) return;
            openNotes.Remove(id);
        }
    }
}