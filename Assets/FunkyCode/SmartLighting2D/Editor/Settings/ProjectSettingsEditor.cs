using UnityEngine;
using UnityEditor;
using FunkyCode.LightingSettings;

namespace FunkyCode
{
    public class ProjectSettingsEditor
    {
        static public void Draw()
        {
            EditorGUI.BeginChangeCheck ();

            LightingSettings.ProjectSettings projectSettings = Lighting2D.ProjectSettings;

            projectSettings.Profile = (LightingSettings.Profile)EditorGUILayout.ObjectField("Default Profile", projectSettings.Profile, typeof(LightingSettings.Profile), true);

            EditorGUILayout.Space();

            projectSettings.renderingMode = (RenderingMode)EditorGUILayout.EnumPopup("Rendering Mode", projectSettings.renderingMode);   
                
            EditorGUILayout.Space();

            projectSettings.colorSpace = (LightingSettings.ColorSpace)EditorGUILayout.EnumPopup("Color Space", projectSettings.colorSpace);

            EditorGUILayout.Space();
            
            projectSettings.shaderPreview = (LightingSettings.ShaderPreview)EditorGUILayout.EnumPopup("Shader Preview", projectSettings.shaderPreview);

            if (projectSettings.shaderPreview == LightingSettings.ShaderPreview.Enabled)
			{
                EditorGUILayout.Space();
            
				EditorGUILayout.HelpBox("Shader Preview Enabled", MessageType.Warning);
            }

            EditorGUILayout.Space();

            projectSettings.managerInstance = (LightingSettings.ManagerInstance)EditorGUILayout.EnumPopup("Manager Instance", projectSettings.managerInstance);

            projectSettings.managerInternal = (LightingSettings.ManagerInternal)EditorGUILayout.EnumPopup("Manager Internal", projectSettings.managerInternal);

            EditorGUILayout.Space();

            projectSettings.MaxLightSize = EditorGUILayout.IntSlider("Max Light Size", projectSettings.MaxLightSize, 10, 1000);

            projectSettings.materialOffScreen = (MaterialOffScreen)EditorGUILayout.EnumPopup("Material Off-Screen", projectSettings.materialOffScreen);

            EditorGUILayout.Space();

            GizmosView.Draw(projectSettings);

            EditorGUILayout.Space();

            EditorView.Draw(projectSettings);

            EditorGUILayout.Space();

            Chunks.Draw(projectSettings);

            EditorGUI.EndChangeCheck ();

            if (GUI.changed) {
                LightingManager2D.ForceUpdate();
                Lighting2D.UpdateByProfile(projectSettings.Profile);

                EditorUtility.SetDirty(projectSettings);
            }
        }


        public class Chunks {
            public static void Draw(LightingSettings.ProjectSettings mainProfile) {
                bool foldout = GUIFoldoutHeader.Begin("Chunks", mainProfile.chunks);

                if (foldout == false) {
                    GUIFoldoutHeader.End();
                    return;
                }

                EditorGUI.indentLevel++;   

                EditorGUILayout.Space();


                
                mainProfile.chunks.enabled = EditorGUILayout.Toggle("Enable", mainProfile.chunks.enabled);

                mainProfile.chunks.chunkSize = EditorGUILayout.IntSlider("Chunk Size", mainProfile.chunks.chunkSize, 10, 100);


                EditorGUI.indentLevel--;

                GUIFoldoutHeader.End();
            }
        }

        public class GizmosView
        {
            public static void Draw(LightingSettings.ProjectSettings mainProfile)
            {
                bool foldout = GUIFoldoutHeader.Begin("Gizmos", mainProfile.gizmos);

                if (!foldout)
                {
                    GUIFoldoutHeader.End();
                    return;
                }

                EditorGUI.indentLevel++;   

                EditorGUILayout.Space();
    
                mainProfile.gizmos.drawGizmos = (EditorDrawGizmos)EditorGUILayout.EnumPopup("Draw Gizmos", mainProfile.gizmos.drawGizmos);

                mainProfile.gizmos.drawGizmosShadowCasters = (EditorShadowCasters)EditorGUILayout.EnumPopup("Gizmos Shadow Casters", mainProfile.gizmos.drawGizmosShadowCasters);

                mainProfile.gizmos.drawGizmosBounds = (EditorGizmosBounds)EditorGUILayout.EnumPopup("Gizmos Bounds", mainProfile.gizmos.drawGizmosBounds);

                mainProfile.gizmos.drawGizmosChunks = (EditorChunks)EditorGUILayout.EnumPopup("Gizmos Chunks", mainProfile.gizmos.drawGizmosChunks);

                mainProfile.gizmos.drawIcons = (EditorIcons)EditorGUILayout.EnumPopup("Gizmos Icons", mainProfile.gizmos.drawIcons);

                EditorGUI.indentLevel--;

                GUIFoldoutHeader.End();
            }
        }

        

        public class EditorView
        {
            public static void Draw(LightingSettings.ProjectSettings mainProfile)
            {
                bool foldout = GUIFoldoutHeader.Begin("Editor View (Overlay)", mainProfile.editorView);

                if (!foldout)
                {
                    GUIFoldoutHeader.End();
                    return;
                }

                EditorGUI.indentLevel++;   

                EditorGUILayout.Space();

                mainProfile.editorView.gameViewLayer = EditorGUILayout.LayerField("Game Layer (Default)", mainProfile.editorView.gameViewLayer);

                mainProfile.editorView.sceneViewLayer = EditorGUILayout.LayerField("Scene Layer (Default)", mainProfile.editorView.sceneViewLayer);

                EditorGUI.indentLevel--;

                GUIFoldoutHeader.End();
            }
        }
    }
}
