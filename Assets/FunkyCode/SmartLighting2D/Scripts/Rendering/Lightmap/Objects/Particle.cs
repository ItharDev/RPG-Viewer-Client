using UnityEngine;

namespace FunkyCode.Rendering.Lightmap
{
	public static class Particle
    {
        static public void DrawPass(Vector2 pos, Vector2 size, float angle)
        {
            angle = angle * Mathf.Deg2Rad + Mathf.PI;

            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            var cosx = size.x * cos;
            var sinx = size.x * sin;

            var cosy = size.y * cos;
            var siny = size.y * sin;

            GL.TexCoord3 (1, 1, 0);
            GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);

            GL.TexCoord3 (0, 1, 0);
            GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);

            GL.TexCoord3 (0, 0, 0);
            GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);

            GL.TexCoord3 (1, 0, 0);
            GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);
		}
	}
}