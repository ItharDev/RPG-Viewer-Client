using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class DynamicAspectRatio : AspectRatioFitter
    {
        private Sprite currentSprite;
        private Image graphic;

        protected override void Awake()
        {
            base.Awake();
            graphic = GetComponent<Image>();
        }
        protected override void Update()
        {
            base.Update();
            if (currentSprite != graphic.sprite)
            {
                currentSprite = graphic.sprite;
                aspectRatio = (float)currentSprite.texture.width / currentSprite.texture.height;
                SetDirty();
            }
        }
    }
}