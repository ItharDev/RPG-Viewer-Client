using UnityEngine;
using UnityEditor;
using FunkyCode.LightingSettings;
using FunkyCode.LightSettings;

namespace FunkyCode
{
	public class ProfileEditor
	{
		public static void DrawProfile(LightingSettings.Profile profile)
		{
			EditorGUI.BeginChangeCheck ();

			// Common Settings

			CommonSettings(profile.lightmapPresets.list[0]);

			EditorGUILayout.Space();

			// Quality Settings

			QualitySettings.Draw(profile);

			EditorGUILayout.Space();

			// Layers

			Layers.Draw(profile);
			
			EditorGUILayout.Space();

			// Day Lighting

			DayLighting.Draw(profile);
			
			EditorGUILayout.Space();

			// Lightmap Presets

			LightmapPresets.Draw(profile.lightmapPresets);

			EditorGUILayout.Space();

			// Light Presets

			LightPresets.Draw(profile.lightPresets);

			EditorGUILayout.Space();

			// Event Presets

			EventPresets.Draw(profile.eventPresets);

			EditorGUILayout.Space();

			EditorGUI.EndChangeCheck ();

			if (GUI.changed)
			{
				if (!EditorApplication.isPlaying)
				{
					if (Lighting2D.Profile == profile)
					{
						Light2D.ForceUpdateAll();
					
						LightingManager2D.ForceUpdate();

						/*

						foreach(OnRenderMode onRender in OnRenderMode.List) {
							LightmapPreset lightmapPreset = onRender.mainBuffer.GetLightmapPreset();
							lightmapPreset.sortingLayer.ApplyToMeshRenderer(onRender.meshRenderer);
						}*/
					}

					EditorUtility.SetDirty(profile);
				}
			}
		}
		
		public static void Draw()
		{
			LightingSettings.Profile profile = Lighting2D.Profile;

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("Current Profile", profile, typeof(LightingSettings.Profile), true);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();

			if (profile == null)
			{
				EditorGUILayout.HelpBox("Lighting2D Settings Profile Not Found!", MessageType.Error);

				return;
			}

			DrawProfile(profile);
		}

		public class LightPresets
		{
			public static void Draw(LightPresetList lightPresetList)
			{
				bool foldout = GUIFoldoutHeader.Begin( "Light Presets (" + lightPresetList.list.Length + ")", lightPresetList);

				if (!foldout)
				{
					GUIFoldoutHeader.End();
					return;
				}

				EditorGUI.indentLevel++;

				int presetCount = EditorGUILayout.IntSlider ("Count", lightPresetList.list.Length, 1, 8);

				if (presetCount !=lightPresetList.list.Length)
				{
					int oldCount = lightPresetList.list.Length;

					System.Array.Resize(ref lightPresetList.list, presetCount);

					for(int i = oldCount; i < presetCount; i++)
					{
						lightPresetList.list[i] = new LightPreset(i);
					}
				}

				for(int i = 0; i < lightPresetList.list.Length; i++)
				{
					LightPreset lightPreset = lightPresetList.list[i];
					
					bool fold = GUIFoldout.Draw( "Preset " + (i + 1) + " (" + lightPreset.name + ")", lightPreset);

					if (!fold)
					{
						continue;
					}

					EditorGUI.indentLevel++;

					lightPreset.name = EditorGUILayout.TextField ("Name", lightPreset.name);

					EditorGUILayout.Space();
					
					DrawLightLayers(lightPreset.layerSetting);

					EditorGUI.indentLevel--;
				}

				EditorGUI.indentLevel--;

				GUIFoldoutHeader.End();
			}
		}

