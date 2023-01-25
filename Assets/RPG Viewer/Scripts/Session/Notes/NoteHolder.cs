using System;
using UnityEngine;

namespace RPG
{
    public class NoteHolder : MonoBehaviour
    {
        public bool Selected;
    }

    [Serializable]
    public struct NoteData
    {
        public string id;
        public string owner;
        public string header;
        public string text;
        public string image;
        public bool isPublic;
        public Vector2 position;
    }
}