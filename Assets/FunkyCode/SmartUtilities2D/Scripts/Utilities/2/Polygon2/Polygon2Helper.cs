using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode.Utilities
{
	public static class Polygon2Helper
	{
		static public Pair2 GetAxis(Polygon2 polygon, float rotation)
		{	
			Pair2 pair = new Pair2(Vector2.zero, Vector2.zero);

			if (polygon == null)
			{
				return(pair);
			}

			float minX = 100000;
			float maxX = -100000;

			int pointsCount = polygon.points.Length;

			Vector2 center = polygon.GetRect().center;

			for(int i = 0; i < pointsCount; i++) {
				
				Vector2 id = polygon.points[i];

				Vector2 tid = id - center;

				float angle2 = Mathf.Atan2(tid.y, tid.x) + rotation + Mathf.PI / 2;
				float dist2 = Mathf.Sqrt(tid.x * tid.x + tid.y * tid.y);

				tid.x = Mathf.Cos(angle2) * dist2;
				tid.y = Mathf.Sin(angle2) * dist2;

				if (minX > tid.x)
				{
					minX = tid.x;

					pair.A = id;
				}

				if (maxX < tid.x)
				{
					maxX = tid.x;

					pair.B = id;
				}
			}

			return(pair);
		}

		static public Rect GetRect(List<Polygon2> polygons)
		{	
			Rect rect = new Rect();

			if (polygons == null)
			{
				return(rect);
			}

			if (polygons.Count > 0) 
			{
				float minX = 100000;
				float minY = 100000;
				float maxX = -100000;
				float maxY = -100000;

				for(int pid = 0; pid < polygons.Count; pid++)
				{
					Polygon2 poly = polygons[pid];

					int pointsCount = poly.points.Length;

					for(int i = 0; i < pointsCount; i++)
					{
						Vector2 id = poly.points[i];
		
						minX = (id.x < minX) ? id.x : minX;
						maxX = (id.x > maxX) ? id.x : maxX;

						minY = (id.y < minY) ? id.y : minY;
						maxY = (id.y > maxY) ? id.y : maxY;
					}
				}
			
				rect.x = minX;
				rect.y = minY;
				rect.width = maxX - minX;
				rect.height = maxY - minY;
			}

			return(rect);
		}

		static public Rect GetDayRect(List<Polygon2> polygons, float height) {	
			Rect rect = new Rect();

			if (polygons == null) {
				return(rect);
			}

			if (polygons.Count > 0) {
				float minX = 100000;
				float minY = 100000;
				float maxX = -100000;
				float maxY = -100000;

				float direction = -Lighting2D.DayLightingSettings.direction * Mathf.Deg2Rad;
				float shadowDistance = height * Lighting2D.DayLightingSettings.height;

				foreach(Polygon2 poly in polygons) {

					int pointsCount = poly.points.Length;

					for(int i = 0; i < pointsCount; i++) {
						
						Vector2 id = poly.points[i];
		
						minX = Mathf.Min(minX, id.x);
						minY = Mathf.Min(minY, id.y);
						maxX = Mathf.Max(maxX, id.x);
						maxY = Mathf.Max(maxY, id.y);

						float x = Mathf.Cos(direction) * shadowDistance;
						float y = Mathf.Sin(direction) * shadowDistance;


						minX = Mathf.Min(minX, id.x + x);
						minY = Mathf.Min(minY, id.y + y);
						maxX = Mathf.Max(maxX, id.x + x);
						maxY = Mathf.Max(maxY, id.y + y);
					}

				}
			
				rect.x = minX;
				rect.y = minY;
				rect.width = maxX - minX;
				rect.height = maxY - minY;
			}

			return(rect);
		}

		static public Rect GetIsoRect(List<Polygon2> polygons) {	
			Rect rect = new Rect();

			if (polygons == null) {
				return(rect);
			}

			if (polygons.Count > 0) {
				float minX = 100000;
				float minY = 100000;
				float maxX = -100000;
				float maxY = -100000;

				foreach(Polygon2 poly in polygons) {

					int pointsCount = poly.points.Length;

					for(int i = 0; i < pointsCount; i++) {
						
						Vector2 id = poly.points[i];

						float x = id.y + id.x / 2;
						float y = id.y - id.x / 2;
		
						minX = Mathf.Min(minX, x);
						minY = Mathf.Min(minY, y);
						maxX = Mathf.Max(maxX, x);
						maxY = Mathf.Max(maxY, y);
					}

				}
			
				rect.x = minX;
				rect.y = minY;
				rect.width = maxX - minX;
				rect.height = maxY - minY;
			}

			return(rect);
		}
		
	}
}