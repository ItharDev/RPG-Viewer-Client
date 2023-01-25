using UnityEngine;

namespace FunkyCode
{
	public class LightColliderTransform
	{
		private bool update = true;

		public bool UpdateNeeded
		{
			get => update;
			set => update = value;
		}

		public Vector2 Position {private set; get;}
		public Vector2 Scale {private set; get;}
		public float Rotation {private set; get;}
		public float shadowHeight = 0;
		public float shadowTranslucency = 0;

		private Vector3 position3D = Vector3.zero;
		private bool flipX = false;
		private bool flipY = false;
		private Vector2 size = Vector2.one;

		private LightColliderShape shape;

		private LightCollider2D lightCollider;

		public LightColliderTransform()
		{
			Position = Vector2.zero;
			Rotation = 0;
			Scale = Vector3.zero;
		}

		public void SetShape(LightColliderShape shape, LightCollider2D lightCollider)
		{
			this.shape = shape;

			this.lightCollider = lightCollider;
		}

		public void Reset()
		{
			Position = Vector2.zero;
			Rotation = 0;
			Scale = Vector3.zero;
		}

		private void UpdateTransform(Transform transform)
		{
			Vector3 newPosition3D = transform.position;
			Vector2 position2D = LightingPosition.GetPosition2D(transform.position);

			Vector2 scale2D = transform.lossyScale;
			float rotation2D = LightingPosition.GetRotation2D(transform);

			if (Scale != scale2D)
			{
				Scale = scale2D;

				update = true;
			}

			if (Rotation != rotation2D)
			{
				Rotation = rotation2D;

				update = true;
			}

			if (position3D != newPosition3D)
			{
				position3D = newPosition3D;

				update = true;
			}

			if (Position != position2D)
			{
				Position = position2D;

				update = true;
			}
		}

		public void Update(bool force)
		{
			Transform transform = shape.transform;
			if (!transform)
			{
				return;
			}

			if (transform.hasChanged || force)
			{
				transform.hasChanged = false;

				UpdateTransform(transform);
			}

			if (shadowTranslucency != lightCollider.shadowTranslucency)
			{
				shadowTranslucency = lightCollider.shadowTranslucency;

				update = true;
			}	

			bool checkShapeSprite = shape.maskType == LightCollider2D.MaskType.SpritePhysicsShape || shape.shadowType == LightCollider2D.ShadowType.SpritePhysicsShape;
			bool checkMaskSprite = shape.maskType == LightCollider2D.MaskType.Sprite || shape.maskType == LightCollider2D.MaskType.BumpedSprite;

			if (checkShapeSprite || checkMaskSprite)
			{
				var spriteRenderer = shape.spriteShape.GetSpriteRenderer();
				if (spriteRenderer)
				{
					if (spriteRenderer.size != size)
					{
						size = spriteRenderer.size;
						
						update = true;
					}

					if (spriteRenderer.flipX != flipX || spriteRenderer.flipY != flipY)
					{
						flipX = spriteRenderer.flipX;
						flipY = spriteRenderer.flipY;

						shape.ResetWorld();

						update = true;
					}
					
					if (shape.spriteShape.GetOriginalSprite() != spriteRenderer.sprite)
					{
						shape.ResetLocal();

						update = true;
					}
				}
			}

			bool checkShapeMesh = shape.maskType == LightCollider2D.MaskType.MeshRenderer || shape.shadowType == LightCollider2D.ShadowType.MeshRenderer;

			if (checkShapeMesh)
			{
				var meshFilter = shape.meshShape.GetMeshFilter();
				if (meshFilter)
				{
					if (meshFilter.sharedMesh != shape.meshShape.Mesh)
					{
						shape.ResetLocal();
						
						update = true;
					}
				}
			}
		}
	}
}