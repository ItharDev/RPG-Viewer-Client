using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.LightShape
{
	public class MeshRendererShape : Base
	{
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;
		private Mesh mesh;

		public Mesh Mesh => mesh;

		public override int GetSortingLayer()
		{			
			return UnityEngine.SortingLayer.GetLayerValueFromID(GetMeshRenderer().sortingLayerID);
		}

		public override int GetSortingOrder()
		{
            return GetMeshRenderer().sortingOrder;
        }

		public override void ResetLocal()
		{
			base.ResetLocal();

			mesh = null;
			Meshes = null;
		}

		public MeshFilter GetMeshFilter()
		{
			if (!meshFilter && transform)
			{
				meshFilter = transform.GetComponent<MeshFilter>();
			}

			return meshFilter;
		}

		public MeshRenderer GetMeshRenderer()
		{
			if (!meshRenderer && transform)
			{
				meshRenderer = transform.GetComponent<MeshRenderer>();
			}

			return meshRenderer;
		}

		public override List<MeshObject> GetMeshes()
		{
			if (Meshes == null)
			{
				var meshFilter = GetMeshFilter();
				if (meshFilter)
				{
					mesh = meshFilter.sharedMesh;
					if (!mesh.isReadable)
					{
						Debug.LogError("SL2D: the mesh you are using is not readable (vert "+mesh.vertices.Length+", tris "+mesh.triangles.Length+", uv " + mesh.uv.Length + ")", transform.gameObject);
					}
				
					if (mesh)
					{
						Meshes = new List<MeshObject>();

						var meshObject = MeshObject.Get(mesh);
						if (meshObject != null)
						{
							Meshes.Add(meshObject);
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

			var meshes = GetMeshes();
			if (meshes == null)
			{
				WorldPolygons = new List<Polygon2>();

				return(WorldPolygons);
			}

			if (meshes.Count < 1)
			{
				WorldPolygons = new List<Polygon2>();

				Debug.LogError("SL2D: no meshes found", transform.gameObject);
				return(WorldPolygons);
			}

			var meshObject = meshes[0];
			if (meshObject == null)
			{
				WorldPolygons = new List<Polygon2>();

				return WorldPolygons;
			}

			Vector3 vecA, vecB, vecC;
			Polygon2 poly;

			if (WorldCache == null)
			{
				WorldPolygons = new List<Polygon2>();

				for (int i = 0; i < meshObject.triangles.GetLength (0); i = i + 3)
				{
					vecA = transform.TransformPoint(meshObject.vertices [meshObject.triangles [i]]);
					vecB = transform.TransformPoint(meshObject.vertices [meshObject.triangles [i + 1]]);
					vecC = transform.TransformPoint(meshObject.vertices [meshObject.triangles [i + 2]]);

					poly = new Polygon2(3);
					poly.points[0] = vecA;
					poly.points[1] = vecB;
					poly.points[2] = vecC;

					WorldPolygons.Add(poly);
				}

				WorldCache = WorldPolygons;
			}
			else
			{
				int count = 0;

				WorldPolygons = WorldCache;

				for (int i = 0; i < meshObject.triangles.GetLength (0); i = i + 3)
				{
					vecA = transform.TransformPoint(meshObject.vertices [meshObject.triangles [i]]);
					vecB = transform.TransformPoint(meshObject.vertices [meshObject.triangles [i + 1]]);
					vecC = transform.TransformPoint(meshObject.vertices [meshObject.triangles [i + 2]]);

					poly = WorldPolygons[count];
					poly.points[0].x = vecA.x;
					poly.points[0].y = vecA.y;
					poly.points[1].x = vecB.x;
					poly.points[1].y = vecB.y;
					poly.points[2].x = vecC.x;
					poly.points[2].y = vecC.y;

					count += 1;
				}
			}
		
			return WorldPolygons;
		}
    }
}