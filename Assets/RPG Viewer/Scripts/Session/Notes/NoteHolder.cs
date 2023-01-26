using System;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class NoteHolder : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField headerInput;
        [SerializeField] private TMP_InputField textInput;
        [SerializeField] private Image image;

        public bool Selected;
        public NoteData Data;

        private NoteManager manager;

        public void Select(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;
            manager.SelectNote(this);

            panel.SetActive(true);
            panel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            Selected = true;
        }

        public void Deselect()
        {
            Selected = false;
            panel.SetActive(false);
        }

        public void LoadData(NoteData data, NoteManager noteManager)
        {
            Data = data;
            manager = noteManager;

            transform.localPosition = new Vector3(data.position.x, data.position.y, -1);
            if (Data.owner == SocketManager.UserId) gameObject.SetActive(true);
            else gameObject.SetActive(Data.isPublic);
        }
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