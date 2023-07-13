using System;
using UnityEngine;

namespace RPG
{
    [Serializable]
    public struct NoteInfo
    {
        public string id;
        public Vector2 position;
        public string owner;
        public bool global;
        public bool IsOwner { get { return owner == GameData.User.id || global; } }

        public NoteInfo(string _id, Vector2 _position, string _owner, bool _global)
        {
            id = _id;
            position = _position;
            owner = _owner;
            global = _global;
        }
    }

    [Serializable]
    public struct NoteData
    {
        public string id;
        public string header;
        public string text;
        public string image;

        public NoteData(string _id, string _header, string _text, string _image)
        {
            id = _id;
            header = _header;
            text = _text;
            image = _image;
        }
    }
}