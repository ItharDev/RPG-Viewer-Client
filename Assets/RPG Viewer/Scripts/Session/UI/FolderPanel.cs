using TMPro;
using UnityEngine;

namespace RPG
{
    public class FolderPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;

        private FolderType type;
        private string path;

        public void LoadData(string _path, FolderType _type)
        {
            path = _path;
            type = _type;
            inputField.text = "";
        }

        public void SaveData()
        {
            switch (type)
            {
                case FolderType.Blueprint:
                    FindObjectOfType<MasterPanel>().AddBlueprintFolder(path, inputField.text);
                    break;
                case FolderType.Scene:
                    FindObjectOfType<MasterPanel>().AddSceneFolder(path, inputField.text);
                    break;
                case FolderType.Journal:
                    FindObjectOfType<MasterPanel>().AddJournalFolder(path, inputField.text);
                    break;
            }

            gameObject.SetActive(false);
        }
    }

    public enum FolderType
    {
        Blueprint,
        Scene,
        Journal,
    }
}