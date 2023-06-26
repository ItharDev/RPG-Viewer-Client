using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class GridCorner : MonoBehaviour
    {
        [SerializeField] private GridUI grid;
        [SerializeField] private CornerType type;

        public void OnDrag(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            grid.MoveCorner(type);
        }
    }

    public enum CornerType
    {
        Top_Left,
        Top_Right,
        Bottom_Left,
        Bottom_Right,
    }
}