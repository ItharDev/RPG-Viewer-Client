using UnityEngine;

namespace RPG
{
    public class BlockPause : MonoBehaviour
    {
        public void StartBlocking()
        {
            PauseHandler.Instance.AddBlocker(this);
        }
        public void StopBlocking()
        {
            PauseHandler.Instance.RemoveBlocker(this);
        }
    }
}
