using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class MultiContentSizeFitter : ContentSizeFitter
    {
        private MultiContentSizeFitter parentSizeFitter;
        private bool isUpdating;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Find parent MultiContentSizeFitter
            parentSizeFitter = transform.parent.GetComponent<MultiContentSizeFitter>();
            isUpdating = true;
        }
        private void LateUpdate()
        {
            if (isUpdating)
            {
                Canvas.ForceUpdateCanvases();
                SetLayoutVertical();
                SetLayoutHorizontal();

                SetDirty();

                // Update parent size fitter
                if (parentSizeFitter != null) parentSizeFitter.UpdateSize();
                isUpdating = false;
            }
        }

        public void UpdateSize()
        {
            isUpdating = true;
        }
    }
}