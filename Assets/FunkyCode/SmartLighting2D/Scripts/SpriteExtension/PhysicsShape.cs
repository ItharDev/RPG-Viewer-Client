using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.SpriteExtension
{		
	public class PhysicsShape
	{
		private List<Polygon2> polygons = null; 

		private MeshObject shapeMeshObject = null;
		private UnityEngine.Sprite sprite;

		public PhysicsShape(UnityEngine.Sprite sprite)
		{
			this.sprite = sprite;
		}

		public List<Polygon2> Get()
		{
			if (polygons == null) {
				Generate();
			}

			return(polygons);
		}

		private void Generate()
		{
			polygons = new List<Polygon2>();

			int count = sprite.GetPhysicsShapeCount();

			List<Vector2> points;
			Polygon2 newPolygon;

			for(int i = 0; i < count; i++) {
				points = new List<Vector2>();
				sprite.GetPhysicsShape(i, points);
				
				newPolygon = new Polygon2(points.ToArray());
				newPolygon.Normalize();

				polygons.Add(newPolygon);
			}
		}

		public Sprite GetSprite()
		{
			return(sprite);
		}

		public MeshObject GetMesh()
		{
			if (shapeMeshObject == null)
			{
				if (polygons.Count > 0)
				{
					shapeMeshObject = MeshObject.Get(polygons[0].CreateMesh(Vector2.zero, Vector2.zero));
				}
			}
			
			return(shapeMeshObject);
		}
	}
}
