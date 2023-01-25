using UnityEngine;

namespace FunkyCode
{
	public class LightTransform
	{
		private bool update = true;
		
		public bool UpdateNeeded => update;

		public Vector2 position = Vector2.zero;
		public float rotation = 0f;
		private float size = 0f;
		private float spotAngleInner = 360;
		private float spotAngleOuter = 360;


		private float outerAngle = 15;

		private Color color = Color.white;

		private Sprite sprite;
		private bool flipX = false;
		private bool flipY = false;

		private float normalIntensity = 1;
		private float normalDepth = 1;

		public Rect WorldRect = new Rect();

		public void ForceUpdate()
		{
			update = true;
		}

		public void ClearUpdate() {
			update = false;
		}

		public void Update(Light2D light)
		{
			if (light.gameObject == null)
			{
				return;
			}

			if (light.transform == null)
			{
				return;
			}
			
			Transform transform = light.transform;

			Vector2 position2D = LightingPosition.GetPosition2D(transform.position);

			float rotation2D = 0;

			switch(light.applyRotation)
			{
				case Light2D.Rotation.Local:

					rotation2D = transform.localRotation.eulerAngles.z;
			
				break;

				case Light2D.Rotation.World:
			
					rotation2D = transform.rotation.eulerAngles.z;
			
				break;
			}
			
			if (position != position2D)
			{
				position = position2D;

				update = true;
			}

			if (rotation != rotation2D)
			{
				rotation = rotation2D;

				update = true;
			}

			if (size != light.size)
			{
				size = light.size;

				update = true;
			}

			if (sprite != light.sprite)
			{
				sprite = light.sprite;

				update = true;
			}

			if (flipX != light.spriteFlipX)
			{
				flipX = light.spriteFlipX;

				update = true;
			}

			if (flipY != light.spriteFlipY)
			{
				flipY = light.spriteFlipY;

				update = true;
			}

			if (spotAngleInner != light.spotAngleInner)
			{
				spotAngleInner = light.spotAngleInner;

				update = true;
			}

			if (spotAngleOuter != light.spotAngleOuter)
			{
				spotAngleOuter = light.spotAngleOuter;

				update = true;
			}
			
			if (outerAngle != light.outerAngle)
			{
				outerAngle = light.outerAngle;

				update = true;
			}
			
			if (normalIntensity != light.bumpMap.intensity)
			{
				normalIntensity = light.bumpMap.intensity;

				update = true;
			}

			if (normalDepth != light.bumpMap.depth)
			{
				normalDepth = light.bumpMap.depth;

				update = true;
			}

			if (update)
			{
				WorldRect = new Rect(position.x - size, position.y - size, size * 2, size * 2);
			}
			
			// no need to update for color and alpha
			if (color != light.color)
			{
				color = light.color;
			}
		}
	}
}