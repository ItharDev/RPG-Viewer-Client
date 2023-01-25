using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode.Rendering
{
    public class LightMainBuffer
    {
        public class Check
        {
            static public void RenderTexture(LightMainBuffer2D buffer)
            {
                Vector2Int screen = GetScreenResolution(buffer);

                if (screen.x <= 0 && screen.y <= 0)
                {
                    return;
                }

                Camera camera = buffer.cameraSettings.GetCamera();

                if (buffer.renderTexture == null || screen.x == buffer.renderTexture.width && screen.y == buffer.renderTexture.height)
                {
                    return;
                }

                switch(camera.cameraType)
                {
                    case CameraType.Game:

                        Rendering.LightMainBuffer.InitializeRenderTexture(buffer);
                    
                    break;

                    case CameraType.SceneView:

                        // scene view pixel rect is constantly changing ( unity bug? )
                        
                        int differenceX = Mathf.Abs(screen.x - buffer.renderTexture.width);
                        int differenceY = Mathf.Abs(screen.y - buffer.renderTexture.height);
                        
                        if (differenceX > 5 || differenceY > 5)
                        {
                            Rendering.LightMainBuffer.InitializeRenderTexture(buffer);
                        }
                    
                    break;
                }
            }

            static public bool CameraSettings(LightMainBuffer2D buffer)
            {
                LightingManager2D manager = LightingManager2D.Get();

                int settingsID = buffer.cameraSettings.id;

                if (settingsID >= manager.cameras.Length)
                {
                    return(false);
                }

                CameraSettings cameraSetting = manager.cameras.Get(settingsID);

                int bufferId = buffer.cameraLightmap.id;

                if (bufferId >= cameraSetting.Lightmaps.Length)
                {
                   return(false);
                }

                CameraLightmap cameraLightmap = cameraSetting.GetLightmap(bufferId);

                if (cameraLightmap.presetId != buffer.cameraLightmap.presetId)
                {
                    return(false);
                }

                switch(buffer.cameraLightmap.sceneView)
                {
                    case CameraLightmap.SceneView.Enabled:

                        if (cameraLightmap.sceneView == CameraLightmap.SceneView.Disabled)
                        {
                            return(false);
                        } 

                    break;

                    case CameraLightmap.SceneView.Disabled:

                        if (cameraSetting.cameraType != buffer.cameraSettings.cameraType)
                        {
                            return(false);
                        }

                    break;
                }

                return(true);
            }
        }

        public static void Update(LightMainBuffer2D buffer)
        {
            LightmapPreset lightmapPreset = buffer.GetLightmapPreset();

            if (lightmapPreset == null)
            {
                buffer.DestroySelf();
                return;
            }

            if (!Rendering.LightMainBuffer.Check.CameraSettings(buffer))
            {
                buffer.DestroySelf();
                return;
            }
            
            Camera camera = buffer.cameraSettings.GetCamera();

            if (camera == null)
            {
                return;
            }

            Rendering.LightMainBuffer.Check.RenderTexture(buffer);
        }

        public static void DrawPost(LightMainBuffer2D buffer)
        {			
			if (buffer.cameraLightmap.overlay != CameraLightmap.Overlay.Enabled)
            {
				return;
			}

            if (Lighting2D.RenderingMode != RenderingMode.OnPostRender)
            {
				return;
			}

			LightingRender2D.PostRender(buffer);
        }

        public static void DrawOn(LightMainBuffer2D buffer)
        {
			if (buffer.cameraLightmap.overlay != CameraLightmap.Overlay.Enabled)
            {
				return;
			}
                
            switch(Lighting2D.RenderingMode)
            {
                case RenderingMode.OnRender:

                    LightingRender2D.OnRender(buffer);

                break;

                case RenderingMode.OnPreRender:

                    LightingRender2D.PreRender(buffer);

                break;
            }
        }

        public static void Render(LightMainBuffer2D buffer)
        {
            Camera camera = buffer.cameraSettings.GetCamera();
            
            if (camera == null)
            {
                return;
            }

            float cameraRotation = LightingPosition.GetCameraRotation(camera);
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, cameraRotation), Vector3.one);

            float sizeY = camera.orthographicSize;
            float sizeX = sizeY * ( (float)camera.pixelWidth / camera.pixelHeight );

            LightmapPreset lightmapPreset = buffer.GetLightmapPreset();

            switch(lightmapPreset.type)
            {
                case LightmapPreset.Type.RGB24:
                case LightmapPreset.Type.R8:
                case LightmapPreset.Type.RHalf:

                    // clear darkness color

                    GL.Clear(false, true, Rendering.Lightmap.Main.ClearColor(camera, lightmapPreset)); 
            
                    GL.LoadPixelMatrix( -sizeX, sizeX, -sizeY, sizeY );
                    GL.MultMatrix(matrix);

                    GL.PushMatrix();
                    
                    Rendering.Day.Main.Draw(camera, lightmapPreset);
                
                    Rendering.Lightmap.Main.Draw(camera, lightmapPreset);

                    GL.PopMatrix();

                break;

                case LightmapPreset.Type.Depth8:

                    // clear R8 depth

                    GL.Clear(false, true,  Rendering.Depth.Main.ClearColor(lightmapPreset)); 

                    GL.LoadPixelMatrix( -sizeX, sizeX, -sizeY, sizeY );
                    GL.MultMatrix(matrix);

                    GL.PushMatrix();

                    Rendering.Depth.Main.Draw(camera, lightmapPreset);

                    GL.PopMatrix();

                break;
            }
        }

        static public Vector2Int GetScreenResolution(LightMainBuffer2D buffer)
        {
            LightmapPreset lightmapPreset = buffer.GetLightmapPreset();

            if (lightmapPreset == null)
            {
                Debug.Log("lightmap preset null");

                return(Vector2Int.zero);
            }

            Camera camera = buffer.cameraSettings.GetCamera();

            if (camera == null)
            {
                Debug.Log("camera null");

                return(Vector2Int.zero);
            }

            float resolution = lightmapPreset.resolution;

            int screenWidth = (int)(camera.pixelRect.width * resolution);
            int screenHeight = (int)(camera.pixelRect.height * resolution);

            return(new Vector2Int(screenWidth, screenHeight));
        }

        static public void InitializeRenderTexture(LightMainBuffer2D buffer)
        {
            Vector2Int screen = GetScreenResolution(buffer);

            if (screen.x <= 0 || screen.y <= 0)
            {
                return;
            }
        
            string idName = "";

            int bufferID = buffer.cameraLightmap.presetId;
            
            if (bufferID < Lighting2D.LightmapPresets.Length)
            {
                idName = Lighting2D.LightmapPresets[bufferID].name + ", ";
            }

            Camera camera = buffer.cameraSettings.GetCamera();

            buffer.name = "Camera Buffer (" + idName + "" + buffer.type + ", Id: " + (bufferID  + 1) + ", Camera: " + camera.name + " )";

            RenderTextureFormat format = RenderTextureFormat.Default;

            switch(buffer.type)
            {
                case LightMainBuffer2D.Type.RGB24:

                    switch(buffer.hdr)
                    {
                        case LightingSettings.HDR.Half:

                            format = RenderTextureFormat.RGB111110Float;

                        break;

                        case LightingSettings.HDR.Float:

                            format = RenderTextureFormat.DefaultHDR;

                        break;

                        case LightingSettings.HDR.Off:

                            format = RenderTextureFormat.RGB565;

                        break;
                    }

                break;

                case LightMainBuffer2D.Type.Depth8:

                    format = RenderTextureFormat.R8; // no HDR

                break;

                case LightMainBuffer2D.Type.R8:

                    format = RenderTextureFormat.R8; // no HDR

                break;

                case LightMainBuffer2D.Type.RHalf:

                    format = RenderTextureFormat.RHalf; // no HDR

                break;
            }

            if (!SystemInfo.SupportsRenderTextureFormat(format))
            {
                format = RenderTextureFormat.Default;
            }

            buffer.renderTexture = new LightTexture (screen.x, screen.y, 0, format);
            buffer.renderTexture.renderTexture.filterMode = Lighting2D.Profile.qualitySettings.lightmapFilterMode;
            buffer.renderTexture.Create ();
        }
    }
}