using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.LightShape
{
    public class SpritePhysicsShape : Base
	{
        private Sprite sprite;

        public SpriteExtension.PhysicsShape physicsShape = null;

        private SpriteRenderer spriteRenderer;

        override public void ResetLocal()
		{
			LocalPolygonsCache = LocalPolygons;
			
            base.ResetLocal();
            
            physicsShape = null;

			sprite = null;
        }

        public Sprite GetOriginalSprite()
		{
            if (sprite == null)
			{
                GetSpriteRenderer();

                if (spriteRenderer != null)
				{
                    sprite = spriteRenderer.sprite;
                }
            }

			return(sprite);
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

        public SpriteExtension.PhysicsShape GetPhysicsShape()
		{
			if (physicsShape == null)
			{
                Sprite sprite = GetOriginalSprite();

                if (sprite != null)
				{
                    physicsShape = SpriteExtension.PhysicsShapeManager.RequestCustomShape(sprite);
                }
			}

			return(physicsShape);
		}

		public override List<MeshObject> GetMeshes()
		{
			if (Meshes == null)
			{
				List<Polygon2> polygons = GetPolygonsLocal();

				if (polygons.Count > 0)
				{
					Meshes = new List<MeshObject>();

					foreach(Polygon2 poly in polygons)
					{
						if (poly.points.Length < 3)
						{
							continue;
						}

						Mesh mesh = PolygonTriangulator2.Triangulate (poly, Vector2.zero, Vector2.zero, PolygonTriangulator2.Triangulation.Advanced);
						
						if (mesh)
						{
							MeshObject meshObject = MeshObject.Get(mesh);

							if (meshObject != null)
							{
								Meshes.Add(meshObject);
							}
						}
					}
				}
			}

			return(Meshes);
		}

		public override List<Polygon2> GetPolygonsWorld()
		{
			if (WorldPolygons != null)
			{
				return(WorldPolygons);
			}

			Vector2 scale = new Vector2();

			List<Polygon2> localPolygons = GetPolygonsLocal();
		
			if (WorldCache != null)
			{
				WorldPolygons = WorldCache;

				Polygon2 poly;
				Polygon2 wPoly;

				SpriteRenderer spriteRenderer = GetSpriteRenderer();

				for(int i = 0; i < localPolygons.Count; i++)
				{
					poly = localPolygons[i];
					wPoly = WorldPolygons[i];

					bool invert = false;
					
					for(int p = 0; p < poly.points.Length; p++)
					{
						wPoly.points[p] = poly.points[p];
					}

					if (spriteRenderer != null)
					{
						if (spriteRenderer.flipX || spriteRenderer.flipY)
						{
							scale.x = 1;
							scale.y = 1;

							if (spriteRenderer.flipX)
							{
								scale.x = -1;

								invert = !invert;
							}

							if (spriteRenderer.flipY)
							{
								scale.y = -1;

								invert = !invert;
							}
						
							wPoly.ToScaleSelf(scale);
						}
					}

					wPoly.ToWorldSpaceSelfUNIVERSAL(transform);

					if (invert)
					{
						wPoly.Normalize();
					}
				}
			}
				else
			{
				Polygon2 polygon;

				WorldPolygons = new List<Polygon2>();

				SpriteRenderer spriteRenderer = GetSpriteRenderer();

				foreach(Polygon2 poly in localPolygons)
				{
					polygon = poly.Copy();

					bool invert = false;

					if (spriteRenderer != null)
					{
						if (spriteRenderer.flipX || spriteRenderer.flipY)
						{
							scale.x = 1;
							scale.y = 1;

							if (spriteRenderer.flipX)
							{
								scale.x = -1;

								invert = !invert;
							}

							if (spriteRenderer.flipY)
							{
								scale.y = -1;

								invert = !invert;
							}

							polygon.ToScaleSelf(scale);
						}
					}
					
					polygon.ToWorldSpaceSelfUNIVERSAL(transform);

					if (invert)
					{
						polygon.Normalize();
					}

					WorldPolygons.Add(polygon);
				}

				WorldCache = WorldPolygons;
			}

			return(WorldPolygons);
		}

		public override List<Polygon2> GetPolygonsLocal()
		{
			if (LocalPolygons != null)
			{
				return(LocalPolygons);
			}
	
			physicsShape = GetPhysicsShape();
		
			if (physicsShape != null)
			{
				LocalPolygons = physicsShape.Get();
			}

			if (LocalPolygons == null)
			{
				LocalPolygons = new List<Polygon2>();
			}

			return(LocalPolygons);
		}
    }
}