using UnityEngine;

namespace RPG
{
    public class PointerHandler : MonoBehaviour
    {
        public void UpdatePointer(Vector2 newPos)
        {
            transform.position = newPos;
        }
    }
}