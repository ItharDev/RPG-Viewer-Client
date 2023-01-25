using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using FunkyCode.LightingSettings;
using FunkyCode.LightSettings;

namespace FunkyCode
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(DayLightCollider2D))]
	public class DayLightCollider2DEditor : Editor
	{
		private DayLightCollider2D dayLightCollider2D;

		private SerializedProperty shadowType;
		private SerializedProperty shadowLayer;

		private SerializedProperty shadowDistance;
		private SerializedProperty shadowThickness;
		private SerializedProperty shadowEffect;
		private SerializedProperty shadowSoftness;
		private SerializedProperty shadowTranslucency;

		private SerializedProperty maskType;
		private SerializedProperty maskLit;
		private SerializedProperty maskLayer;

		private SerializedProperty depth;

		private SerializedProperty depthFalloff;

		private SerializedProperty depthCustomValue;
		
		private void InitProperties()
		{
			shadowType = serializedObject.FindProperty("shadowType");
			shadowLayer = serializedObject.FindProperty("shadowLayer");
			shadowDistance = serializedObject.FindProperty("shadowDistance");
			shadowTranslucency = serializedObject.FindProperty("shadowTranslucency");

			shadowEffect = serializedObject.FindProperty("shadowEffect");
			shadowSoftness = serializedObject.FindProperty("shadowSoftness");

			shadowThickness = serializedObject.FindProperty("shadowThickness");

			maskType = serializedObject.FindProperty("maskType");
			maskLayer = serializedObject.FindProperty("maskLayer");
			maskLit = serializedObject.FindProperty("maskLit");

			depth = serializedObject.FindProperty("depth");
			depthFalloff = serializedObject.FindProperty("depthFalloff");

			depthCustomValue = serializedObject.FindProperty("depthCustomValue");
		}

		private void OnEnable(){
			dayLightCollider2D = target as DayLightCollider2D;

			InitProperties();
			
			Undo.undoRedoPerformed += RefreshAll;
		}

		internal void OnDisable(){
			Undo.undoRedoPerformed -= RefreshAll;
		}

		void RefreshAll(){
			DayLightCollider2D.ForceUpdateAll();
		}

		static public bool foldoutbumpedSprite = false;

		override public void OnInspectorGUI()
		{
			DayLightCollider2D script = target as DayLightCollider2D;

			if (!UsesShadows() && !UsesMask() && !UsesDepth())
			{
				EditorGUILayout.HelpBox("Day Layers are not included in Lightmap Presets \n", MessageType.Warning);
				
				return;
			}

			if (UsesShadows())
			{
				// Shadow Properties
				EditorGUILayout.PropertyField(shadowType, new GUIContent ("Shadow Type"));

				if (script.mainShape.shadowType != DayLightCollider2D.ShadowType.None)
				{
					shadowLayer.intValue = EditorGUILayout.Popup("Shadow Layer (Day)", shadowLayer.intValue, Lighting2D.Profile.layers.dayLayers.GetNames());

					if (script.mainShape.shadowType == DayLightCollider2D.ShadowType.SpritePhysicsShape || script.mainShape.shadowType == DayLightCollider2D.ShadowType.Collider2D || script.mainShape.shadowType == DayLightCollider2D.ShadowType.FillSpritePhysicsShape)
					{
						EditorGUILayout.PropertyField(shadowEffect, new GUIContent ("Shadow Effect"));

						if (dayLightCollider2D.shadowEffect == DayLightCollider2D.ShadowEffect.Softness)
						{
							EditorGUILayout.PropertyField(shadowSoftness, new GUIContent ("Shadow Softness"));
						}
					}
					
					if (script.mainShape.shadowType != DayLightCollider2D.ShadowType.FillCollider2D && script.mainShape.shadowType != DayLightCollider2D.ShadowType.FillSpritePhysicsShape)
					{
						EditorGUILayout.PropertyField(shadowDistance, new GUIContent ("Shadow Distance"));
					}

					if (script.mainShape.shadowType == DayLightCollider2D.ShadowType.SpriteProjection)
					{
						EditorGUILayout.PropertyField(shadowThickness, new GUIContent ("Shadow Thickness"));
					}

					EditorGUILayout.PropertyField(shadowTranslucency, new GUIContent ("Shadow Translucency"));
				}

				EditorGUILayout.Space();
			}

			if (UsesMask())
			{
				EditorGUILayout.PropertyField(maskType, new GUIContent ("Mask Type"));
				
				if (script.mainShape.maskType != DayLightCollider2D.MaskType.None)
				{
					maskLayer.intValue = EditorGUILayout.Popup("Mask Layer (Day)", maskLayer.intValue, Lighting2D.Profile.layers.dayLayers.GetNames());
					
					if (script.mainShape.maskType == DayLightCollider2D.MaskType.BumpedSprite)
					{
						GUIBumpMapMode.DrawDay(script.normalMapMode);
					}

					EditorGUILayout.PropertyField(maskLit, new GUIContent ("Mask Lit"));
				}

				EditorGUILayout.Space();
			}

			if (UsesDepth())
			{
				EditorGUILayout.PropertyField(depth, new GUIContent ("Depth"));

				if (script.depth != DayLightCollider2D.Depth.None)
				{
					if (dayLightCollider2D.shadowType != DayLightCollider2D.ShadowType.FillCollider2D && dayLightCollider2D.shadowType != DayLightCollider2D.ShadowType.FillSpritePhysicsShape && dayLightCollider2D.shadowType != DayLightCollider2D.ShadowType.SpriteOffset)
					{
						EditorGUILayout.PropertyField(depthFalloff, new GUIContent ("Depth Falloff"));
					}
						else
					{
						EditorGUI.BeginDisabledGroup(true);

						EditorGUILayout.EnumPopup("Depth Falloff", (DayLightCollider2D.DepthFalloff.Disabled));

						EditorGUI.EndDisabledGroup();
					}

					switch(script.depth)
					{
						case DayLightCollider2D.Depth.Custom:

							depthCustomValue.intValue = EditorGUILayout.IntSlider("Depth Value", depthCustomValue.intValue, -100, 100);

						break;

						case DayLightCollider2D.Depth.SortingOrder:
						case DayLightCollider2D.Depth.ZPosition:

							EditorGUI.BeginDisabledGroup(true);

							EditorGUILayout.Slider("Depth Value", dayLightCollider2D.GetDepth(), -100, 100);

							EditorGUI.EndDisabledGroup();

						break;
					}
				}

				EditorGUILayout.Space();
			}

			serializedObject.ApplyModifiedProperties();
			
			if (GUILayout.Button("Update"))
			{
				SpriteExtension.PhysicsShapeManager.Clear();

				foreach(UnityEngine.Object target in targets)
				{
					DayLightCollider2D daylightCollider2D = target as DayLightCollider2D;
					
					daylightCollider2D.mainShape.ResetLocal();

					daylightCollider2D.Initialize();
				}
			}

			if (GUI.changed)
			{
				foreach(UnityEngine.Object target in targets)
				{
					DayLightCollider2D daylightCollider2D = target as DayLightCollider2D;
					daylightCollider2D.Initialize();

					if (!EditorApplication.isPlaying)
					{
						EditorUtility.SetDirty(target);
					}
				}

				if (!EditorApplication.isPlaying)
				{
					EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
				}
			}
		}

		public bool UsesDepth()
		{
			LightmapPresetList presetList = Lighting2D.Profile.lightmapPresets;

			for(int i = 0; i < presetList.list.Length; i++)
			{
				LightmapPreset preset = presetList[i];

				if (preset.type == LightmapPreset.Type.Depth8)
				{
					if (preset.dayLayers.list.Length > 0)
					{
						return(true);
					}
				}
			}

			return(false);
		}

		public bool UsesMask()
		{
			LightmapPresetList presetList = Lighting2D.Profile.lightmapPresets;

			for(int i = 0; i < presetList.list.Length; i++)
			{
				LightmapPreset preset = presetList[i];

				if (preset.type != LightmapPreset.Type.Depth8)
				{
					for(int x = 0; x < preset.dayLayers.list.Length; x++)
					{
						LightmapLayer layer = preset.dayLayers[x];

						if (layer.type != LayerType.ShadowsOnly)
						{
							return(true);
						}
					}
				}
			}

			return(false);
		}

		public bool UsesShadows()
		{
			LightmapPresetList presetList = Lighting2D.Profile.lightmapPresets;

			for(int i = 0; i < presetList.list.Length; i++)
			{
				LightmapPreset preset = presetList[i];

				if (preset.type != LightmapPreset.Type.Depth8)
				{
					for(int x = 0; x < preset.dayLayers.list.Length; x++)
					{
						LightmapLayer layer = preset.dayLayers[x];

						if (layer.type != LayerType.MaskOnly)
						{
							return(true);
						}
					}
				}
			}

			if (UsesDepth())
			{
				return(true);
			}

			return(false);
		}
	}
}