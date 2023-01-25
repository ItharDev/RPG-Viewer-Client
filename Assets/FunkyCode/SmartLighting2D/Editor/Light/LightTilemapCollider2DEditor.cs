using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using FunkyCode.LightTilemapCollider;

namespace FunkyCode
{
	[CustomEditor(typeof(LightTilemapCollider2D))]
	public class LightTilemapCollider2DEditor : Editor
	{
		override public void OnInspectorGUI()
		{
			LightTilemapCollider2D script = target as LightTilemapCollider2D;

			script.mapType = (MapType)EditorGUILayout.EnumPopup("Tilemap Type", script.mapType);

			EditorGUILayout.Space();

			switch(script.mapType)
			{
				case MapType.UnityRectangle:
					script.rectangle.shadowType = (ShadowType)EditorGUILayout.EnumPopup("Shadow Type", script.rectangle.shadowType);
					
					EditorGUI.BeginDisabledGroup(script.rectangle.shadowType == ShadowType.None);

					script.shadowLayer = EditorGUILayout.Popup("Shadow Layer (Collider)", script.shadowLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());
					
					switch(script.rectangle.shadowType)
					{
						case ShadowType.Grid:
						case ShadowType.SpritePhysicsShape:
							script.shadowTileType = (ShadowTileType)EditorGUILayout.EnumPopup("Shadow Tile Type", script.shadowTileType);
						break;
					}

					script.shadowTranslucency = EditorGUILayout.Slider( "Shadow Translucency", script.shadowTranslucency, 0, 1);

					script.rectangle.shadowOptimization = EditorGUILayout.Toggle("Shadow Optimization", script.rectangle.shadowOptimization);				

					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					script.rectangle.maskType = (MaskType)EditorGUILayout.EnumPopup("Mask Type", script.rectangle.maskType);
					
					EditorGUI.BeginDisabledGroup(script.rectangle.maskType == MaskType.None);

					script.maskLayer = EditorGUILayout.Popup("Mask Layer (Collider)", script.maskLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());

					if (script.rectangle.maskType == MaskType.BumpedSprite)
					{
						GUIBumpMapMode.Draw(serializedObject, script);
					}

					EditorGUI.EndDisabledGroup();

				break;

				case MapType.UnityIsometric:
					
					script.isometric.shadowType = (ShadowType)EditorGUILayout.EnumPopup("Shadow Type", script.isometric.shadowType);
					
					EditorGUI.BeginDisabledGroup(script.isometric.shadowType == ShadowType.None);

					script.shadowLayer = EditorGUILayout.Popup("Shadow Layer (Collider)", script.shadowLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());
					script.shadowTileType = (ShadowTileType)EditorGUILayout.EnumPopup("Shadow Tile Type", script.shadowTileType);
					
					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					script.isometric.maskType = (MaskType)EditorGUILayout.EnumPopup("Mask Type", script.isometric.maskType);
					
					EditorGUI.BeginDisabledGroup(script.isometric.maskType == MaskType.None);

					script.maskLayer = EditorGUILayout.Popup("Mask Layer (Collider)", script.maskLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());

					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					script.isometric.ZasY = EditorGUILayout.Toggle("Z as Y", script.isometric.ZasY);

				break;


				case MapType.UnityHexagon:
					
					script.hexagon.shadowType = (ShadowType)EditorGUILayout.EnumPopup("Shadow Type", script.hexagon.shadowType);
					
					EditorGUI.BeginDisabledGroup(script.hexagon.shadowType == ShadowType.None);

					script.shadowLayer = EditorGUILayout.Popup("Shadow Layer (Collider)", script.shadowLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());
					script.shadowTileType = (ShadowTileType)EditorGUILayout.EnumPopup("Shadow Tile Type", script.shadowTileType);
						
					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					script.hexagon.maskType = (MaskType)EditorGUILayout.EnumPopup("Mask Type", script.hexagon.maskType);
					
					EditorGUI.BeginDisabledGroup(script.hexagon.maskType == MaskType.None);

					script.maskLayer = EditorGUILayout.Popup("Mask Layer (Collider)", script.maskLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());

					EditorGUI.EndDisabledGroup();
				break;

				case MapType.SuperTilemapEditor:
					#if (SUPER_TILEMAP_EDITOR)

					script.superTilemapEditor.shadowTypeSTE = (SuperTilemapEditorSupport.TilemapCollider2D.ShadowType)EditorGUILayout.EnumPopup("Shadow Type", script.superTilemapEditor.shadowTypeSTE);
				
					script.shadowLayer = EditorGUILayout.Popup("Shadow Layer (Collider)", script.shadowLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());
					
					script.superTilemapEditor.shadowOptimization = EditorGUILayout.Toggle("Shadow Optimization", script.superTilemapEditor.shadowOptimization);				
					
					EditorGUILayout.Space();

					script.superTilemapEditor.maskTypeSTE = (SuperTilemapEditorSupport.TilemapCollider2D.MaskType)EditorGUILayout.EnumPopup("Mask Type", script.superTilemapEditor.maskTypeSTE);
					
					EditorGUI.BeginDisabledGroup(script.superTilemapEditor.maskTypeSTE == SuperTilemapEditorSupport.TilemapCollider2D.MaskType.None);
					
					script.maskLayer = EditorGUILayout.Popup("Mask Layer (Collider)", script.maskLayer, Lighting2D.Profile.layers.colliderLayers.GetNames());
					
					if (script.superTilemapEditor.maskTypeSTE == SuperTilemapEditorSupport.TilemapCollider2D.MaskType.BumpedSprite)
					{
						GUIBumpMapMode.Draw(serializedObject, script);
					}
					
					EditorGUI.EndDisabledGroup();

					#endif
				break;
			}

			EditorGUILayout.Space();

			Update(script);

			serializedObject.ApplyModifiedProperties();
			
			if (GUI.changed)
			{
				script.Initialize();

				Light2D.ForceUpdateAll();
				LightingManager2D.ForceUpdate();

				if (!EditorApplication.isPlaying)
				{
					EditorUtility.SetDirty(target);
					EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
				}
			}
		}

		static void Update(LightTilemapCollider2D script)
		{
			if (GUILayout.Button("Update"))
			{
				SpriteExtension.PhysicsShapeManager.Clear();
				
				script.Initialize();

				Light2D.ForceUpdateAll();
				
				LightingManager2D.ForceUpdate();
			}
		}
	}
}