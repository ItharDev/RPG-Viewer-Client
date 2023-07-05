using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace RPG
{
    [Serializable]
    public struct NoteInfo
    {
        public string id;
        public Vector2 position;
        public string data;
        public string owner;
        public bool global;
    }

    [Serializable]
    public struct NoteData
    {
        public string id;
        public List<SectionData> sections;

        public NoteData(string _id, List<SectionData> _sections)
        {
            id = _id;
            sections = _sections;
        }
    }

    [Serializable]
    public struct SectionData
    {
        public string text;
        public string image;

        public SectionData(string _text, string _image)
        {
            text = _text;
            image = _image;
        }
    }
}