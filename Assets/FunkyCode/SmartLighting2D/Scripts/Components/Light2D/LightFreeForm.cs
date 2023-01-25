using UnityEngine;
using FunkyCode.Utilities;
using System.Collections.Generic;

namespace FunkyCode
{
	[System.Serializable]
	public class FreeFormPoints
	{
		[SerializeField]
		public List<Vector2> points = new List<Vector2>();

		public FreeFormPoints()
		{
			float size = 1;
			float i = 0;
			float pointsCount = 3;

			while (i < 360)
			{
				points.Add (new Vector2(Mathf.Cos (i * Mathf.Deg2Rad) * size, Mathf.Sin (i * Mathf.Deg2Rad) * size));
				
				i += 360f / (float)pointsCount;
			}
		}
	}

	public class LightFreeForm
	{		
		public Polygon2 polygon = new Polygon2(1);

		public Rect worldRect = new Rect();

		private bool update = true;
		
		public bool UpdateNeeded = false;

		public void ForceUpdate()
		{
			update = true;
		}

		// only if something changed (UI / API)

		public void Update(Light2D source)
		{
			if (!update)
			{
				return;
			}

			update = false;

			bool changeUpdate = false;

			if (source.freeFormPoints.points.Count != polygon.points.Length)
			{
				System.Array.Resize(ref polygon.points, source.freeFormPoints.points.Count);
			
				changeUpdate = true;
			}

			float minSize = 0;

			for(int i = 0; i < polygon.points.Length; i++)
			{
				Vector2 point = polygon.points[i];

				Vector2 cPoint = source.freeFormPoints.points[i];

				minSize = Mathf.Max(minSize, cPoint.magnitude + source.freeFormFalloff);

				if (point != cPoint)
				{
					changeUpdate = true;

					/// ????????
					/// Debug.Log("do2 + " + point + ">"+ cPoint); 

					polygon.points[i] = cPoint;
				}
			}

			if (minSize < source.size)
			{
				source.size = minSize;
			}

			if (minSize > source.size)
			{
				source.size = minSize;
			}
		
			if (changeUpdate)
			{
				UpdateNeeded = true;

				worldRect = polygon.GetRect();
			}
		}
	}
}