		public class EventPresets
		{
			public static void Draw(EventPresetList eventPresetList)
			{
				bool foldout = GUIFoldoutHeader.Begin( "Light Event Presets (" + (eventPresetList.list.Length - 1) + ")", eventPresetList);

				if (!foldout)
				{
					GUIFoldoutHeader.End();
					return;
				}

				EditorGUI.indentLevel++;

				int bufferCount = EditorGUILayout.IntSlider ("Count", eventPresetList.list.Length - 1, 1, 4) + 1;

				if (bufferCount != eventPresetList.list.Length)
				{
					int oldCount = eventPresetList.list.Length;

					System.Array.Resize(ref eventPresetList.list, bufferCount);

					for(int i = oldCount; i < bufferCount; i++)
					{
						eventPresetList.list[i] = new EventPreset(i);
					}
				}

				for(int i = 1; i < eventPresetList.list.Length; i++)
				{
					EventPreset eventPreset = eventPresetList.list[i];

					bool fold = GUIFoldout.Draw( "Preset " + (i) + " (" + eventPreset.name + ")", eventPreset);

					if (!fold)
					{
						continue;
					}

					EditorGUI.indentLevel++;

					eventPreset.name = EditorGUILayout.TextField ("Name", eventPreset.name);

					EditorGUILayout.Space();
					
					DrawEventLayers(eventPreset.layerSetting);

					EditorGUI.indentLevel--;
				}

				EditorGUI.indentLevel--;

				GUIFoldoutHeader.End();
			}
		}

		static public void DrawEventLayers(EventPresetLayers presetLayers)
		{
			LayerEventSetting[] layerSetting = presetLayers.Get();

			int layerCount = layerSetting.Length;

			layerCount = EditorGUILayout.IntSlider("Layer Count", layerCount, 1, 4);

			EditorGUILayout.Space();

			if (layerCount != layerSetting.Length)
			{
				int oldCount = layerSetting.Length;

				System.Array.Resize(ref layerSetting, layerCount);

				for(int i = oldCount; i < layerCount; i++)
				{
					if (layerSetting[i] == null)
					{
						layerSetting[i] = new LayerEventSetting();
						layerSetting[i].layerID = i;
					}
				}

				presetLayers.SetArray(layerSetting);
			}

			for(int i = 0; i < layerSetting.Length; i++)
			{
				LayerEventSetting layer = layerSetting[i];

				layer.layerID = EditorGUILayout.Popup(" ", layer.layerID, Lighting2D.Profile.layers.colliderLayers.GetNames());
			}
		}

		static public void DrawLightLayers(LightPresetLayers presetLayers)
		{
			LayerSetting[] layerSetting = presetLayers.Get();

			int layerCount = layerSetting.Length;

			layerCount = EditorGUILayout.IntSlider("Layer Count", layerCount, 1, 8);

			EditorGUILayout.Space();

			if (layerCount != layerSetting.Length)
			{
				int oldCount = layerSetting.Length;

				System.Array.Resize(ref layerSetting, layerCount);

				for(int i = oldCount; i < layerCount; i++)
				{
					if (layerSetting[i] == null)
					{
						layerSetting[i] = new LayerSetting();
						layerSetting[i].layerID = i;
					}
					
				}

				presetLayers.SetArray(layerSetting);
			}

			for(int i = 0; i < layerSetting.Length; i++)
			{
				LayerSetting layer = layerSetting[i];

				bool foldout = GUIFoldout.Draw( "Layer " + (i + 1), layer);

				if (foldout)
				{
					EditorGUI.indentLevel++;
				
					layer.layerID = EditorGUILayout.Popup("Layer (Collider)", layer.layerID, Lighting2D.Profile.layers.colliderLayers.GetNames());
					
					layer.type = (LightLayerType)EditorGUILayout.EnumPopup("Type", layer.type);

					bool shadowEnabled = layer.type != LightLayerType.MaskOnly;
					bool maskEnabled = layer.type != LightLayerType.ShadowOnly;
					
					EditorGUILayout.Space();

					layer.sorting = (LightLayerSorting)EditorGUILayout.EnumPopup("Sorting", layer.sorting);
					
					EditorGUI.BeginDisabledGroup(layer.sorting == LightLayerSorting.None);
					
					layer.sortingIgnore = (LightLayerSortingIgnore)EditorGUILayout.EnumPopup("Sorting Ignore", layer.sortingIgnore);
					
					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					EditorGUI.BeginDisabledGroup(!shadowEnabled);

					layer.shadowEffect = (LightLayerShadowEffect)EditorGUILayout.EnumPopup("Shadow Effect", layer.shadowEffect);

					EditorGUI.EndDisabledGroup();

					EditorGUI.BeginDisabledGroup(!shadowEnabled || layer.shadowEffect != LightLayerShadowEffect.PerpendicularProjection);

					layer.shadowEffectLayer = EditorGUILayout.Popup("Effect Layer (Collider)", layer.shadowEffectLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());

					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					EditorGUI.BeginDisabledGroup(!maskEnabled);

					layer.maskLit = (LightLayerMaskLit)EditorGUILayout.EnumPopup("Mask Lit", layer.maskLit);

					EditorGUI.EndDisabledGroup();

					bool maskEffectLit = (layer.maskLit == LightLayerMaskLit.AboveLit);
			
					EditorGUI.BeginDisabledGroup(!maskEnabled || !maskEffectLit);
				
					layer.maskLitDistance = EditorGUILayout.FloatField("Mask Lit Distance", layer.maskLitDistance);

					if (layer.maskLitDistance < 0)
					{
						layer.maskLitDistance = 0;
					}
			
					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					EditorGUI.indentLevel--;
				}
				
				EditorGUILayout.Space();
			}
		}

