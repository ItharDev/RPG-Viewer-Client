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

        private void OnEnable()
        {
            // Add event listeners
            Events.OnNoteCreated.AddListener(CreateNote);
            Events.OnNoteInfoModified.AddListener(ModifyInfo);
            Events.OnNoteDataModified.AddListener(ModifyData);
            Events.OnStateChanged.AddListener(ReloadNotes);
            Events.OnSceneLoaded.AddListener(LoadNotes);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnNoteCreated.RemoveListener(CreateNote);
            Events.OnNoteInfoModified.RemoveListener(ModifyInfo);
            Events.OnNoteDataModified.RemoveListener(ModifyData);
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
                    data.id = info.data;
                    CreateNote(info, data);

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, info.data);
        }
        private void ModifyInfo(string id, NoteInfo info)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            // Load new info
            note.ReloadInfo(info);
        }
        private void ModifyData(string id, NoteData data)
        {
            // Find the correct note
            Note note = notes[id];

            // Check if note was found
            if (note == null) return;

            // Load new data
            note.ReloadData(data);
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
                if (oldState.synced && !newState.synced)
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
    }
}