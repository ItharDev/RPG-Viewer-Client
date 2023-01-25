using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode.Rendering.Manager
{
    public static class Main
	{
        public static void InternalUpdate()
        {
			UpdateCameras();

			CameraTransform.Update();
			
            UpdateMaterials();

			UpdateMainBuffers();
        }

        public static void Render()
		{
			if (Lighting2D.Disable)
			{
				return;
			}

            var cameras = LightingManager2D.Get().cameras;
			if (cameras.Length < 1)
				return;

			UpdateLoop();
			
			LightBuffer2D.List.ForEach(x => x.Render());
			LightMainBuffer2D.List.ForEach(x => x.Render());
		}

        private static void UpdateLoop()
		{
			if (DayLightCollider2D.List.Count > 0)
				for(int id = 0; id < DayLightCollider2D.List.Count; id++)
					DayLightCollider2D.List[id].UpdateLoop();
			
			if (LightCollider2D.List.Count > 0)
				for(int id = 0; id < LightCollider2D.List.Count; id++)
					LightCollider2D.List[id].UpdateLoop();

			if (LightSprite2D.List.Count > 0)
				for(int id = 0; id < LightSprite2D.List.Count; id++)
					LightSprite2D.List[id].UpdateLoop();
			
			if (Light2D.List.Count > 0)
				for(int id = 0; id < Light2D.List.Count; id++)
					Light2D.List[id].UpdateLoop();
		
			if (OnRenderMode.List.Count > 0)
				for(int id = 0; id < OnRenderMode.List.Count; id++)
					OnRenderMode.List[id].UpdateLoop();
		}

        public static void UpdateCameras()
		{
			// should reset materials
			
			LightmapShaders.ResetShaders();

			LightmapMaterials.SetDayLight();

			MaterialSystem.Clear();

            var cameras = LightingManager2D.Get().cameras;

			for(int i = 0; i < cameras.Length; i++)
			{
				var cameraSetting = cameras.Get(i);

				for(int b = 0; b < cameraSetting.Lightmaps.Length; b++)
				{
					var cameraLightmap = cameraSetting.GetLightmap(b);
					if (cameraLightmap.presetId >= Lighting2D.LightmapPresets.Length)
						continue;

					var lightmapPreset = Lighting2D.LightmapPresets[cameraLightmap.presetId];

					var buffer = LightMainBuffer2D.Get(false, cameraSetting, cameraLightmap, lightmapPreset);

					if (buffer != null)
					{
						var camera = cameraSetting.GetCamera();
						CameraTransform.GetCamera(camera);

						PassLightmap(false, buffer, cameraSetting, cameraLightmap, lightmapPreset);
					}
	
					if (cameraLightmap.sceneView == CameraLightmap.SceneView.Enabled)
					{
						var sceneSettings = cameraSetting;
						sceneSettings.cameraType = CameraSettings.CameraType.SceneView;

						buffer = LightMainBuffer2D.Get(true, sceneSettings, cameraLightmap, lightmapPreset);

						if (buffer != null)
						{
							Camera camera = sceneSettings.GetCamera();
							CameraTransform.GetCamera(camera);
						
							PassLightmap(true, buffer, sceneSettings, cameraLightmap, lightmapPreset);
						}
					}
				}
			}

			if (MaterialSystem.Count > 0)
			{
				for(int i = 0; i < MaterialSystem.Count; i++)
				{
					var material = MaterialSystem.materialPasses[i].material;

					LightmapMaterials.ClearMaterial(material);
				}
				
				for(int i = 0; i < MaterialSystem.Count; i++)
				{
					var pass = MaterialSystem.materialPasses[i];

					int passId = pass.passId;

					if (passId == 0)
					{
						passId = 1; // incremental
					}

					LightmapMaterials.SetMaterial(passId, pass);
				}				
			}
		}

		public static void PassLightmap(bool isSceneView, LightMainBuffer2D buffer, CameraSettings cameraSetting, CameraLightmap cameraLightmap, LightmapPreset lightmapPreset)
		{
			// bool isSceneView = false; // bool isSceneView = cameraSetting.cameraType == CameraSettings.CameraType.SceneView;

			buffer.cameraLightmap.rendering = cameraLightmap.rendering;

			buffer.cameraLightmap.overlay = cameraLightmap.overlay;

			buffer.cameraLightmap.renderLayerId = cameraLightmap.renderLayerId;

			if (buffer.cameraLightmap.customMaterial != cameraLightmap.customMaterial)
			{
				buffer.cameraLightmap.customMaterial = cameraLightmap.customMaterial;

				buffer.ClearMaterial();
			}

			if (buffer.cameraLightmap.overlayMaterial != cameraLightmap.overlayMaterial)
			{
				buffer.cameraLightmap.overlayMaterial = cameraLightmap.overlayMaterial;

				buffer.ClearMaterial();
			}

			if (cameraLightmap.rendering == CameraLightmap.Rendering.Disabled)
			{
				return;
			}

			Camera camera = cameraSetting.GetCamera();

			switch(cameraLightmap.output)
			{
				case CameraLightmap.Output.Materials:

					foreach(Material material in cameraLightmap.GetMaterials().materials)
					{
						if (material == null)
						{
							continue;
						}

						int matPassId = 0;

						switch(cameraLightmap.materialsType)
						{
							case CameraLightmap.MaterialType.Pass1: matPassId = 1; break;
							case CameraLightmap.MaterialType.Pass2: matPassId = 2; break;
							case CameraLightmap.MaterialType.Pass3: matPassId = 3; break;
							case CameraLightmap.MaterialType.Pass4: matPassId = 4; break;
							case CameraLightmap.MaterialType.Pass5: matPassId = 5; break;
							case CameraLightmap.MaterialType.Pass6: matPassId = 6; break;
							case CameraLightmap.MaterialType.Pass7: matPassId = 7; break;
							case CameraLightmap.MaterialType.Pass8: matPassId = 8; break;
						}
						
						MaterialSystem.Add(material, isSceneView, matPassId, camera, buffer.renderTexture, lightmapPreset);			
					}

				break;

				case CameraLightmap.Output.Shaders:

					// incremental ID

					int passId = 1; // incremental

					LightmapShaders.SetShaders(isSceneView, passId, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass1:

					LightmapShaders.SetShaders(isSceneView, 1, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass2:

					LightmapShaders.SetShaders(isSceneView, 2, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass3:

					LightmapShaders.SetShaders(isSceneView, 3, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass4:

					LightmapShaders.SetShaders(isSceneView, 4, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass5:

					LightmapShaders.SetShaders(isSceneView, 5, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass6:

					LightmapShaders.SetShaders(isSceneView, 6, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass7:

					LightmapShaders.SetShaders(isSceneView, 7, camera, buffer.renderTexture, lightmapPreset);

				break;

				case CameraLightmap.Output.Pass8:

					LightmapShaders.SetShaders(isSceneView, 8, camera, buffer.renderTexture, lightmapPreset);

				break;
			}
		}
		
		public static void UpdateMaterials()
		{
			if (Lighting2D.Materials.Initialize())
			{
				LightMainBuffer2D.Clear();
				LightBuffer2D.Clear();

				Light2D.ForceUpdateAll();
			}
		}

		public static void UpdateMainBuffers()
		{
			if (LightMainBuffer2D.List.Count <= 0)
			{
				return;
			}

			for(int i = 0; i < LightMainBuffer2D.List.Count; i++)
			{
				LightMainBuffer2D.List[i]?.Update();
			}

			for(int i = 0; i < LightMainBuffer2D.List.Count; i++)
			{
				var buffer = LightMainBuffer2D.List[i];

				if (Lighting2D.Disable)
				{
					buffer.updateNeeded = false;	

					continue;
				}

				var cameraSettings = buffer.cameraSettings;
				var cameraLightmap = buffer.cameraLightmap;
				
				bool render = cameraLightmap.rendering != CameraLightmap.Rendering.Disabled;
			
				buffer.updateNeeded = (render && cameraSettings.GetCamera() != null);
			}
		}
    }
}