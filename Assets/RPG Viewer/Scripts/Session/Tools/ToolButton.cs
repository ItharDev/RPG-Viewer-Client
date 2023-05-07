using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class ToolButton : MonoBehaviour
    {
        [SerializeField] private GameObject outline;
        public Image Icon;
        public TMP_Text Header;

        public void Select()
        {
            outline.SetActive(true);
        }
        public void Deselect()
        {
            outline.SetActive(false);
        }
    }
}