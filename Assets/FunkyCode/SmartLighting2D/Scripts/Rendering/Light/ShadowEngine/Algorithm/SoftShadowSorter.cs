using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode
{
	public static class SoftShadowSorter
	{
		public static Vector2 center;

		public static Vector2 minPoint;
		public static Vector2 maxPoint;

		public static void Set(Polygon2 polygon, Light2D light)
		{
			Vector2 lightPosition = -light.transform2D.position;

			center.x = 0;
			center.y = 0;

			int pointsCount = polygon.points.Length;

			// polygon center could be optimized

			for(int i = 0; i < pointsCount; i++)
			{
				Vector2 p = polygon.points[i];
	
				center.x += p.x + lightPosition.x;
				center.y += p.y + lightPosition.y;
			}

			center.x /= pointsCount;
			center.y /= pointsCount;
			
			float centerDirection = Mathf.Atan2(center.x, center.y) * Mathf.Rad2Deg;

			centerDirection = (centerDirection + 720) % 360 + 180;
	
			float min = 10000;
			float max = -10000;
		
			for(int id = 0; id < polygon.points.Length; id++)
			{
				Vector2 p = polygon.points[id];

				float dir = Mathf.Atan2(p.x + lightPosition.x, p.y + lightPosition.y) * Mathf.Rad2Deg;

				dir = (dir + 720 - centerDirection) % 360;
				
				float direction = dir;

				if (direction < min)
				{
					min = direction;
					minPoint = p;
				}

				if (direction > max)
				{
					max = direction;
					maxPoint = p;
				}
			}
		}
	}
}