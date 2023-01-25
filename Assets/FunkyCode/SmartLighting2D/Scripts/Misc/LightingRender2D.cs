using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode
{
	public class LightingRender2D
	{
		public static Mesh preRenderMesh = null;

		public static Mesh GetMesh()
		{
			if (preRenderMesh == null)
			{
				Mesh mesh = new Mesh();

				mesh.vertices = new Vector3[]{new Vector3(-1, -1), new Vector3(1, -1), new Vector3(1, 1), new Vector3(-1, 1)};
				mesh.triangles = new int[]{2, 1, 0, 0, 3, 2};
				mesh.uv = new Vector2[]{new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)};

				preRenderMesh = mesh;
			}
			return(preRenderMesh);
		}

		static public Vector2Int GetTextureSize(LightingSourceTextureSize textureSize)
		{
			switch(textureSize)
			{
				case LightingSourceTextureSize.PixelPerfect:
					return(new Vector2Int(Screen.width, Screen.height));

				case LightingSourceTextureSize.px2048:
					return(new Vector2Int(2048, 2048));

				case LightingSourceTextureSize.px1024:
					return(new Vector2Int(1024, 1024));

				case LightingSourceTextureSize.px512:
					return(new Vector2Int(512, 512));

				case LightingSourceTextureSize.px256:
					return(new Vector2Int(256, 256));
				
				default:
					return(new Vector2Int(128, 128));
			}
		}

		public static Vector3 GetSize(Camera camera)
		{
			float sizeY = camera.orthographicSize;

			Vector3 size = new Vector2(sizeY, sizeY);
			
			size.x *= ((float)camera.pixelRect.width / (float)camera.pixelRect.height);
			size.x *= (((float)camera.pixelRect.width + 1f) / camera.pixelRect.width);

			size.y *= (((float)camera.pixelRect.height + 1f) / camera.pixelRect.height);
			
			size.z = 1;

			return(size);
		}

		// post-render mode drawing
		public static void PostRender(LightMainBuffer2D mainBuffer)
		{
			Camera camera = mainBuffer.cameraSettings.GetCamera();

			if (camera == null)
			{
				return;
			}

			if (mainBuffer.cameraLightmap.rendering == CameraLightmap.Rendering.Disabled)
			{
				return;
			}

			if (Lighting2D.RenderingMode != RenderingMode.OnPostRender)
			{
				return;
			}

			if (Camera.current != camera)
			{
				return;
			}

			Rendering.Universal.Texture.Quad.Draw(mainBuffer.GetMaterial(), LightingPosition.GetCameraPlanePosition(camera), GetSize(camera), camera.transform.eulerAngles.z, LightingPosition.GetCameraPlanePosition(camera).z);
		}

		// mesh-render mode drawing
		static public void OnRender(LightMainBuffer2D mainBuffer)
		{
			Camera camera = mainBuffer.cameraSettings.GetCamera();

			if (camera == null)
			{
				return;
			}

			if (mainBuffer.cameraLightmap.rendering == CameraLightmap.Rendering.Disabled)
			{
				return;
			}

			if (Lighting2D.RenderingMode != RenderingMode.OnRender)
			{
				return;
			}
			
			OnRenderMode onRenderMode = OnRenderMode.Get(mainBuffer);

			if (onRenderMode == null)
			{
				return;
			}
			
			onRenderMode.UpdatePosition();

			if (onRenderMode.meshRenderer != null)
			{
				if (mainBuffer.cameraLightmap.rendering!= CameraLightmap.Rendering.Enabled)
				{
					onRenderMode.meshRenderer.enabled = false;
					return;
				}

				onRenderMode.meshRenderer.enabled = true;
				
				if (onRenderMode.meshRenderer.sharedMaterial != mainBuffer.GetMaterial())
				{
					onRenderMode.meshRenderer.sharedMaterial = mainBuffer.GetMaterial();
				}
				
				if (onRenderMode.meshRenderer.sharedMaterial == null)
				{
					onRenderMode.meshRenderer.sharedMaterial = mainBuffer.GetMaterial();
				}
			}
		}

		// graphics.draw() mode drawing
		static public void PreRender(LightMainBuffer2D mainBuffer)
		{
			Camera camera = mainBuffer.cameraSettings.GetCamera();

			if (camera == null)
			{
				return;
			}

			if (mainBuffer.cameraLightmap.rendering == CameraLightmap.Rendering.Disabled)
			{
				return;
			}

			if (Lighting2D.RenderingMode != RenderingMode.OnPreRender)
			{
				return;
			}

			Graphics.DrawMesh(LightingRender2D.GetMesh(), Matrix4x4.TRS(LightingPosition.GetCameraPlanePosition(camera), camera.transform.rotation, GetSize(camera)), mainBuffer.GetMaterial(), 0, camera);
		}
	}
}