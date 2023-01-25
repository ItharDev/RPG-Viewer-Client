using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode
{
	public class LightMainBuffer2D
	{
		public enum Type
		{
			RGB24,
			R8,
			RHalf,
			Depth8
		}

		public string name = "Uknown";

		private Material material = null;

		public bool updateNeeded = false;

		public Type type;
		public HDR hdr;

		public LightTexture renderTexture;
		public CameraSettings cameraSettings;
		public CameraLightmap cameraLightmap;
		public bool sceneView;

		public static List<LightMainBuffer2D> List = new List<LightMainBuffer2D>();

		public bool IsActive => List.IndexOf(this) > -1;

		public LightMainBuffer2D(bool sceneView, Type type, HDR hdr, CameraSettings cameraSettings, CameraLightmap cameraLightmap)
		{
			this.type = type;
			this.hdr = hdr;

			this.cameraLightmap = cameraLightmap;
			this.cameraSettings = cameraSettings;
			this.sceneView = sceneView;

			List.Add(this);
		}

		public static void Clear()
		{
			foreach(var buffer in new List<LightMainBuffer2D>(List))
			{
				buffer.DestroySelf();
			}

			List.Clear();
		}

		public void DestroySelf()
		{
			if (renderTexture != null)
			{
				if (renderTexture.renderTexture)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy (renderTexture.renderTexture);
					}
					else
					{
						renderTexture.renderTexture.Release();
						renderTexture.renderTexture.DiscardContents();
						
						UnityEngine.Object.DestroyImmediate (renderTexture.renderTexture);
					}
				}
			}

			List.Remove(this);
		}

		static public LightMainBuffer2D Get(bool sceneView, CameraSettings cameraSettings, CameraLightmap lightmap, LightmapPreset lightmapPreset)
		{
			Type type = (Type)lightmapPreset.type;
			HDR hdr = (HDR)lightmapPreset.hdr;

			if (cameraSettings.GetCamera() == null)
			{
				return null;
			}

			foreach(var mainBuffer in List)
				if (mainBuffer.hdr == hdr
					&& mainBuffer.type == type
					&& mainBuffer.sceneView == sceneView
					&& mainBuffer.cameraSettings.GetCamera() == cameraSettings.GetCamera()
					&& mainBuffer.cameraLightmap.presetId == lightmap.presetId)
					return mainBuffer;

			if (Lighting2D.LightmapPresets.Length <= lightmap.presetId)
			{
				Debug.LogWarning("Lighting2D: Not enough buffer settings initialized");

				return null;
			}

			var buffer = new LightMainBuffer2D(sceneView, type, hdr, cameraSettings, lightmap);

			Rendering.LightMainBuffer.InitializeRenderTexture(buffer);

			return buffer;
		}

		public LightmapPreset GetLightmapPreset()
		{
			if (Lighting2D.LightmapPresets.Length <= cameraLightmap.presetId)
			{
				Debug.LogWarning("Lighting2D: Not enough buffer settings initialized");

				return null;
			}

			return Lighting2D.LightmapPresets[cameraLightmap.presetId];
		}

		public void ClearMaterial()
		{
			material = null;
		}

		public Material GetMaterial()
		{
			if (material == null)
			{
				switch(cameraLightmap.overlayMaterial)
				{
					case CameraLightmap.OverlayMaterial.Multiply:
					
						material = new Material(Shader.Find("Light2D/Internal/Multiply"));	
						break;

					case CameraLightmap.OverlayMaterial.Additive:
						
						material = new Material(Shader.Find("Legacy Shaders/Particles/Additive")); // use light 2D shader?	
						break;

					case CameraLightmap.OverlayMaterial.Custom:

						material = new Material(cameraLightmap.GetMaterial());
						break;

					case CameraLightmap.OverlayMaterial.Reference:

						material = cameraLightmap.customMaterial;
						break;
				}
			}
			
			if (material)
			{
				if (renderTexture != null)
				{
					material.mainTexture = renderTexture.renderTexture;
				}
				else
				{
					Debug.LogWarning("render texture null");
				}
			}
			
			return material;
		}

		public void Update()
		{
			Rendering.LightMainBuffer.Update(this);
		}

		public void Render()
		{
			if (cameraLightmap.rendering == CameraLightmap.Rendering.Disabled)
			{
				return;
			}

			if (updateNeeded)
			{
				var camera = Camera.current;
				if (camera)
				{
					// return;	
				}

				if (renderTexture != null)
				{
					var previous = RenderTexture.active;

					RenderTexture.active = renderTexture.renderTexture;

					Rendering.LightMainBuffer.Render(this);

					RenderTexture.active = previous;
				}
				else
				{
					Debug.LogWarning($"null render texture in buffer {cameraSettings.id}:{ cameraLightmap.presetId}:{sceneView}");
				}
			}

			Rendering.LightMainBuffer.DrawOn(this);
		}
	}
}