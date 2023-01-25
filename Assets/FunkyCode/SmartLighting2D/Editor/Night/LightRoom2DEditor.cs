using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace FunkyCode
	{
	[CustomEditor(typeof(LightRoom2D))]
	public class LightRoom2DEditor : Editor {
		override public void OnInspectorGUI() {
			LightRoom2D script = target as LightRoom2D;

			script.lightLayer = EditorGUILayout.Popup("Layer (Light)", script.lightLayer, Lighting2D.Profile.layers.lightLayers.GetNames());

			script.shape.type = (LightRoom2D.RoomType)EditorGUILayout.EnumPopup("Room Type", script.shape.type);

			script.color = EditorGUILayout.ColorField("Color", script.color);

			Update(); 

			if (GUI.changed) {
				if (EditorApplication.isPlaying == false) {
					EditorUtility.SetDirty(script);
					EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

					LightingManager2D.ForceUpdate();
				}
			}
		}

		void Update() {
			LightRoom2D script = target as LightRoom2D;

			if (GUILayout.Button("Update")) {
				script.Initialize();
			}
		}
	}
}