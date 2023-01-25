using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering
{
    public class LightBuffer
    {
        static public void Render(Light2D light)
        {
			float size = light.size;

			GL.PushMatrix();

            if (light.IsPixelPerfect())
            {
                var camera = Camera.main;

                float cameraRotation = LightingPosition.GetCameraRotation(camera);
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, cameraRotation), Vector3.one);

                float sizeY = camera.orthographicSize;
                float sizeX = sizeY * ( (float)camera.pixelWidth / camera.pixelHeight);
                
                GL.LoadPixelMatrix(-sizeX, sizeX, -sizeY, sizeY);
            }
            else
            {
                GL.LoadPixelMatrix(-size, size, -size, size);
            }
			
			Rendering.Light.Main.Draw(light);

			GL.PopMatrix();

            light.drawingEnabled = Rendering.Light.ShadowEngine.continueDrawing;
		}

        static public void RenderTranslucency(Light2D light)
        {
			float size = light.size;

			GL.PushMatrix();

            if (light.IsPixelPerfect())
            {
                var camera = Camera.main;

                float cameraRotation = LightingPosition.GetCameraRotation(camera);
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, cameraRotation), Vector3.one);

                float sizeY = camera.orthographicSize;
                float sizeX = sizeY * ((float)camera.pixelWidth / camera.pixelHeight );
                
                GL.LoadPixelMatrix( -sizeX, sizeX, -sizeY, sizeY );
            }
            else
            {
                GL.LoadPixelMatrix( -size, size, -size, size ); 
            }
			
			Rendering.Light.Main.DrawTranslucency(light);

			GL.PopMatrix();

            light.drawingTranslucencyEnabled = Rendering.Light.ShadowEngine.continueDrawing;
		}
        
        static public void RenderFreeForm(Light2D light)
        {
            LightFreeForm freeForm = light.freeForm;

            Vector2[] points = freeForm.polygon.points;

            int pointsCount = points.Length;
            if (pointsCount < 3)
                return;

            float size = light.size;
            GL.LoadPixelMatrix( -size, size, -size, size ); 

            GL.PushMatrix();

            MeshObject meshObject = MeshObject.Get(freeForm.polygon.CreateMesh(Vector2.zero, Vector2.zero));

            Material material = Lighting2D.Materials.GetAdditive();
            material.mainTexture = null;

            GLExtended.color = Color.white;

            material.SetPass(0);

            GLExtended.DrawMeshPass(meshObject);

            // make new free form edge material (min/max - not additive
            // load new edge texture to the material
            // draw image line function

            if (light.freeFormFalloff > 0)
            {
                float edgeSize = light.freeFormFalloff;

                material = Lighting2D.Materials.lights.GetFreeFormEdgeLight();
                material.mainTexture = null;
                material.SetFloat("_Strength", light.freeFormFalloffStrength);

                material.SetPass(0);

                GL.Begin(GL.QUADS);

                for(int i = 0; i < pointsCount; i++)
                {
                    Vector2 point = points[i];
                    Vector2 nextPoint = points[(i + 1) % pointsCount];

                    float direction = point.Atan2(nextPoint);

                    Vector2 p3 = nextPoint;
                    Vector2 p4 = point;
            
                    Vector2 p1 = point;
                    p1.x += Mathf.Cos(direction - Mathf.PI / 2) * edgeSize;
                    p1.y += Mathf.Sin(direction - Mathf.PI / 2) * edgeSize;

                    Vector2 p2 = nextPoint;
                    p2.x += Mathf.Cos(direction - Mathf.PI / 2) * edgeSize;
                    p2.y += Mathf.Sin(direction - Mathf.PI / 2) * edgeSize;
                    
                    GL.Color(new Color(0, 0, 0, 0));
                    GL.Vertex3(p1.x, p1.y, 0);
                    GL.Vertex3(p2.x, p2.y, 0);

                    GL.Color(new Color(1, 1, 0, 0));
                    GL.Vertex3(p3.x, p3.y, 0);
                    GL.Vertex3(p4.x, p4.y, 0);

                    GL.Color(new Color(0, 0, 0, 1));
                    GL.Vertex3(point.x - edgeSize, point.y - edgeSize, 0);

                    GL.Color(new Color(1, 0, 0, 1));
                    GL.Vertex3(point.x + edgeSize, point.y - edgeSize, 0);

                    GL.Color(new Color(1, 1, 0, 1));
                    GL.Vertex3(point.x + edgeSize, point.y + edgeSize, 0);

                    GL.Color(new Color(0, 1, 0, 1));
                    GL.Vertex3(point.x - edgeSize, point.y + edgeSize, 0);
                }

                GL.End();
            }
    
			GL.PopMatrix();
		}

        static public void UpdateName(LightBuffer2D buffer)
        {
            var freeString = string.Empty;

            if (buffer.Free)
                freeString = "free";
            else
                freeString = "taken";

            if (buffer.renderTexture != null)
                buffer.name = $"Buffer (Id: {LightBuffer2D.List.IndexOf(buffer) + 1}, Size: {buffer.renderTexture.width}, {freeString})";
            else
                buffer.name = "Buffer (Id: {LightBuffer2D.List.IndexOf(buffer) + 1}, No Texture, {freeString})";
        }

        static public void InitializeRenderTexture(LightBuffer2D buffer, Vector2Int textureSize)
        {
            var format = RenderTextureFormat.R8;
            if (!SystemInfo.SupportsRenderTextureFormat(format))
            {
                format = RenderTextureFormat.Default;
            }

            buffer.renderTexture = new LightTexture(textureSize.x, textureSize.y, 0, format);
            buffer.renderTexture.renderTexture.filterMode = Lighting2D.Profile.qualitySettings.lightFilterMode;
 
            UpdateName(buffer);
        }
            
        static public void InitializeFreeFormTexture(LightBuffer2D buffer, Vector2Int textureSize)
        {
            var format = RenderTextureFormat.R8;
            if (!SystemInfo.SupportsRenderTextureFormat(format))
            {
                format = RenderTextureFormat.Default;
            }

            buffer.freeFormTexture = new LightTexture(textureSize.x, textureSize.y, 0, format);
            buffer.freeFormTexture.renderTexture.filterMode = Lighting2D.Profile.qualitySettings.lightFilterMode;
 
            UpdateName(buffer);
        }

        static public void InitializeTranslucencyTexture(LightBuffer2D buffer, Vector2Int textureSize)
        {
            var format = RenderTextureFormat.R8;
            if (!SystemInfo.SupportsRenderTextureFormat(format))
            {
                format = RenderTextureFormat.Default;
            }

            buffer.translucencyTexture = new LightTexture(textureSize.x, textureSize.y, 0, format);
            buffer.translucencyTexture.renderTexture.filterMode = Lighting2D.Profile.qualitySettings.lightFilterMode;

            buffer.translucencyTextureBlur = new LightTexture(textureSize.x, textureSize.y, 0, format);
            buffer.translucencyTextureBlur.renderTexture.filterMode = Lighting2D.Profile.qualitySettings.lightFilterMode;

            UpdateName(buffer);
        }
    }
}