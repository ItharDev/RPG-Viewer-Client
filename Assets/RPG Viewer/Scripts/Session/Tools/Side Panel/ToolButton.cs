using UnityEngine;

namespace RPG
{
    public class ToolButton : MonoBehaviour
    {
        [SerializeField] private GameObject outline;

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