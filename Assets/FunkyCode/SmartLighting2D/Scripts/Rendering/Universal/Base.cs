using UnityEngine;

namespace FunkyCode.Rendering.Universal
{
	public class Base
	{
		public static Vector2[] meshUV = null;
		private static Mesh preRenderMesh = null;

        public static Mesh GetRenderMesh()
		{
			if (preRenderMesh)
				return preRenderMesh;

			var mesh = new Mesh();

			mesh.vertices = new Vector3[]{new Vector3(-1, -1), new Vector3(1, -1), new Vector3(1, 1), new Vector3(-1, 1)};
			mesh.triangles = new int[]{0, 1, 2, 2, 3, 0};
			mesh.uv = new Vector2[]{new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)};
			meshUV = mesh.uv;

			preRenderMesh = mesh;
	
			return preRenderMesh;
		}
	}
}