		public class LightmapPresets
		{
			public static void Draw(LightmapPresetList lightmapList)
			{
				bool foldout = GUIFoldoutHeader.Begin( "Lightmap Presets (" + lightmapList.list.Length + ")", lightmapList);

				if (!foldout)
				{
					GUIFoldoutHeader.End();
					return;
				}

				EditorGUI.indentLevel++;

				int bufferCount = EditorGUILayout.IntSlider ("Count", lightmapList.list.Length, 1, 8);

				if (bufferCount != lightmapList.list.Length)
				{
					int oldCount = lightmapList.list.Length;

					System.Array.Resize(ref lightmapList.list, bufferCount);

					for(int i = oldCount; i < bufferCount; i++)
					{
						lightmapList.list[i] = new LightmapPreset(i);
					}
				}

				for(int i = 0; i < lightmapList.list.Length; i++)
				{
					LightmapPreset lightmapPreset = lightmapList.list[i];

					bool fold = GUIFoldout.Draw( "Preset " + (i + 1) + " (" + lightmapPreset.name + ")", lightmapPreset);

					if (!fold)
					{
						continue;
					}

					EditorGUI.indentLevel++;

					lightmapPreset.name = EditorGUILayout.TextField ("Name", lightmapPreset.name);

					EditorGUILayout.Space();

					lightmapPreset.type = (LightmapPreset.Type)EditorGUILayout.EnumPopup ("Type", lightmapPreset.type);

					lightmapPreset.hdr = (LightmapPreset.HDR)EditorGUILayout.EnumPopup ("HDR", lightmapPreset.hdr);

					switch(lightmapPreset.type)
					{
						case LightmapPreset.Type.RGB24:
						case LightmapPreset.Type.R8:
						case LightmapPreset.Type.RHalf:

							EditorGUILayout.Space();

							CommonSettings(lightmapPreset);

							EditorGUILayout.Space();
							
							EditorGUILayout.Space();

							LayerSettings.DrawList(lightmapPreset.dayLayers, "Day Layers (" + lightmapPreset.dayLayers.list.Length + ")", Lighting2D.Profile.layers.dayLayers, true);

							EditorGUILayout.Space();
							
							LayerSettings.DrawList(lightmapPreset.lightLayers, "Light Layers (" + lightmapPreset.lightLayers.list.Length  + ")", Lighting2D.Profile.layers.lightLayers, false);

							EditorGUILayout.Space();

							break;

						case LightmapPreset.Type.Depth8:

							lightmapPreset.depth = EditorGUILayout.IntSlider("Depth", lightmapPreset.depth, -100, 100);

							lightmapPreset.resolution = EditorGUILayout.Slider("Resolution", lightmapPreset.resolution, 0.25f, 1.0f);
						
							EditorGUILayout.Space();

							LayerSettings.DrawList(lightmapPreset.dayLayers, "Day Layers (" + lightmapPreset.dayLayers.list.Length + ")", Lighting2D.Profile.layers.dayLayers, false);

							EditorGUILayout.Space();

							break;
					}

					EditorGUI.indentLevel--;
				}
			
				EditorGUI.indentLevel--;

				GUIFoldoutHeader.End();
			}
		}

