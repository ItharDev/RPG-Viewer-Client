using System.Collections.Generic;
using UnityEngine;
using System;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Light.Shadow
{
    public static class Soft
	{
       	private static Pair2 pair = Pair2.Zero();
		private static EdgePass pass = new EdgePass();

        public static void Draw(List<Polygon2> polygons, float shadowTranslucency)
		{
			Vector2 position = ShadowEngine.lightOffset;
	
			for(int i = 0; i < polygons.Count; i++)
			{
				if (ShadowEngine.drawMode == ShadowEngine.DRAW_MODE_SOFT_CONVEX) {
					ShadowForObject(polygons[i], position, shadowTranslucency);
				} else {
					ShadowForVertex(polygons[i], position);
				}
			}
		}

		static private void ShadowForVertex(Polygon2 polygon, Vector2 position)
		{
			if (ShadowEngine.ignoreInside)
			{
				// change to sides of vertices?
				if (Math2D.PointInPoly(-position, polygon)) 
				{ 
					return;
				}
			}
				else if (ShadowEngine.dontdrawInside)
			{	
				if (Math2D.PointInPoly(-position, polygon))
				{ 
					ShadowEngine.continueDrawing = false;
					return;
				}
			}

			Vector2[] pointsList = polygon.points;
			int pointsCount = pointsList.Length;

			for(int x = 0; x < pointsCount; x++) {
				pair.A.x = pointsList[(x) % pointsCount].x + position.x;
				pair.A.y = pointsList[(x) % pointsCount].y + position.y;

				pair.B.x = pointsList[(x + 2) % pointsCount].x + position.x;
				pair.B.y = pointsList[(x + 2) % pointsCount].y + position.y;

				Pair2 edge_world = pair;

				Vector2 edgePosition;
				edgePosition.x = (float)(pair.A.x + pair.B.x) / 2;
				edgePosition.y = (float)(pair.A.y + pair.B.y) / 2;

				float edgeRotation = (float)Math.Atan2(pair.B.y - pair.A.y, pair.B.x - pair.A.x);

				float edgeSize = Vector2.Distance(pair.A, pair.B) / 2;

				pass.edgePosition = edgePosition;
				pass.edgeRotation = edgeRotation;
				pass.edgeSize = edgeSize;
				pass.coreSize = ShadowEngine.light.coreSize;

				pass.Generate();
				pass.SetVars();
				pass.Draw();
			}
		}

		static private void ShadowForObject(Polygon2 polygon, Vector2 position, float shadowTranslucency)
		{	
			if (ShadowEngine.ignoreInside) {
				// change to sides of vertices?
				if (Math2D.PointInPoly(-position, polygon))
				{ 
					return;
				}
			} 
				else if (ShadowEngine.dontdrawInside)
			{
				if (Math2D.PointInPoly(-position, polygon)) 
				{ 
					ShadowEngine.continueDrawing = false;
					return;
				}
			}

			Vector2[] pointsList = polygon.points;
			int pointsCount = pointsList.Length;

			Light2D light = ShadowEngine.light;

			SoftShadowSorter.Set(polygon, light);

			pair.A.x = SoftShadowSorter.minPoint.x + position.x;
			pair.A.y = SoftShadowSorter.minPoint.y + position.y;

			pair.B.x = SoftShadowSorter.maxPoint.x + position.x;
			pair.B.y = SoftShadowSorter.maxPoint.y + position.y;

			Pair2 edge_world = pair;

			Vector2 edgePosition;
			edgePosition.x = (float)(pair.A.x + pair.B.x) / 2;
			edgePosition.y = (float)(pair.A.y + pair.B.y) / 2;

			float edgeRotation = (float)Math.Atan2(pair.B.y - pair.A.y, pair.B.x - pair.A.x);

			float edgeSize = (float)Vector2.Distance(pair.A, pair.B) / 2;

			pass.edgePosition = edgePosition;
			pass.edgeRotation = edgeRotation;
			pass.edgeSize = edgeSize;
			pass.coreSize = light.coreSize;
			pass.shadowTranslucency = shadowTranslucency;

			pass.Generate();
			pass.SetVars();
			pass.Draw();

			pass.edgePosition = edgePosition;
			pass.edgeRotation = edgeRotation + Mathf.PI;
			pass.edgeSize = edgeSize;
			pass.coreSize = light.coreSize;
			pass.shadowTranslucency = shadowTranslucency;

			pass.Generate();
			pass.SetVars();
			pass.Draw();
		}
    }
}
