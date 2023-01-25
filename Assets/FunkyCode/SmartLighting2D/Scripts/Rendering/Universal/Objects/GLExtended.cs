using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FunkyCode
{
    public class GLExtended
    {
        public static Color color = Color.white;

        public static void ResetColor()
        {
            color = Color.white;
        }

        public static void DrawMeshPass(MeshObject mesh, Vector3 position, Vector2 scale, float rotation)
        {
            Vector2 v0, v1, v2;
            Vector3 p0, p1, p2;
            int t0, t1, t2;
            Vector2 uv0, uv1, uv2;

            bool useUV = mesh.uv.Length > 0;

            float angle = rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            int length = mesh.triangles.Length;
            
            GL.Color(color);

            for (int i = 0; i < length; i += 3)
            {
                t0 = mesh.triangles[i + 0];
                t1 = mesh.triangles[i + 1];
                t2 = mesh.triangles[i + 2];

                p0 = mesh.vertices[t0]; 
                p1 = mesh.vertices[t1];
                p2 = mesh.vertices[t2];

                p0.x *= scale.x;
                p0.y *= scale.y;

                p1.x *= scale.x;
                p1.y *= scale.y;

                p2.x *= scale.x;
                p2.y *= scale.y;

                v0.x = p0.x * cos - p0.y * sin + position.x;
                v0.y = p0.x * sin + p0.y * cos + position.y;

                v1.x = p1.x * cos - p1.y * sin + position.x;
                v1.y = p1.x * sin + p1.y * cos + position.y;

                v2.x = p2.x * cos - p2.y * sin + position.x;
                v2.y = p2.x * sin + p2.y * cos + position.y;

                if (useUV)
                {
                    uv0 = mesh.uv[t0];
                    uv1 = mesh.uv[t1];
                    uv2 = mesh.uv[t2];

                    GL.TexCoord3(uv0.x, uv0.y, 0);
                    GL.Vertex3(v0.x, v0.y, 0);

                    GL.TexCoord3(uv1.x, uv1.y, 0);
                    GL.Vertex3(v1.x, v1.y, 0);

                    GL.TexCoord3(uv2.x, uv2.y, 0);
                    GL.Vertex3(v2.x, v2.y, 0);
                    
                }
                    else
                {
                    GL.Vertex3(v2.x, v2.y, 0);
                    GL.Vertex3(v1.x, v1.y, 0);
                    GL.Vertex3(v0.x, v0.y, 0);
                }
            }
        }

         public static void DrawMeshPass(MeshObject mesh)
         {
            Vector3 p0, p1, p2;
            int t0, t1, t2;

            int length = mesh.triangles.Length;

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i < length; i += 3)
            {
                t0 = mesh.triangles[i];
                t1 = mesh.triangles[i + 1];
                t2 = mesh.triangles[i + 2];

                p0 = mesh.vertices[t0]; 
                p1 = mesh.vertices[t1];
                p2 = mesh.vertices[t2];

                GL.Vertex3(p2.x, p2.y, 0);
                GL.Vertex3(p1.x, p1.y, 0);
                GL.Vertex3(p0.x, p0.y, 0);
            }

            GL.End();
        }

        public static void DrawMeshPass(List<MeshObject> meshes, Vector3 position, Vector2 scale, float rotation)
        {
            int length = meshes.Count;

            for (int i = 0; i < length; i += 3)
            {
                MeshObject mesh = meshes[i];

                DrawMeshPass(mesh, position, scale, rotation);
            }
        }

        public static void DrawMesh(MeshObject mesh, Vector3 position, Vector2 scale, float rotation)
        {
            GL.Begin(GL.TRIANGLES);

            DrawMeshPass(mesh, position, scale, rotation);

            GL.End();
        }

        public static void DrawMesh(List<MeshObject> meshes, Vector3 position, Vector2 scale, float rotation)
        {
            GL.Begin(GL.TRIANGLES);

            int length = meshes.Count;

            for (int i = 0; i < length; i += 3)
            {
                MeshObject mesh = meshes[i];
                
                DrawMeshPass(mesh, position, scale, rotation);
            }

            GL.End();
        }
    }
}