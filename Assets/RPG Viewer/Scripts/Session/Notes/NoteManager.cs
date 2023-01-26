using System;
using System.Collections.Generic;
using System.Linq;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class NoteManager : MonoBehaviour
    {
        [SerializeField] private NoteHolder notePrefab;
        [SerializeField] private Transform noteParent;

        private List<NoteHolder> notes = new List<NoteHolder>();
        private StateManager stateManager;
        private Session session;

        private void Update()
        {
            if (stateManager.ToolState == ToolState.Notes)
            {
                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    bool modified = false;
                    for (int i = 0; i < notes.Count; i++)
                    {
                        if (notes[i].Selected)
                        {
                            modified = true;
                            break;
                        }
                    }
                    if (!modified) CreateNote(Camera.main.ScreenToWorldPoint(Input.mousePosition));
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

        public void SelectNote(NoteHolder holder)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i] != holder) notes[i].Deselect();
            }
        }
        public void AddNote(NoteData data)
        {
            var note = Instantiate(notePrefab, noteParent);
            note.LoadData(data, this);
            notes.Add(note);
        }
    }
}