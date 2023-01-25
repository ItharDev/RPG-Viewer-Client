using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.LightShape
{
	public abstract class Base
	{
		protected List<Polygon2> WorldPolygons = null;
		protected List<Polygon2> WorldCache = null;

		protected Vector2? WorldPoint = null;
		protected Rect WorldRect;
		protected Rect WorldDayRect;

		// iso vars
		protected Rect IsoWorldRect;

		protected List<Polygon2> LocalPolygons = null;
		protected List<Polygon2> LocalPolygonsCache = null;

		protected List<MeshObject> Meshes = null;
	
		protected Transform transform;

		public virtual int GetSortingOrder() => 0;
	
		public virtual int GetSortingLayer() => 0;

		public virtual List<MeshObject> GetMeshes() => null;
		public virtual List<Polygon2> GetPolygonsLocal() => null;
		public abstract List<Polygon2> GetPolygonsWorld();

		public void SetTransform(Transform transform)
		{
			this.transform = transform;
		}

		public virtual void ResetLocal()
		{
			Meshes = null;
			LocalPolygons = null;
			WorldPolygons = null;
			WorldCache = null;

			ResetWorld();
		}

		public virtual void ResetWorld()
		{
			WorldPolygons = null;
			WorldRect = new Rect();
			WorldDayRect = new Rect();
			IsoWorldRect = new Rect();
			WorldPoint = null;
		}

		public Rect GetWorldRect()
		{
			if (WorldRect.width < 0.01f)
				WorldRect = Polygon2Helper.GetRect(GetPolygonsWorld());

			return WorldRect;
		}

		public Rect GetDayRect(float shadowDistance)
		{
			if (WorldDayRect.width < 0.01f)
				WorldDayRect = Polygon2Helper.GetDayRect(GetPolygonsWorld(), shadowDistance);

			return WorldRect;
		}

		public Rect GetIsoWorldRect()
		{
			if (IsoWorldRect.width < 0.01f)
				IsoWorldRect = Polygon2Helper.GetIsoRect(GetPolygonsWorld());

			return IsoWorldRect;
		}
		
		public Vector2 GetPivotPoint_ShapeCenter()
		{
			if (WorldPoint == null) 
				WorldPoint = GetWorldRect().center;

			return WorldPoint.Value;
		}

		public Vector2 GetPivotPoint_TransformCenter()
		{
			if (WorldPoint == null)
				WorldPoint = transform.position;

			return WorldPoint.Value;
		}

		public Vector2 GetPivotPoint_LowestY()
		{
			if (WorldPoint == null)
			{
				var polys = GetPolygonsWorld();
				var lowestPoint = new Vector2(0, 999999);

				foreach(var poly in polys)
					foreach(var p in poly.points)
						if (p.y < lowestPoint.y)
							lowestPoint = p;
	
				WorldPoint = lowestPoint;
			}

			return WorldPoint.Value;
		}
	}
}