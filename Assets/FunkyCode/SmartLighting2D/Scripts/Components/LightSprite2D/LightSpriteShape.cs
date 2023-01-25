using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode
{
	[System.Serializable]
	public class LightSpriteTransform
	{
		public bool applyRotation = true;

		public Vector2 scale = new Vector2(1, 1);
		public float rotation = 0;
		public Vector2 position = new Vector2(0, 0);	
	}

	[System.Serializable]
	public class LightSpriteShape {
		private LightSpriteTransform lightSpriteTransform;
		private VirtualSpriteRenderer spriteRenderer;
		private Transform transform;
		


		public bool update = false;
		private Vector2 position = Vector2.zero;
		private float rotation = 0;
		private Vector2 scale = Vector2.one;
		private Sprite sprite;

		public void Set(VirtualSpriteRenderer spriteRenderer, Transform transform, LightSpriteTransform lightSpriteTransform) {
			this.spriteRenderer = spriteRenderer;
			this.lightSpriteTransform = lightSpriteTransform;
			this.transform = transform;
		}

		public void Update() {
			if (lightSpriteTransform == null) {
				return;
			}

			if (transform == null) {
				return;
			}
			
			Vector2 position2D = lightSpriteTransform.position;
			position2D += LightingPosition.GetPosition2D(transform.position);

			float rotation2D = transform.eulerAngles.z + lightSpriteTransform.rotation;

			Vector2 scale2D = lightSpriteTransform.scale;
			scale2D += LightingPosition.GetPosition2D(transform.lossyScale);

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

			if (scale != scale2D)
			{
				scale = scale2D;

				update = true;
			}

			if (update)
			{
				worldPolygon = null;

				update = false;
			}
		}

		public Rect GetWorldRect()
		{
			GetSpriteWorldPolygon();
			return(worldrect);
		}

		private Polygon2 worldPolygon = null;
		private Rect worldrect = new Rect();

		public Polygon2 GetSpriteWorldPolygon()
		{
			if (worldPolygon != null)
			{
				return(worldPolygon);
			}

			if (transform == null)
			{
				return(null);
			}

			Vector2 position = LightingPosition.GetPosition2D(transform.position);
			position += lightSpriteTransform.position;

			Vector2 scale = LightingPosition.GetPosition2D(transform.lossyScale);
			scale *= lightSpriteTransform.scale;

			float rotation = 0;

			if (lightSpriteTransform.applyRotation)
			{
				rotation += transform.eulerAngles.z + lightSpriteTransform.rotation;
			}

			SpriteTransform spriteTransform = new SpriteTransform(spriteRenderer, position, scale, rotation);

			float rot = spriteTransform.rotation;
			Vector2 size = spriteTransform.scale;
			Vector2 pos = spriteTransform.position;

			rot = rot * Mathf.Deg2Rad + Mathf.PI;

			float rectAngle = Mathf.Atan2(size.y, size.x);
			float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

			Vector2 v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
			Vector2 v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
			Vector2 v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
			Vector2 v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
		
			worldPolygon = GetPolygon();
			
			worldPolygon.points[0].x = v1.x;
			worldPolygon.points[0].y = v1.y;

			worldPolygon.points[1].x = v2.x;
			worldPolygon.points[1].y = v2.y;

			worldPolygon.points[2].x = v3.x;
			worldPolygon.points[2].y = v3.y;

			worldPolygon.points[3].x = v4.x;
			worldPolygon.points[3].y = v4.y;

			worldrect = worldPolygon.GetRect();

			return(worldPolygon);
		}

		private Polygon2 polygon = null;
		private Polygon2 GetPolygon() {
			if (polygon == null) {
				polygon = new Polygon2(4);
				polygon.points[0] = Vector2.zero;
				polygon.points[1] = Vector2.zero;
				polygon.points[2] = Vector2.zero;
				polygon.points[3] = Vector2.zero;
			}

			return(polygon);
		}
	}
}