		public class Layers
		{
			public static void Draw(LightingSettings.Profile profile)
			{
				bool foldout = GUIFoldoutHeader.Begin("Layers", profile.layers);
		
				if (!foldout)
				{
					GUIFoldoutHeader.End();
					return;
				}

				EditorGUI.indentLevel++;

					EditorGUILayout.Space();

					DrawList(profile.layers.colliderLayers, "Collider Layers", "Collider Layer");

					EditorGUILayout.Space();

					DrawList(profile.layers.lightLayers, "Light Layers", "Light Layer");

					EditorGUILayout.Space();

					DrawList(profile.layers.dayLayers, "Day Layers", "Day Layer");

				EditorGUI.indentLevel--;

				GUIFoldoutHeader.End();
			}

			public static void DrawList(LightingSettings.LayersList layerList, string name, string singular)
			{
				bool foldout = GUIFoldout.Draw(name, layerList);

				if (!foldout)
				{
					return;
				}
				
				EditorGUI.indentLevel++;

				int lightLayerCount = EditorGUILayout.IntSlider ("Count", layerList.names.Length, 1, 10);

				if (lightLayerCount != layerList.names.Length)
				{
					int oldCount = layerList.names.Length;

					System.Array.Resize(ref layerList.names, lightLayerCount);

					for(int i = oldCount; i < lightLayerCount; i++)
					{
						layerList.names[i] = singular + " " + (i);
					}
				}

				for(int i = 0; i < lightLayerCount; i++)
				{
					layerList.names[i] = EditorGUILayout.TextField(" ", layerList.names[i]);
				}

				EditorGUI.indentLevel--;
			}
		}

		public class QualitySettings
		{
			public static void Draw(LightingSettings.Profile profile)
			{
				bool foldout = GUIFoldoutHeader.Begin( "Quality", profile.qualitySettings);

				if (!foldout)
				{
					GUIFoldoutHeader.End();
					return;
				}
		
				EditorGUI.indentLevel++;

					EditorGUILayout.Space();

					profile.qualitySettings.projection = (Projection)EditorGUILayout.EnumPopup("Projection", profile.qualitySettings.projection);
							
					profile.qualitySettings.coreAxis = (CoreAxis)EditorGUILayout.EnumPopup("Core Axis", profile.qualitySettings.coreAxis);

					profile.qualitySettings.updateMethod = (LightingSettings.UpdateMethod)EditorGUILayout.EnumPopup("Update Method", profile.qualitySettings.updateMethod);

					profile.qualitySettings.lightTextureSize = (LightingSourceTextureSize)EditorGUILayout.Popup("Light Resolution", (int)profile.qualitySettings.lightTextureSize, LightingSettings.QualitySettings.LightingSourceTextureSizeArray);

					profile.qualitySettings.lightFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Light Filter Mode", profile.qualitySettings.lightFilterMode);
						
					profile.qualitySettings.lightEffectTextureSize = (LightingSourceTextureSize)EditorGUILayout.Popup("Translucent Resolution", (int)profile.qualitySettings.lightEffectTextureSize, LightingSettings.QualitySettings.LightingSourceTextureSizeArray);

					profile.qualitySettings.lightmapFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Lightmap Filter Mode", profile.qualitySettings.lightmapFilterMode);

				EditorGUI.indentLevel--;

				GUIFoldoutHeader.End();
			}
		}

