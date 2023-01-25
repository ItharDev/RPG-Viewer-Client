using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.LightShape
{		
	public class SpriteShape : Base
	{
		private Sprite originalSprite;
		private SpriteRenderer spriteRenderer;

		private VirtualSpriteRenderer virtualSpriteRenderer = new VirtualSpriteRenderer();

		public override int GetSortingLayer()
		{
			return(UnityEngine.SortingLayer.GetLayerValueFromID(GetSpriteRenderer().sortingLayerID));
		}

        public override int GetSortingOrder()
        {
			SpriteRenderer spriteRenderer = GetSpriteRenderer();

			if (spriteRenderer != null)
			{
				return(spriteRenderer.sortingOrder);
			}

            return(0);
        }

		public override List<Polygon2> GetPolygonsLocal()
		{
			if (LocalPolygons == null)
			{
				LocalPolygons = new List<Polygon2>();

				if (spriteRenderer == null)
				{
					Debug.LogWarning("Light Collider 2D: Cannot access sprite renderer (Sprite Shape Local)", transform.gameObject);
					return(LocalPolygons);
				}

				Vector2 v1, v2, v3, v4;

				if (spriteRenderer.drawMode == SpriteDrawMode.Tiled && spriteRenderer.tileMode == SpriteTileMode.Continuous)
				{
					float rot = transform.eulerAngles.z;
					Vector2 size = transform.localScale * spriteRenderer.size * 0.5f;
					Vector2 pos = Vector3.zero;

					rot = rot * Mathf.Deg2Rad + Mathf.PI;

					float rectAngle = Mathf.Atan2(size.y, size.x);
					float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

					v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
					v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
					v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
					v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
				}
					else
				{
					virtualSpriteRenderer.Set(spriteRenderer);

					Vector2 position = Vector3.zero;
					Vector2 scale = transform.localScale;
					float rotation = transform.eulerAngles.z;
		
					SpriteTransform spriteTransform = new SpriteTransform(virtualSpriteRenderer, position, scale, rotation);

					float rot = spriteTransform.rotation;
					Vector2 size = spriteTransform.scale;
					Vector2 pos = spriteTransform.position;

					rot = rot * Mathf.Deg2Rad + Mathf.PI;

					float rectAngle = Mathf.Atan2(size.y, size.x);
					float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

					v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
					v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
					v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
					v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
				}
		
				Polygon2 polygon = new Polygon2(4);

				polygon.points[0] = v1;
				polygon.points[1] = v2;
				polygon.points[2] = v3;
				polygon.points[3] = v4;

				LocalPolygons.Add(polygon);
			}

			return(LocalPolygons);
		}

		public override List<Polygon2> GetPolygonsWorld()
		{
			if (WorldPolygons == null)
			{
				Vector2 v1, v2, v3, v4;

				if (WorldCache == null)
				{
					WorldPolygons = new List<Polygon2>();

					if (spriteRenderer == null)
					{
						Debug.LogWarning("Light Collider 2D: Cannot access sprite renderer (Sprite Shape)", transform.gameObject);

						return(null);
					}

					if (spriteRenderer.drawMode == SpriteDrawMode.Tiled && spriteRenderer.tileMode == SpriteTileMode.Continuous)
					{
						float rot = transform.eulerAngles.z;
						Vector2 size = transform.lossyScale * spriteRenderer.size * 0.5f;
						Vector2 pos = transform.position;

						rot = rot * Mathf.Deg2Rad + Mathf.PI;

						float rectAngle = Mathf.Atan2(size.y, size.x);
						float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

						v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
						v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
						v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
						v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
					}
						else
					{
						virtualSpriteRenderer.Set(spriteRenderer);

						Vector2 position = transform.position;
						Vector2 scale = transform.lossyScale;
						float rotation = transform.eulerAngles.z;
			
						SpriteTransform spriteTransform = new SpriteTransform(virtualSpriteRenderer, position, scale, rotation);

						float rot = spriteTransform.rotation;
						Vector2 size = spriteTransform.scale;
						Vector2 pos = spriteTransform.position;

						rot = rot * Mathf.Deg2Rad + Mathf.PI;

						float rectAngle = Mathf.Atan2(size.y, size.x);
						float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

						v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
						v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
						v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
						v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
					
						WorldCache = WorldPolygons;
					}

					Polygon2 polygon = new Polygon2(4);

					polygon.points[0] = v1;
					polygon.points[1] = v2;
					polygon.points[2] = v3;
					polygon.points[3] = v4;

					WorldPolygons.Add(polygon);

				}
					else
				{
					WorldPolygons = WorldCache;

					if (spriteRenderer.drawMode == SpriteDrawMode.Tiled && spriteRenderer.tileMode == SpriteTileMode.Continuous) {
						
						Vector2 size = transform.lossyScale * spriteRenderer.size * 0.5f;
						Vector2 pos = transform.position;

						float rot = transform.eulerAngles.z;
						rot = rot * Mathf.Deg2Rad + Mathf.PI;

						float rectAngle = Mathf.Atan2(size.y, size.x);
						float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

						v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
						v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
						v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
						v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
					}
						else
					{
						virtualSpriteRenderer.Set(spriteRenderer);

						Vector2 position = transform.position;
						Vector2 scale = transform.lossyScale;
						float rotation = transform.eulerAngles.z;
			
						SpriteTransform spriteTransform = new SpriteTransform(virtualSpriteRenderer, position, scale, rotation);

						Vector2 size = spriteTransform.scale;
						Vector2 pos = spriteTransform.position;

						float rot = spriteTransform.rotation;
						rot = rot * Mathf.Deg2Rad + Mathf.PI;

						float rectAngle = Mathf.Atan2(size.y, size.x);
						float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

						v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
						v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
						v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
						v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
					}

					Polygon2 polygon = WorldPolygons[0];
						
					polygon.points[0] = v1;
					polygon.points[1] = v2;
					polygon.points[2] = v3;
					polygon.points[3] = v4;
				}	
			}

			return(WorldPolygons);
		}

		public override void ResetLocal()
		{
			base.ResetLocal();

			originalSprite = null;
		}

		public SpriteRenderer GetSpriteRenderer()
		{
			if (spriteRenderer != null)
			{
				return(spriteRenderer);
			}
			
			if (transform == null)
			{
				return(spriteRenderer);
			}

			if (spriteRenderer == null)
			{
				spriteRenderer = transform.GetComponent<SpriteRenderer>();
			}
			
			return(spriteRenderer);
		}

		public Sprite GetOriginalSprite()
		{
            if (originalSprite == null)
			{
                GetSpriteRenderer();

                if (spriteRenderer != null)
				{
                    originalSprite = spriteRenderer.sprite;
                }
            }
			return(originalSprite);
		}
	}
}