using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace FunkyCode
{
	[CustomEditor(typeof(LightingManager2D))]
	public class LightingManager2DEditor : Editor
	{
		private static string[] sceneLayer = new string[]{"Scene Layer", "Unity Layer"};
		private static string[] gameLayer = new string[]{"Game Layer", "Unity Layer"};

		private LightingManager2D lightingManager;

		private void OnEnable()
		{
			lightingManager = target as LightingManager2D;
		}

		override public void OnInspectorGUI()
		{
			DrawProfile();

			if (Lighting2D.ProjectSettings.shaderPreview == LightingSettings.ShaderPreview.Enabled)
			{
				EditorGUILayout.Space();

				EditorGUILayout.HelpBox("Shader Preview Enabled", MessageType.Warning);

				if (GUILayout.Button("Disable Shader Preview"))
				{
					LightingSettings.ProjectSettings projectSettings = Lighting2D.ProjectSettings;

					projectSettings.shaderPreview = LightingSettings.ShaderPreview.Disabled;

					LightingManager2D.ForceUpdate();
					Lighting2D.UpdateByProfile(projectSettings.Profile);

					EditorUtility.SetDirty(projectSettings);
				}
			}
			
			EditorGUILayout.Space();

			ResizeCameras(lightingManager.cameras);

			EditorGUILayout.Space();

			DrawCameras(lightingManager, lightingManager.cameras);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("version " + Lighting2D.VERSION_STRING);
			
			if (lightingManager.version < Lighting2D.VERSION)
			{
				Reinitialize(lightingManager);

				return;
			}
			
			if (GUILayout.Button("Re-Initialize"))
			{
				Lighting2DGizmoFiles.Initialize();

				Reinitialize(lightingManager);
			}

			if (GUI.changed)
			{
				Light2D.ForceUpdateAll();
				LightingManager2D.ForceUpdate();

				if (!EditorApplication.isPlaying)
				{	
					EditorUtility.SetDirty(target);
					EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());	
				}
			}
		}

		public void DrawProfile()
		{
			var newProfile = (LightingSettings.Profile)EditorGUILayout.ObjectField("Profile", lightingManager.setProfile, typeof(LightingSettings.Profile), true);
			if (newProfile != lightingManager.setProfile)
			{
				lightingManager.setProfile = newProfile;

				lightingManager.UpdateProfile();

				// LightMainBuffer2D.Clear();
				// Light2D.ForceUpdateAll();
			}
		}

		public void Reinitialize(LightingManager2D manager)
		{
			Debug.Log("Lighting Manager 2D: reinitialized");

			if (manager.version > 0 && manager.version < Lighting2D.VERSION)
			{
				Debug.Log($"Lighting Manager 2D: version update from {manager.version_string} to {Lighting2D.VERSION_STRING}");
			}

			foreach(Transform transform in manager.transform)
			{
				DestroyImmediate(transform.gameObject);
			}
				
			manager.version_string = Lighting2D.VERSION_STRING;
			manager.version = Lighting2D.VERSION;

			Light2D.ForceUpdateAll();

			LightingManager2D.ForceUpdate();

			if (!EditorApplication.isPlaying)
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
			}
		}

		public void ResizeCameras(LightingCameras cameras)
		{
			int oldCount = cameras.Length;
			int newCount = EditorGUILayout.IntSlider("Camera Count", oldCount, 0, 10);
			
			if (oldCount == newCount)
			{
				return;
			}

			CameraSettings[] cameraSettings = cameras.cameraSettings;

			System.Array.Resize(ref cameras.cameraSettings, newCount);

			if (newCount > oldCount)
			{
				// add newcamera objects
				for(int i = 0; i < oldCount; i++)
				{
					cameras.cameraSettings[i] = cameraSettings[i];
				}
			}

			for(int i = 0; i < newCount; i++)
			{
				cameras.cameraSettings[i].id = i;
			}
		}

		public void DrawCameras(LightingManager2D script, LightingCameras cameras)
		{
			for(int id = 0; id < cameras.Length; id++)
			{
				CameraSettings cameraSetting = cameras.Get(id);

				script.foldout_cameras[id] = EditorGUILayout.Foldout(script.foldout_cameras[id], "Camera " + (cameraSetting.id + 1) + " (" + cameraSetting.GetTypeName() + ")", true);

				if (!script.foldout_cameras[id])
				{
					EditorGUILayout.Space();
					continue;
				}

				EditorGUI.indentLevel++;

				EditorGUILayout.Space();

				CameraSettings.CameraType oldType = cameraSetting.cameraType;
				CameraSettings.CameraType newType = (CameraSettings.CameraType)EditorGUILayout.EnumPopup("Camera Type", cameraSetting.cameraType);

				if (newType != oldType)
				{
					cameraSetting.cameraType = newType;

					cameras.Set(id, cameraSetting);
				}

				if (cameraSetting.cameraType == CameraSettings.CameraType.Custom)
				{
					Camera oldCamera = cameraSetting.customCamera;
					Camera newCamera = (Camera)EditorGUILayout.ObjectField(cameraSetting.customCamera, typeof(Camera), true);

					if (oldCamera != newCamera)
					{
						cameraSetting.customCamera = newCamera;

						cameras.Set(id, cameraSetting);
					}
				}

				EditorGUILayout.Space();

				ResizeLightmaps(cameras, id, cameraSetting);

				EditorGUILayout.Space();

				DrawLightmaps(script, cameras, id);

				EditorGUI.indentLevel--;

				EditorGUILayout.Space();
			}
		}

		public void ResizeLightmaps(LightingCameras cameras, int id, CameraSettings cameraSettings)
		{
			CameraLightmap[] cameraLightmaps = cameraSettings.Lightmaps;
			
			int oldCount = cameraLightmaps.Length;
			int newCount = EditorGUILayout.IntSlider("Lightmap Count", oldCount, 0, 10);

			if (oldCount == newCount)
			{
				return;
			}

			System.Array.Resize(ref cameraLightmaps, newCount);

			cameraSettings.Lightmaps = cameraLightmaps;

			for(int i = 0; i < newCount; i++)
			{
				cameraSettings.Lightmaps[i].id = i;
			}

			if (newCount > oldCount)
			{
				for(int i = oldCount; i < newCount; i++)
				{
					cameraSettings.Lightmaps[i] = new CameraLightmap(i);
				}
			}

			cameraSettings.id = id;

			cameras.Set(id, cameraSettings);
		}

		public void DrawLightmaps(LightingManager2D script, LightingCameras cameras, int id)
		{
			CameraSettings cameraSetting = cameras.Get(id);

			for(int index = 0; index < cameraSetting.Lightmaps.Length; index++)
			{
				CameraLightmap cameraLightmap = cameraSetting.Lightmaps[index];

				string[] lightmapLayers = Lighting2D.Profile.lightmapPresets.GetLightmapLayers();

				if (cameraLightmap.presetId < lightmapLayers.Length)
				{
					string presetName = lightmapLayers[cameraLightmap.presetId];

					script.foldout_lightmapPresets[id, index] = EditorGUILayout.Foldout(script.foldout_lightmapPresets[id, index], "Lightmap " + (cameraLightmap.id + 1) + " (" + presetName + ")", true);

					if (!script.foldout_lightmapPresets[id, index])
					{
						EditorGUILayout.Space();
						continue;
					}

					EditorGUI.indentLevel++;

					EditorGUILayout.Space();

					cameraLightmap.presetId = EditorGUILayout.Popup("Lightmap", (int)cameraLightmap.presetId, Lighting2D.Profile.lightmapPresets.GetLightmapLayers());

					EditorGUILayout.Space();

					cameraLightmap.rendering = (CameraLightmap.Rendering)EditorGUILayout.EnumPopup("Rendering", cameraLightmap.rendering);

					if (cameraLightmap.rendering == CameraLightmap.Rendering.Enabled)
					{			
						EditorGUILayout.Space();

						cameraLightmap.sceneView = (CameraLightmap.SceneView)EditorGUILayout.EnumPopup("Scene View", cameraLightmap.sceneView);

						EditorGUILayout.Space();

						cameraLightmap.overlay = (CameraLightmap.Overlay)EditorGUILayout.EnumPopup("Overlay", cameraLightmap.overlay);

						if (cameraLightmap.overlay == CameraLightmap.Overlay.Enabled)
						{
							cameraLightmap.overlayMaterial = (CameraLightmap.OverlayMaterial)EditorGUILayout.EnumPopup("Material", cameraLightmap.overlayMaterial);
				
							if (cameraLightmap.overlayMaterial == CameraLightmap.OverlayMaterial.Custom || cameraLightmap.overlayMaterial == CameraLightmap.OverlayMaterial.Reference)
							{
								cameraLightmap.customMaterial = (Material)EditorGUILayout.ObjectField(cameraLightmap.customMaterial, typeof(Material), true);
							}
							
							if (cameraSetting.cameraType == CameraSettings.CameraType.SceneView)
							{
								cameraLightmap.overlayLayerType = (CameraLightmap.OverlayLayerType)EditorGUILayout.Popup("Type", (int)cameraLightmap.overlayLayerType, sceneLayer); 
							}
								else
							{
								cameraLightmap.overlayLayerType = (CameraLightmap.OverlayLayerType)EditorGUILayout.Popup("Type", (int)cameraLightmap.overlayLayerType, gameLayer); 
							}

							if (cameraLightmap.overlayLayerType == CameraLightmap.OverlayLayerType.UnityLayer) 
							{
								cameraLightmap.renderLayerId = EditorGUILayout.LayerField("Layer", cameraLightmap.renderLayerId);
							}

							cameraLightmap.overlayPosition = (CameraLightmap.OverlayPosition)EditorGUILayout.EnumPopup("Position", cameraLightmap.overlayPosition);

							if (cameraLightmap.overlayPosition == CameraLightmap.OverlayPosition.Custom)
							{
								cameraLightmap.customPosition = EditorGUILayout.FloatField("Position", cameraLightmap.customPosition); 
							}

							EditorGUI.BeginDisabledGroup(Lighting2D.ProjectSettings.renderingMode != LightingSettings.RenderingMode.OnRender);

							GUISortingLayer.Draw(cameraLightmap.sortingLayer, false);

							EditorGUI.EndDisabledGroup();
						}
					}

					EditorGUILayout.Space();

					cameraLightmap.output = (CameraLightmap.Output)EditorGUILayout.EnumPopup("Output", cameraLightmap.output);

					switch(cameraLightmap.output)
					{
						case CameraLightmap.Output.Materials:
									
							script.foldout_lightmapMaterials[id, index] = EditorGUILayout.Foldout(script.foldout_lightmapMaterials[id, index], "Materials", true);

							if (script.foldout_lightmapMaterials[id, index])
							{
								EditorGUI.indentLevel++;

								cameraLightmap.materialsType = (CameraLightmap.MaterialType)EditorGUILayout.EnumPopup("Type", cameraLightmap.materialsType);

								LightmapMaterials materials = cameraLightmap.GetMaterials();

								int matCount = EditorGUILayout.IntField("Count", materials.materials.Length);

								if (matCount != materials.materials.Length)
								{
									System.Array.Resize(ref materials.materials, matCount);
								}

								for(int i = 0; i < materials.materials.Length; i++)
								{
									materials.materials[i] = (Material)EditorGUILayout.ObjectField(materials.materials[i], typeof(Material), true);
								}

								EditorGUI.indentLevel--;
							}

						break;
					}

					cameraSetting.Lightmaps[index] = cameraLightmap;

					EditorGUI.indentLevel--;

					EditorGUILayout.Space();
				}
			}

			cameras.Set(id, cameraSetting);
		}
	}
}