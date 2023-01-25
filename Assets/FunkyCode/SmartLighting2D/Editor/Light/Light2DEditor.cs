using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using FunkyCode.LightingSettings;
using FunkyCode.LightSettings;
using FunkyCode.Utilities;

namespace FunkyCode
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Light2D))]
	public class Light2DEditor : Editor
	{
		private Light2D light2D;

		private SerializedProperty lightType;

		private SerializedProperty lightPresetId;
		private SerializedProperty eventPresetId;
		private SerializedProperty lightLayer;
		private SerializedProperty occlusionLayer;
		private SerializedProperty translucentLayer;
		private SerializedProperty translucentPresetId;

		private SerializedProperty color;

		private SerializedProperty size;
		private SerializedProperty spotAngleInner;
		private SerializedProperty spotAngleOuter;

		private SerializedProperty outerAngle;
		private SerializedProperty shadowDistanceClose;
		private SerializedProperty shadowDistanceFar;

		private SerializedProperty maskTranslucencyType;
		private SerializedProperty maskTranslucencyStrength;

		private SerializedProperty textureSize;

		private SerializedProperty lightSprite;
		private SerializedProperty spriteFlipX;
		private SerializedProperty spriteFlipY;
		private SerializedProperty sprite;

		private SerializedProperty freeFormPoints;
		private SerializedProperty freeFormFalloff;
		private SerializedProperty freeFormPoint;
		private SerializedProperty freeFormFalloffStrength;

		private SerializedProperty whenInsideCollider;

		private bool foldoutSprite = false;
		private bool foldoutBumpMap = false;
		private bool foldoutFreeForm = false;
		private bool foldoutTranslucency = false;

		private void InitProperties()
		{
			lightType = serializedObject.FindProperty("lightType");

			lightPresetId = serializedObject.FindProperty("lightPresetId");
			eventPresetId = serializedObject.FindProperty("eventPresetId");

			lightLayer = serializedObject.FindProperty("lightLayer");
			occlusionLayer = serializedObject.FindProperty("occlusionLayer");
			translucentLayer = serializedObject.FindProperty("translucentLayer");

			translucentPresetId = serializedObject.FindProperty("translucentPresetId");

			color = serializedObject.FindProperty("color");

			size = serializedObject.FindProperty("size");
			spotAngleInner = serializedObject.FindProperty("spotAngleInner");
			spotAngleOuter = serializedObject.FindProperty("spotAngleOuter");
			
			outerAngle = serializedObject.FindProperty("outerAngle");

			shadowDistanceClose = serializedObject.FindProperty("shadowDistanceClose");
			shadowDistanceFar = serializedObject.FindProperty("shadowDistanceFar");

			textureSize = serializedObject.FindProperty("textureSize");

			lightSprite = serializedObject.FindProperty("lightSprite");
			spriteFlipX = serializedObject.FindProperty("spriteFlipX");
			spriteFlipY = serializedObject.FindProperty("spriteFlipY");
			sprite = serializedObject.FindProperty("sprite");

			
			
			maskTranslucencyType = serializedObject.FindProperty("maskTranslucencyQuality");
			maskTranslucencyStrength = serializedObject.FindProperty("maskTranslucencyStrength");

			whenInsideCollider = serializedObject.FindProperty("whenInsideCollider");

			freeFormPoints = serializedObject.FindProperty("freeFormPoints.points");

			freeFormFalloff = serializedObject.FindProperty("freeFormFalloff");

			freeFormPoint = serializedObject.FindProperty("freeFormPoint");

			freeFormFalloffStrength = serializedObject.FindProperty("freeFormFalloffStrength");
		}

		private void OnEnable()
		{
			light2D = target as Light2D;

			InitProperties();
			
			Undo.undoRedoPerformed += RefreshAll;
		}

		internal void OnDisable()
		{
			Undo.undoRedoPerformed -= RefreshAll;
		}

		void RefreshAll()
		{
			Light2D.ForceUpdateAll();
		}

		private void DrawPoints(List<Vector2> points)
		{
			for(int i = 0; i < points.Count; i++)
			{
				Vector3 point = points[i];

				point.z = light2D.transform.position.z;

				Vector3 nextPoint = points[(i + 1) % points.Count];

				point.x += light2D.transform2D.position.x;
				point.y += light2D.transform2D.position.y;

				nextPoint.x += light2D.transform2D.position.x;
				nextPoint.y += light2D.transform2D.position.y;

				Handles.DrawLine(point, nextPoint);
			}
		}

		public Camera GetSceneCamera()
		{
			UnityEditor.SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;
			
			Camera camera = null;

			if (sceneView != null)
			{
				camera = sceneView.camera;

				if (!camera.orthographic)
				{
					camera = null;
				}
			}

			return(camera);
		}

		private bool OnScene_FreeForm()
		{
			Camera camera = GetSceneCamera();

			if (camera == null)
			{
				return(false);
			}

			bool changed = true;

			float cameraSize = camera.orthographicSize;

			Handles.color = new Color(1, 0.4f, 0);	

			List<Vector2> points = light2D.freeFormPoints.points;

			bool intersect = false;

			for(int i = 0; i < points.Count; i++)
			{
				Vector3 point = points[i];

				point.z = light2D.transform.position.z;

				Vector3 nextPoint = points[(i + 1) % points.Count];

				nextPoint.z = light2D.transform.position.z;

				point.x += light2D.transform2D.position.x;
				point.y += light2D.transform2D.position.y;

				nextPoint.x += light2D.transform2D.position.x;
				nextPoint.y += light2D.transform2D.position.y;

				Handles.DrawLine(point, nextPoint);

				Vector3 result = Handles.FreeMoveHandle(point, Quaternion.identity, 0.05f * cameraSize, Vector2.zero, Handles.CylinderHandleCap);

				if (point != result)
				{
					result.x -= light2D.transform2D.position.x;
					result.y -= light2D.transform2D.position.y;

					List<Vector2> cPoints = new List<Vector2>(points);

					cPoints[i] = result;

					if (Utilities.Math2D.PolygonIntersectItself(cPoints))
					{
						intersect = true;
					}
						else
					{
						points[i] = result;

						changed = true;
					}
				}
			}

			if (!intersect)
			{
				DrawPoints(points);
			}

			return(changed);
		}

		private bool OnScene_Point()
		{
			Camera camera = GetSceneCamera();

			if (camera == null)
			{
				return(false);
			}

			bool changed = false;

			Handles.color = new Color(1, 0.4f, 0);	

			float cameraSize = camera.orthographicSize;

			Vector3 point = light2D.transform.position;

			float rotation = (light2D.transform.localRotation.eulerAngles.z + 90) * Mathf.Deg2Rad;

			point.x += Mathf.Cos(rotation) * light2D.size;
			point.y += Mathf.Sin(rotation) * light2D.size;
			
			Vector3 result = Handles.FreeMoveHandle(point, Quaternion.identity, 0.05f * cameraSize, Vector2.zero, Handles.CylinderHandleCap);

			float moveDistance = Vector2.Distance(point, result);

			if (moveDistance > 0)
			{
				float newSize = Vector2.Distance(result, light2D.transform2D.position);

				light2D.size = newSize;

				changed = true;
			}

			float originAngle = 90f + (int)(Mathf.Atan2 (light2D.transform.position.y - point.y, light2D.transform.position.x - point.x) * Mathf.Rad2Deg);

			float rotateAngle = 90f + (int)(Mathf.Atan2 (light2D.transform.position.y - result.y, light2D.transform.position.x - result.x) * Mathf.Rad2Deg);

			rotateAngle = Math2D.NormalizeRotation(rotateAngle);
			originAngle = Math2D.NormalizeRotation(originAngle);

			if (Mathf.Abs(rotateAngle - originAngle) > 0.001f)
			{
				Quaternion QRotation = light2D.transform.localRotation;

				Vector3 vRotation = QRotation.eulerAngles;

				vRotation.z = rotateAngle;

				light2D.transform.localRotation = Quaternion.Euler(vRotation);

				changed = true;
			}
			
			Vector3 innerPoint = light2D.transform.position;

			float innerValue = light2D.spotAngleInner / 180 - 1;

			innerPoint.x -= Mathf.Cos(rotation) * light2D.size * innerValue;

			innerPoint.y -= Mathf.Sin(rotation) * light2D.size * innerValue;

			Handles.color = new Color(1f, 0.5f, 0.5f);
	
			Vector3 innerHandle = Handles.FreeMoveHandle(innerPoint, Quaternion.identity, 0.05f * cameraSize, Vector2.zero, Handles.CylinderHandleCap);

			if (Vector2.Distance(innerHandle, innerPoint) > 0.001f)
			{
				float nextInnerAngle = Vector2.Distance(innerHandle, point) / light2D.size;

				nextInnerAngle = Math2D.Range(nextInnerAngle * 180, 0, 360);

				if (Vector2.Distance(innerHandle, light2D.transform.position) > light2D.size)
				{
					float a = Vector2.Distance(innerHandle, light2D.transform.position);
					float b = Vector2.Distance(innerHandle, point);
					
					nextInnerAngle = (b < a) ? 0 : 360;
				}

				float nextOuterAngle = nextInnerAngle + (light2D.spotAngleOuter - light2D.spotAngleInner);
				nextOuterAngle = Math2D.Range(Mathf.Max(nextInnerAngle, nextOuterAngle), 0, 360);

				light2D.spotAngleOuter = nextOuterAngle;

				light2D.spotAngleInner = nextInnerAngle;

				changed = true;
			}

			Handles.color = new Color(0.5f, 0.5f, 1f);

			if (light2D.spotAngleInner < 360)
			{
				Vector3 outerPointLeft = light2D.transform.position;

				float outerValue = (light2D.spotAngleOuter ) * Mathf.Deg2Rad * 0.5f;

				outerPointLeft.x += Mathf.Cos(rotation + outerValue) * light2D.size;

				outerPointLeft.y += Mathf.Sin(rotation + outerValue) * light2D.size;

				Vector3 outerHandleLeft = Handles.FreeMoveHandle(outerPointLeft, Quaternion.identity, 0.05f * cameraSize, Vector2.zero, Handles.CylinderHandleCap);

				float transformRotation = light2D.transform.rotation.eulerAngles.z;

				if (Vector2.Distance(outerPointLeft, outerHandleLeft) > 0.001f)
				{
					originAngle = 90f + (int)(Mathf.Atan2 (light2D.transform.position.y - outerPointLeft.y, light2D.transform.position.x - outerPointLeft.x) * Mathf.Rad2Deg);
					originAngle -= transformRotation;
					originAngle = Math2D.NormalizeRotation(originAngle);
					
					rotateAngle = 90f + (int)(Mathf.Atan2 (light2D.transform.position.y - outerHandleLeft.y, light2D.transform.position.x - outerHandleLeft.x) * Mathf.Rad2Deg);
					rotateAngle -= transformRotation;
					rotateAngle = Math2D.NormalizeRotation(rotateAngle);

					light2D.spotAngleOuter = rotateAngle * 2f;
					light2D.spotAngleOuter = Math2D.Range(Mathf.Max(light2D.spotAngleInner, light2D.spotAngleOuter), 0, 360);

					changed = true;
				}

				Vector3 outerPointRight = light2D.transform.position;

				outerPointRight.x += Mathf.Cos(rotation - outerValue) * light2D.size;

				outerPointRight.y += Mathf.Sin(rotation - outerValue) * light2D.size;

				Vector3 outerHandleRight = Handles.FreeMoveHandle(outerPointRight, Quaternion.identity, 0.05f * cameraSize, Vector2.zero, Handles.CylinderHandleCap);

				if (Vector2.Distance(outerPointRight, outerHandleRight) > 0.01f)
				{
					originAngle = -90f - (int)(Mathf.Atan2 (light2D.transform.position.y - outerPointRight.y, light2D.transform.position.x - outerPointRight.x) * Mathf.Rad2Deg);
					originAngle += transformRotation;
					originAngle = Math2D.NormalizeRotation(originAngle);
					
					rotateAngle = -90f - (int)(Mathf.Atan2 (light2D.transform.position.y - outerHandleRight.y, light2D.transform.position.x - outerHandleRight.x) * Mathf.Rad2Deg);
					rotateAngle += transformRotation;
					rotateAngle = Math2D.NormalizeRotation(rotateAngle);

					light2D.spotAngleOuter = rotateAngle * 2f;

					light2D.spotAngleOuter = Math2D.Range(Mathf.Max(light2D.spotAngleInner, light2D.spotAngleOuter), 0, 360);

					changed = true;
				}
			}

			return(changed);
		}

		private void OnSceneGUI()
		{
			if (light2D == null)
			{
				return;
			}

			bool changed = false;

			switch(light2D.lightType)
			{
				case Light2D.LightType.FreeForm:
					changed = OnScene_FreeForm();
				break;

				case Light2D.LightType.Point:
				case Light2D.LightType.Sprite:
					changed = OnScene_Point();
				break;

			}

			if (changed)
			{
				light2D.ForceUpdate();

				if (!EditorApplication.isPlaying)
				{
					EditorUtility.SetDirty(target);
	
					EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
				}
			}
		}

		override public void OnInspectorGUI()
		{
			if (light2D == null)
			{
				return;
			}

			EditorGUILayout.PropertyField(lightType, new GUIContent("Type"));

			EditorGUILayout.Space();

			lightPresetId.intValue = EditorGUILayout.Popup("Light Preset", lightPresetId.intValue, Lighting2D.Profile.lightPresets.GetPresetNames());
			
			eventPresetId.intValue = EditorGUILayout.Popup("Event Preset", eventPresetId.intValue, Lighting2D.Profile.eventPresets.GetBufferLayers());

			EditorGUILayout.Space();

			int value = lightLayer.intValue + 1;

			value = EditorGUILayout.Popup("Shadows (Light)", value, Lighting2D.Profile.layers.lightLayers.GetOcclusionNames());		

			lightLayer.intValue = value - 1;

			occlusionLayer.intValue = EditorGUILayout.Popup("Occlusion (Light)", occlusionLayer.intValue, Lighting2D.Profile.layers.lightLayers.GetOcclusionNames());		

			translucentLayer.intValue = EditorGUILayout.Popup("Translucency (Light)", translucentLayer.intValue, Lighting2D.Profile.layers.lightLayers.GetTranslucencyNames());		
		
			EditorGUILayout.Space();
					
			light2D.applyRotation = (Light2D.Rotation)EditorGUILayout.EnumPopup("Rotation", light2D.applyRotation);

			EditorGUILayout.PropertyField(whenInsideCollider, new GUIContent ("When Inside Collider"));
			
			bool customSize = Lighting2D.Profile.qualitySettings.lightTextureSize == LightingSourceTextureSize.Custom;

			EditorGUI.BeginDisabledGroup(!customSize);

			if (customSize)
			{
				textureSize.intValue = EditorGUILayout.Popup("Resolution", (int)light2D.textureSize, LightingSettings.QualitySettings.LightingSourceTextureSizeArray);
			}
				else
			{
				EditorGUILayout.Popup("Resolution", (int)Lighting2D.Profile.qualitySettings.lightTextureSize, LightingSettings.QualitySettings.LightingSourceTextureSizeArray);
			}

			EditorGUI.EndDisabledGroup();
		
			EditorGUILayout.Space();

			Color colorValue = EditorGUILayout.ColorField(new GUIContent("Color"), color.colorValue, true, true, true);
	
			colorValue.a = EditorGUILayout.Slider("Alpha", colorValue.a, 0, 1);

			color.colorValue = colorValue;

			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(light2D.lightType == Light2D.LightType.FreeForm);

			size.floatValue = EditorGUILayout.Slider("Size", size.floatValue, 0.1f, Lighting2D.ProjectSettings.MaxLightSize);

			EditorGUI.EndDisabledGroup();

			// LIGHT PRESET PROPERTIES ///////////////////////////////////////

			float inner = spotAngleInner.floatValue;
			float outer = spotAngleOuter.floatValue;

			double roundInner = System.Math.Round(inner, 2);
			double roundOuter = System.Math.Round(outer, 2);

			if (light2D.lightLayer >= 0 || light2D.translucentLayer > 0|| light2D.occlusionLayer > 0)
			{
				switch(light2D.lightType)
				{
					case Light2D.LightType.Sprite:

						EditorGUILayout.MinMaxSlider("Spot Angle (" + roundInner + ", " + roundOuter + ")", ref inner, ref outer, 0f, 360f);

						spotAngleInner.floatValue = inner;
						spotAngleOuter.floatValue = outer;

					break;

					case Light2D.LightType.Point:

						EditorGUILayout.MinMaxSlider("Spot Angle (" + roundInner + ", " + roundOuter + ")", ref inner, ref outer, 0f, 360f);

						spotAngleInner.floatValue = inner;
						spotAngleOuter.floatValue = outer;

						light2D.lightStrength = EditorGUILayout.Slider("Falloff", light2D.lightStrength, 0, 1);

					break;
				}
			}

			if (light2D.lightLayer >= 0 || light2D.translucentLayer > 0)
			{
				EditorGUILayout.Space();

				if (UsesLegacyShadows())
				{
					outerAngle.floatValue = EditorGUILayout.Slider("Soft Legacy Shadows", outerAngle.floatValue, 0, 60);
				}

				if (UsesSoftShadows())
				{
					light2D.coreSize = EditorGUILayout.Slider("Soft Shadows", light2D.coreSize, 0.1f, 10f);

					light2D.falloff = EditorGUILayout.Slider("Soft Falloff", light2D.falloff, 0, 10f);
				}

				if (UsesSoftDefaultShadows())
				{
					light2D.lightRadius = EditorGUILayout.Slider("Soft Radius", light2D.lightRadius, 0.1f, 10f);
				}

				if (UsesDefaultShadows())
				{
					EditorGUILayout.PropertyField(shadowDistanceClose, new GUIContent ("Soft Distance Close"));

					EditorGUILayout.PropertyField(shadowDistanceFar, new GUIContent ("Soft Distance Far"));
				}
			}

			//////////////////////////////////////////////////////////////////////////////

			EditorGUILayout.Space();

			switch(light2D.lightType)
			{
				case Light2D.LightType.Sprite:

					foldoutSprite = EditorGUILayout.Foldout(foldoutSprite, "Sprite", true);

					if (foldoutSprite)
					{
						EditorGUI.indentLevel++;

						EditorGUILayout.PropertyField(lightSprite, new GUIContent ("Type"));

						//script.lightSprite = (Light2D.LightSprite)EditorGUILayout.EnumPopup("Light Sprite", script.lightSprite);
				
						if (light2D.lightSprite == Light2D.LightSprite.Custom)
						{
							EditorGUILayout.PropertyField(spriteFlipX, new GUIContent ("Flip X"));
							EditorGUILayout.PropertyField(spriteFlipY, new GUIContent ("Flip Y"));
							
							sprite.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField("", sprite.objectReferenceValue, typeof(Sprite), true);
						}
							else
						{
							if (light2D.sprite != Light2D.GetDefaultSprite())
							{
								light2D.sprite = Light2D.GetDefaultSprite();
							}
						}
					
						EditorGUI.indentLevel--;
					}

					EditorGUILayout.Space();

				break;

				case Light2D.LightType.FreeForm:

					foldoutFreeForm = EditorGUILayout.Foldout(foldoutFreeForm, "Free Form", true);

					if (foldoutFreeForm)
					{
						EditorGUI.indentLevel++;

						freeFormPoint.floatValue = EditorGUILayout.Slider("Point", freeFormPoint.floatValue, 0, 1);

						EditorGUILayout.PropertyField(freeFormFalloff, new GUIContent("Falloff"));

						freeFormFalloffStrength.floatValue = EditorGUILayout.Slider("Falloff Strength", freeFormFalloffStrength.floatValue, 0, 1);

						freeFormFalloff.floatValue = Mathf.Max(freeFormFalloff.floatValue, 0);

						EditorGUILayout.PropertyField(freeFormPoints, new GUIContent("Points"));

						EditorGUI.indentLevel--;
					}

					EditorGUILayout.Space();

				break;
			}
		
			if (light2D.translucentLayer > 0)
			{
				foldoutTranslucency = EditorGUILayout.Foldout(foldoutTranslucency, "Translucency", true);

				if (foldoutTranslucency)
				{
					EditorGUI.indentLevel++;

					translucentPresetId.intValue = EditorGUILayout.Popup("Light Preset", translucentPresetId.intValue, Lighting2D.Profile.lightPresets.GetPresetNames());

					EditorGUILayout.PropertyField(maskTranslucencyType, new GUIContent ("Quality"));
					
					maskTranslucencyStrength.floatValue = EditorGUILayout.Slider("Strength", maskTranslucencyStrength.floatValue, 0, 1);
					
					EditorGUI.indentLevel--;
				}

				EditorGUILayout.Space();
			}

			GUIMeshMode.Draw(serializedObject, light2D.meshMode);

			EditorGUILayout.Space();

			if (UsesMasks())
			{
				foldoutBumpMap = EditorGUILayout.Foldout(foldoutBumpMap, "Bump Map", true);

				if (foldoutBumpMap)
				{
					EditorGUI.indentLevel++;

					light2D.bumpMap.intensity = EditorGUILayout.Slider("Intensity", light2D.bumpMap.intensity, 0, 2);
					light2D.bumpMap.depth = EditorGUILayout.Slider("Depth", light2D.bumpMap.depth, 0.1f, 20f);

					EditorGUI.indentLevel--;
				}

				EditorGUILayout.Space();
			}

			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();	

			if (GUI.changed)
			{
				foreach(UnityEngine.Object target in targets)
				{
					if (target == null)
					{
						continue;
					}

					Light2D light2D = target as Light2D;

					if (light2D == null)
					{
						continue;
					}
					
					light2D.ForceUpdate();

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

		public bool UsesSoftShadows()
		{
			LayerSetting[] layerSettings = light2D.GetLightPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.SoftConvex || setting.shadowEffect == LightLayerShadowEffect.SoftConvex)
				{
					return(true);
				}
			}

			layerSettings = light2D.GetTranslucencyPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.SoftConvex || setting.shadowEffect == LightLayerShadowEffect.SoftConvex)
				{
					return(true);
				}
			}

			return(false);
		}
		
		public bool UsesSoftDefaultShadows()
		{
			LayerSetting[] layerSettings = light2D.GetLightPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.Soft)
				{
					return(true);
				}
			}

			layerSettings = light2D.GetTranslucencyPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.Soft)
				{
					return(true);
				}
			}

			return(false);
		}

		public bool UsesMasks()
		{
			LayerSetting[] layerSettings = light2D.GetLightPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.type == LightLayerType.MaskOnly || setting.type == LightLayerType.ShadowAndMask)
				{
					return(true);
				}
			}

			layerSettings = light2D.GetTranslucencyPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.type == LightLayerType.MaskOnly || setting.type == LightLayerType.ShadowAndMask)
				{
					return(true);
				}
			}

			return(false);
		}

		public bool UsesLegacyShadows()
		{
			LayerSetting[] layerSettings = light2D.GetLightPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.LegacyCPU || setting.shadowEffect == LightLayerShadowEffect.LegacyGPU)
				{
					return(true);
				}
			}

			layerSettings = light2D.GetTranslucencyPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.LegacyCPU || setting.shadowEffect == LightLayerShadowEffect.LegacyGPU)
				{
					return(true);
				}
			}

			return(false);
		}

		public bool UsesDefaultShadows()
		{
			LayerSetting[] layerSettings = light2D.GetLightPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.Default)
				{
					return(true);
				}
			}

			layerSettings = light2D.GetTranslucencyPresetLayers();

			for(int i = 0; i < layerSettings.Length; i++)
			{
				LayerSetting setting = layerSettings[i];

				if (setting.shadowEffect == LightLayerShadowEffect.Default)
				{
					return(true);
				}
			}

			return(false);
		}
	}
}
