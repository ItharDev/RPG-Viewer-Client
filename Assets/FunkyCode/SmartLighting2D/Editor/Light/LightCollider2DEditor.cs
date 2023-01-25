using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using FunkyCode.LightingSettings;
using FunkyCode.LightSettings;

namespace FunkyCode
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LightCollider2D))]
	public class LightCollider2DEditor : Editor
	{
		private LightCollider2D lightCollider2D;

		private SerializedProperty shadowType;
		private SerializedProperty shadowLayer;
		private SerializedProperty shadowDistance;
		private SerializedProperty shadowDistanceMin;
		private SerializedProperty shadowDistanceMax;
		private SerializedProperty shadowTranslucency;
		
		private SerializedProperty maskType;
		private SerializedProperty maskLayer;
		private SerializedProperty maskLit;
		private SerializedProperty maskLitCustom;
		private SerializedProperty maskPivot;

		private SerializedProperty depthType;
		private SerializedProperty depthOrder;

		public bool UsesShadows()
		{
			LightPreset[] presets = Lighting2D.Profile.lightPresets.Get();

			for(int i = 0; i < presets.Length; i++)
			{
				LightPreset lightPreset = presets[i];

				LayerSetting[] layerSettings = lightPreset.layerSetting.Get();

				for(int x = 0; x < layerSettings.Length; x++)
				{
					LayerSetting setting = layerSettings[x];

					if (setting.type != LightLayerType.MaskOnly)
					{
						return(true);
					}
				}
			}

			return(false);
		}

		public bool UsesMasks()
		{
			LightPreset[] presets = Lighting2D.Profile.lightPresets.Get();

			for(int i = 0; i < presets.Length; i++)
			{
				LightPreset lightPreset = presets[i];

				LayerSetting[] layerSettings = lightPreset.layerSetting.Get();

				for(int x = 0; x < layerSettings.Length; x++)
				{
					LayerSetting setting = layerSettings[x];

					if (setting.type != LightLayerType.ShadowOnly)
					{
						return(true);
					}
				}
			}

			return(false);
		}

		private void InitProperties()
		{
			shadowType = serializedObject.FindProperty("shadowType");
			shadowLayer = serializedObject.FindProperty("shadowLayer");
			shadowDistance = serializedObject.FindProperty("shadowDistance");
			shadowDistanceMin = serializedObject.FindProperty("shadowDistanceMin");
			shadowDistanceMax = serializedObject.FindProperty("shadowDistanceMax");
			shadowTranslucency = serializedObject.FindProperty("shadowTranslucency");
			
			maskType = serializedObject.FindProperty("maskType");
			maskLayer = serializedObject.FindProperty("maskLayer");
			maskLit = serializedObject.FindProperty("maskLit");
			maskLitCustom = serializedObject.FindProperty("maskLitCustom");
			maskPivot = serializedObject.FindProperty("maskPivot");

			depthType = serializedObject.FindProperty("depthType");
			depthOrder = serializedObject.FindProperty("depthOrder");
		}

		private void OnEnable()
		{
			lightCollider2D = target as LightCollider2D;

			InitProperties();
			
			Undo.undoRedoPerformed += RefreshAll;
		}

		internal void OnDisable()
		{
			Undo.undoRedoPerformed -= RefreshAll;
		}

		void RefreshAll()
		{
			LightCollider2D.ForceUpdateAll();
		}

		override public void OnInspectorGUI()
		{
			if (lightCollider2D == null)
			{
				return;
			}

			// Warning
			// Debug.Log(lightCollider2D.mainShape.spriteShape.GetOriginalSprite().packingMode);
			
			// Shadow Properties

			if (UsesShadows())
			{
				EditorGUILayout.PropertyField(shadowType, new GUIContent ("Shadow Type"));

				if (shadowType.intValue != (int)LightCollider2D.ShadowType.None)
				{
					EditorGUI.BeginDisabledGroup(shadowType.intValue == (int)LightCollider2D.ShadowType.None);

					shadowLayer.intValue = EditorGUILayout.Popup("Shadow Layer (Collider)", shadowLayer.intValue, Lighting2D.Profile.layers.colliderLayers.GetNames());

					EditorGUILayout.PropertyField(shadowDistance, new GUIContent ("Shadow Distance"));

					if (shadowDistance.intValue > 0)
					{
						float minV = shadowDistanceMin.floatValue;
						float maxV = shadowDistanceMax.floatValue;

						float min = (float)Mathf.Round(minV * 100f) / 100f;
						float max = (float)Mathf.Round(maxV * 100f) / 100f;

						EditorGUILayout.MinMaxSlider("Shadow Length (" + min + ", " + max +")", ref minV, ref maxV, 0, 10);

						shadowDistanceMin.floatValue = minV;
						shadowDistanceMax.floatValue = maxV;
					}

					//EditorGUILayout.PropertyField(shadowDistanceMin, new GUIContent ("Shadow Distance (Min)"));

					//EditorGUILayout.PropertyField(shadowDistanceMax, new GUIContent ("Shadow Distance (Max)"));

					EditorGUILayout.PropertyField(shadowTranslucency, new GUIContent ("Shadow Translucency"));

					EditorGUI.EndDisabledGroup();
				}
			}

			EditorGUILayout.Space();

			// Mask Properties

			if (UsesMasks())
			{
				EditorGUILayout.PropertyField(maskType, new GUIContent ("Mask Type"));

				if (maskType.intValue != (int)LightCollider2D.MaskType.None)
				{
					EditorGUI.BeginDisabledGroup(maskType.intValue == (int)LightCollider2D.MaskType.None);

					maskLayer.intValue = EditorGUILayout.Popup("Mask Layer (Collider)", maskLayer.intValue, Lighting2D.Profile.layers.colliderLayers.GetNames());
					
					if (lightCollider2D.maskLit == LightSettings.MaskLit.Custom)
					{
						EditorGUILayout.Space();

						EditorGUILayout.PropertyField(maskLit, new GUIContent ("Mask Lit"));

						EditorGUILayout.PropertyField(maskLitCustom, new GUIContent ("Mask Lit Custom"));

						EditorGUILayout.Space();
					}
						else
					{
						EditorGUILayout.PropertyField(maskLit, new GUIContent ("Mask Lit"));
					}
					
					EditorGUILayout.PropertyField(maskPivot, new GUIContent ("Mask Pivot"));

					EditorGUI.EndDisabledGroup();

					if (lightCollider2D.maskType == LightCollider2D.MaskType.BumpedSprite || lightCollider2D.maskType == LightCollider2D.MaskType.BumpedMeshRenderer)
					{
						GUIBumpMapMode.Draw(serializedObject, lightCollider2D);
					}
				}
	
				EditorGUILayout.Space();
			}
	
			serializedObject.ApplyModifiedProperties();

			// Update

			if (GUILayout.Button("Update"))
			{
				SpriteExtension.PhysicsShapeManager.Clear();

				foreach(UnityEngine.Object target in targets)
				{
					LightCollider2D lightCollider2D = target as LightCollider2D;

					lightCollider2D.Initialize();
				}

				LightingManager2D.ForceUpdate();
			}

			if (GUI.changed) 
			{
				foreach(UnityEngine.Object target in targets)
				{
					LightCollider2D lightCollider2D = target as LightCollider2D;
					lightCollider2D.Initialize();
					lightCollider2D.UpdateNearbyLights();

					if (!EditorApplication.isPlaying)
					{
						EditorUtility.SetDirty(target);
					}
				}

				if (!EditorApplication.isPlaying)
				{
					EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
				}

				LightingManager2D.ForceUpdate();
			}
		}
	}
}