		public class DayLighting
		{
			public static void Draw(LightingSettings.Profile profile)
			{
				bool foldout = GUIFoldoutHeader.Begin( "Day Lighting", profile.dayLightingSettings);

				if (!foldout)
				{
					GUIFoldoutHeader.End();
					return;
				}

				EditorGUI.indentLevel++;

					EditorGUILayout.Space();

					profile.dayLightingSettings.ShadowColor = EditorGUILayout.ColorField("Shadow Color", profile.dayLightingSettings.ShadowColor);

					profile.dayLightingSettings.ShadowColor.a = EditorGUILayout.Slider("Shadow Alpha", profile.dayLightingSettings.ShadowColor.a, 0, 1);
					
					profile.dayLightingSettings.direction = EditorGUILayout.Slider("Direction", profile.dayLightingSettings.direction, 0 , 360);

					profile.dayLightingSettings.height = EditorGUILayout.Slider("Height", profile.dayLightingSettings.height, 0.1f, 10);

					NormalMap.Draw(profile);
			
				EditorGUI.indentLevel--;

				GUIFoldoutHeader.End();
			}

			public class NormalMap
			{
				public static void Draw(LightingSettings.Profile profile)
				{
					profile.dayLightingSettings.bumpMap.height = EditorGUILayout.Slider("Bump Height", profile.dayLightingSettings.bumpMap.height, 0, 5);
					profile.dayLightingSettings.bumpMap.strength = EditorGUILayout.Slider("Bump Strength", profile.dayLightingSettings.bumpMap.strength, 0, 5);
				}
			}
		}

		static void CommonSettings(LightmapPreset lightmapPreset)
		{
			if (lightmapPreset.type != LightmapPreset.Type.Depth8)
			{
				lightmapPreset.darknessColor = EditorGUILayout.ColorField("Darkness Color", lightmapPreset.darknessColor);
				lightmapPreset.darknessColor.a = EditorGUILayout.Slider("Darkness Alpha", lightmapPreset.darknessColor.a, 0, 1);
				lightmapPreset.resolution = EditorGUILayout.Slider("Resolution", lightmapPreset.resolution, 0.25f, 1.0f);
			}
		}

		public class LayerSettings
		{
			public static void DrawList(LightmapLayerList lightmapLayers, string name, LayersList layerList, bool drawType)
			{
				bool foldout = GUIFoldout.Draw(name, lightmapLayers);

				if (!foldout)
				{
					return;
				}

				EditorGUI.indentLevel++;

				LightmapLayer[] layerSettings = lightmapLayers.Get();
			
				int layerCount = EditorGUILayout.IntSlider ("Count", layerSettings.Length, 0, 10);

				EditorGUILayout.Space();
				
				if (layerCount != layerSettings.Length)
				{
					int oldCount = layerSettings.Length;

					System.Array.Resize(ref layerSettings, layerCount);

					for(int i = oldCount; i < layerCount; i++)
					{
						if (layerSettings[i] == null)
						{
							layerSettings[i] = new LightmapLayer();
							layerSettings[i].id = i;
						}
					}

					lightmapLayers.SetArray(layerSettings);
				}

				for(int i = 0; i < layerSettings.Length; i++)
				{
					layerSettings[i].id = EditorGUILayout.Popup("Layer", layerSettings[i].id, layerList.GetNames());

					if (drawType)
					{
						layerSettings[i].type = (LayerType)EditorGUILayout.EnumPopup("Type", layerSettings[i].type);
						layerSettings[i].sorting = (LayerSorting)EditorGUILayout.EnumPopup("Sorting", layerSettings[i].sorting);
					}
						
					EditorGUILayout.Space();
				}

				EditorGUI.indentLevel--;
			}
		}
	}
}