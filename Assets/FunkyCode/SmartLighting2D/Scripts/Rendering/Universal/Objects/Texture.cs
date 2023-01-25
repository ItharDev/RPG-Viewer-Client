using UnityEngine;

namespace FunkyCode.Rendering.Universal
{
	public class Texture : Base
    {
        public static  class Quad
        {
            public static class Sprite
            {
                static public void DrawMultiPass(Vector2 pos, Vector2 size, Rect uv, float rot)
                {
                    rot = rot * Mathf.Deg2Rad + Mathf.PI;

                    float cos = Mathf.Cos(rot);
                    float sin = Mathf.Sin(rot);

                    float cosx = size.x * cos;
                    float sinx = size.x * sin;

                    float cosy = size.y * cos;
                    float siny = size.y * sin;

                    GL.Color(GLExtended.color);

                    GL.MultiTexCoord3 (0,uv.width, uv.height, 0);
                    GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);
                    
                    GL.MultiTexCoord3 (0,uv.x, uv.height, 0);
                    GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);

                    GL.MultiTexCoord3 (0,uv.x, uv.y, 0);
                    GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);

                    GL.MultiTexCoord3 (0,uv.width, uv.y, 0);
                    GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);
                } 

                static public void DrawPass(Vector2 pos, Vector2 size, Rect uv, float rot)
                {
                    rot = rot * Mathf.Deg2Rad + Mathf.PI;

                    float cos = Mathf.Cos(rot);
                    float sin = Mathf.Sin(rot);

                    float cosx = size.x * cos;
                    float sinx = size.x * sin;

                    float cosy = size.y * cos;
                    float siny = size.y * sin;

                    float uvX = uv.x;
                    float uvY = uv.y;
                    float uvWidth = uv.width;
                    float uvHeight = uv.height;

                    GL.Color(GLExtended.color);

                    GL.TexCoord3 (uvWidth, uvHeight, 0);
                    GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);
                    
                    GL.TexCoord3 (uvX, uvHeight, 0);
                    GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);

                    GL.TexCoord3 (uvX, uvY, 0);
                    GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);

                    GL.TexCoord3 (uvWidth, uvY, 0);
                    GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);
                } 

                static public void Draw(Vector2 pos, Vector2 size, Rect uv, float rot)
                {
                    rot = rot * Mathf.Deg2Rad - Mathf.PI;

                    float cos = Mathf.Cos(rot);
                    float sin = Mathf.Sin(rot);

                    float cosx = size.x * cos;
                    float sinx = size.x * sin;

                    float cosy = size.y * cos;
                    float siny = size.y * sin;

                    GL.Begin (GL.QUADS);

                    GL.Color(GLExtended.color);

                    GL.TexCoord3 (uv.width, uv.height, 0);
                    GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);
                    
                    GL.TexCoord3 (uv.x, uv.height, 0);
                    GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);


                    GL.TexCoord3 (uv.x, uv.y, 0);
                    GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);

                    GL.TexCoord3 (uv.width, uv.y, 0);
                    GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);

                    GL.End ();
                }
            }

            // ste uv (improve sprite matrix!)
            public static class STE
            {
                static private Vector2 v0, v1, v2, v3;
            
                static public void DrawPass(Vector2 pos, Vector2 size, Rect uv, float rot)
                {
                    float cos = Mathf.Cos(rot);
                    float sin = Mathf.Sin(rot);

                    float cosx = size.x * cos;
                    float sinx = size.x * sin;

                    float cosy = size.y * cos;
                    float siny = size.y * sin;

                    GL.TexCoord3 (uv.x, uv.y, 0);
                    GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);

                    GL.TexCoord3 (uv.x + uv.width, uv.y, 0);
                    GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);

                    GL.TexCoord3 (uv.x + uv.width, uv.y + uv.height, 0);
                    GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);

                    GL.TexCoord3 (uv.x, uv.y + uv.height, 0);
                    GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);
                }
            }

            static private Vector2 v0, v1, v2, v3;

            static public void Draw(Material material, Vector2 pos, Vector2 size, float rot, float z)
            {
                rot = rot * Mathf.Deg2Rad - Mathf.PI;

                float cos = Mathf.Cos(rot);
                float sin = Mathf.Sin(rot);

                float cosx = size.x * cos;
                float sinx = size.x * sin;

                float cosy = size.y * cos;
                float siny = size.y * sin;
   
                material.SetPass (0);       
                
                GL.Begin (GL.QUADS);
                
                GL.Color(GLExtended.color);

                GL.TexCoord3 (1, 1, 0); 
                GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, z);
            
                GL.TexCoord3 (0, 1, 0);
                GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, z);

                GL.TexCoord3 (0, 0, 0);
                GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, z);

                GL.TexCoord3 (1, 0, 0);
                GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, z);

                GL.End ();
            }

            static public void Draw(Vector2 pos, Vector2 size, float rot)
            {
                rot = rot * Mathf.Deg2Rad - Mathf.PI;

                float cos = Mathf.Cos(rot);
                float sin = Mathf.Sin(rot);

                float cosx = size.x * cos;
                float sinx = size.x * sin;

                float cosy = size.y * cos;
                float siny = size.y * sin;

                GL.Begin (GL.QUADS);

                GL.TexCoord3 (1, 1, 0);
                GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);
            
                GL.TexCoord3 (0, 1, 0);
                GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);
             
                GL.TexCoord3 (0, 0, 0);
                GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);
       
                GL.TexCoord3 (1, 0, 0);
                GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);

                GL.End ();
            }
        }
    }
}