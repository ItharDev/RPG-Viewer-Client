using UnityEngine;
using UnityEditor;

namespace FunkyCode
{
    public static class DebuggingEditor
    {
		static bool gameLightmapFoldout = false;
        static bool sceneLightmapFoldout = false;

		static bool lightFoldout = false;
		static bool shaderPassFoldout = false;
        static bool materialPassFoldout = false;

        static bool camerasFoldout = false;

        static bool[] passFoldouts = new bool[9];

        static bool[] overlayFoldouts = new bool[20];

		public static void Debugging()
		{
			EditorGUI.indentLevel++;

            int gameCount = 0;

            foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List)
            {
                if (buffer.cameraSettings.cameraType == CameraSettings.CameraType.SceneView)
                {
                    continue;
                }

                gameCount++;
            }

			gameLightmapFoldout = EditorGUILayout.Foldout(gameLightmapFoldout, "Game Lightmaps (" + gameCount + ")", true);

			if (gameLightmapFoldout)
			{
				EditorGUI.indentLevel++;

                int id = 0;

				foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List)
				{
					CameraSettings cameraSetting = buffer.cameraSettings;

                    if (cameraSetting.cameraType == CameraSettings.CameraType.SceneView)
                    {
                        continue;
                    }

                    id++;

					CameraLightmap cameraLightmap = buffer.cameraLightmap;

					EditorGUILayout.ObjectField("Camera Target (" +  cameraSetting.cameraType + ")", cameraSetting.GetCamera(), typeof(Camera), true);
					EditorGUILayout.Popup("Lightmap Preset (" + cameraLightmap.id + ")", (int)cameraLightmap.presetId, Lighting2D.Profile.lightmapPresets.GetLightmapLayers());
					EditorGUILayout.EnumPopup("Rendering", cameraLightmap.rendering);
					EditorGUILayout.EnumPopup("Render Texture Type", buffer.type);
                    EditorGUILayout.EnumPopup("Render Texture HDR", buffer.hdr);

                    Rect gui = EditorGUILayout.GetControlRect();

                    EditorGUI.ObjectField(gui, "Render Texture", buffer.renderTexture.renderTexture, typeof(RenderTexture), true);

                    overlayFoldouts[id] = EditorGUILayout.Foldout(overlayFoldouts[id], "Overlay (" + cameraLightmap.overlay + ")", true);

                    if (overlayFoldouts[id])
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.EnumPopup("Overlay Material Type", cameraLightmap.overlayMaterial);
                        EditorGUILayout.ObjectField("Overlay Material", buffer.GetMaterial(), typeof(Material), true);
                        EditorGUI.indentLevel--;
                    }
			
					if (buffer.renderTexture != null)
					{
                        Rect guiRect = EditorGUILayout.GetControlRect();

                        Texture texture = buffer.renderTexture.renderTexture;
         
                        float ratio = (float)texture.width / texture.height;

                        float width = 200 * ratio;
                        float height = 200;

                        Rect drawRect = new Rect(guiRect.x + guiRect.width - width, guiRect.y, width, height);

                        EditorGUI.DrawPreviewTexture(drawRect, texture, null);

                              
                        GUIStyle GUIStyle = new GUIStyle();
                        GUIStyle.fontSize = 12;
                        GUIStyle.normal.textColor = Color.white;
                        GUIStyle.alignment = TextAnchor.LowerRight;

                        EditorGUI.LabelField(drawRect, texture.width + "x" + texture.height, GUIStyle);
     
                        GUILayout.Space(height);
					}
	
					EditorGUILayout.Space();
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.Space();
			}

            int sceneCount = 0;

            foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List)
            {
                if (buffer.cameraSettings.cameraType != CameraSettings.CameraType.SceneView)
                {
                    continue;
                }

                sceneCount ++;
            }

            sceneLightmapFoldout = EditorGUILayout.Foldout(sceneLightmapFoldout, "Scene Lightmaps (" + sceneCount + ")", true);

			if (sceneLightmapFoldout)
			{
				EditorGUI.indentLevel++;

                int id = 0;

				foreach(LightMainBuffer2D buffer in LightMainBuffer2D.List)
				{
					CameraSettings cameraSetting = buffer.cameraSettings;

                    if (cameraSetting.cameraType != CameraSettings.CameraType.SceneView)
                    {
                        continue;
                    }

                    id++;

					CameraLightmap cameraLightmap = buffer.cameraLightmap;

					EditorGUILayout.ObjectField("Camera Target (" +  cameraSetting.cameraType + ")", cameraSetting.GetCamera(), typeof(Camera), true);
					EditorGUILayout.Popup("Lightmap Preset (" + cameraLightmap.id + ")", (int)cameraLightmap.presetId, Lighting2D.Profile.lightmapPresets.GetLightmapLayers());
					EditorGUILayout.EnumPopup("Rendering", cameraLightmap.rendering);
					EditorGUILayout.EnumPopup("Render Texture Type", buffer.type);
                    EditorGUILayout.EnumPopup("Render Texture HDR", buffer.hdr);

                    Rect gui = EditorGUILayout.GetControlRect();

                    EditorGUI.ObjectField(gui, "Render Texture", buffer.renderTexture.renderTexture, typeof(RenderTexture), true);  

                    overlayFoldouts[id] = EditorGUILayout.Foldout(overlayFoldouts[id], "Overlay (" + cameraLightmap.overlay + ")", true);

                    if (overlayFoldouts[id])
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.EnumPopup("Overlay Material Type", cameraLightmap.overlayMaterial);
                        EditorGUILayout.ObjectField("Overlay Material", buffer.GetMaterial(), typeof(Material), true);
                        EditorGUI.indentLevel--;
                    }
			
					if (buffer.renderTexture != null)
					{
                        Rect guiRect = EditorGUILayout.GetControlRect();

                        Texture texture = buffer.renderTexture.renderTexture;
         
                        float ratio = (float)texture.width / texture.height;

                        float width = 200 * ratio;
                        float height = 200;

                        Rect drawRect = new Rect(guiRect.x + guiRect.width - width, guiRect.y, width, height);

                        EditorGUI.DrawPreviewTexture(drawRect, texture, null);

                              
                        GUIStyle GUIStyle = new GUIStyle();
                        GUIStyle.fontSize = 12;
                        GUIStyle.normal.textColor = Color.white;
                        GUIStyle.alignment = TextAnchor.LowerRight;

                        EditorGUI.LabelField(drawRect, texture.width + "x" + texture.height, GUIStyle);
     
                        GUILayout.Space(height);
					}
	
					EditorGUILayout.Space();
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.Space();
			}
			
			int taken = 0;

			foreach(LightBuffer2D buffer in LightBuffer2D.List)
			{
				if (!buffer.Free) {
					taken += 1;
				}
			}

			lightFoldout = EditorGUILayout.Foldout(lightFoldout, "Light Shadowmaps (" + taken + "/" + LightBuffer2D.List.Count + ")", true);

			if (lightFoldout)
			{
				EditorGUI.indentLevel++;

				foreach(LightBuffer2D buffer in LightBuffer2D.List)
				{
                    Rect guiRect = EditorGUILayout.GetControlRect();

					if (!buffer.Free)
					{
						EditorGUI.ObjectField(new Rect(guiRect.x, guiRect.y, guiRect.width - 100, guiRect.height), "Source", buffer.Light, typeof(Light2D), true);
					}
                        else
                    {
                        EditorGUI.LabelField(new Rect(guiRect.x, guiRect.y, guiRect.width - 100, guiRect.height), "Source: Free");
                    }

					if (buffer.renderTexture != null)
					{
						// EditorGUILayout.ObjectField("Render Texture", buffer.renderTexture.renderTexture, typeof(Texture), true);

                        Texture texture = buffer.renderTexture.renderTexture;
                        
                        EditorGUI.ObjectField(new Rect(guiRect.x, guiRect.y + 20, guiRect.width - 100, guiRect.height), "Texture", texture, typeof(RenderTexture), true);
                        EditorGUI.LabelField(new Rect(guiRect.x, guiRect.y + 40, guiRect.width, guiRect.height), buffer.name);
                        
                        float width = 100;
                        float height = 100;

                        Rect drawRect = new Rect(guiRect.x + guiRect.width - width, guiRect.y, width, height);

                        EditorGUI.DrawPreviewTexture(drawRect, texture, null);
                        
                        GUIStyle GUIStyle = new GUIStyle();
                        GUIStyle.fontSize = 12;
                        GUIStyle.normal.textColor = Color.white;
                        GUIStyle.alignment = TextAnchor.LowerRight;

                        EditorGUI.LabelField(drawRect, texture.width.ToString(), GUIStyle);

                      
                        GUILayout.Space(height);
					}

					if (buffer.translucencyTexture != null)
					{
                        guiRect = EditorGUILayout.GetControlRect();

                        Texture texture =  buffer.translucencyTexture.renderTexture;
    
                        EditorGUI.LabelField(new Rect(guiRect.x, guiRect.y + 20, guiRect.width, guiRect.height), "Translucency Texture");

                        float ratio = (float)texture.width / texture.height;

                        float width = 100 * ratio;
                        float height = 100;

                        Rect drawRect = new Rect(guiRect.x + guiRect.width - width, guiRect.y, width, height);

                        EditorGUI.DrawPreviewTexture(drawRect, texture, null);
                        
                        GUIStyle GUIStyle = new GUIStyle();
                        GUIStyle.fontSize = 15;
                        GUIStyle.normal.textColor = Color.white;
                        GUIStyle.alignment = TextAnchor.LowerRight;

                        EditorGUI.LabelField(drawRect, texture.width.ToString(), GUIStyle);
                        
                        GUILayout.Space(height);
                    }

					/*

					if (buffer.collisionTextureBlur != null)
					{
						EditorGUILayout.ObjectField("Collision Texture (Post)", buffer.collisionTextureBlur.renderTexture, typeof(Texture), true);
					}

					*/

					if (buffer.freeFormTexture != null)
					{
						EditorGUILayout.ObjectField("Free Form Texture", buffer.freeFormTexture.renderTexture, typeof(Texture), true);
					}

					EditorGUILayout.Space();
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.Space();
			}

            int passCount = 0;

            for(int i = 1; i <= 8; i++)
            {
                if (NameType(i) > 0)
                {
                    passCount++;
                }
            }
			
			shaderPassFoldout = EditorGUILayout.Foldout(shaderPassFoldout, "Shader Passes (" + passCount + ")", true);

			if (shaderPassFoldout)
			{
				EditorGUI.indentLevel++;

                for(int i = 1; i <= 8; i++)
                {
                    if (NameType(i) > 0)
                    {
                        DrawPass(i);
                    }
                }

				EditorGUI.indentLevel--;
			}

            materialPassFoldout = EditorGUILayout.Foldout(materialPassFoldout, "Material Passes (" + MaterialSystem.Count + ")", true);

			if (materialPassFoldout)
			{
                EditorGUI.indentLevel++;

                for(int i = 0; i < MaterialSystem.Count; i++)
                {
                    MaterialPass pass = MaterialSystem.materialPasses[i];

                    EditorGUILayout.LabelField("[PassId: " + pass.passId + "] -> " + pass.material.name + " " + pass.material.shader.name);
                }

                EditorGUI.indentLevel--;
            }

            camerasFoldout = EditorGUILayout.Foldout(camerasFoldout, "Cameras (" + CameraTransform.List.Count + ")", true);

			if (camerasFoldout)
			{
				EditorGUI.indentLevel++;

                foreach(CameraTransform camera in CameraTransform.List)
                {
                    EditorGUILayout.LabelField(camera.Camera.name);
                }
                
				EditorGUI.indentLevel--;
			}

            EditorGUILayout.Space();

			EditorGUI.indentLevel--;


			/*

			EditorGUILayout.Foldout(true, "Internal");

			EditorGUI.indentLevel++;

			EditorGUILayout.ObjectField("Mask Material", Lighting2D.materials.mask.GetMask(), typeof(Material), true);

			EditorGUI.indentLevel--;
			*/
		}

        public static void DrawPass(int id)
        {
            int typeId = NameType(id);

            string nameType = "";

            switch(typeId) {
                case 0:
                    nameType = "NULL";
                break;

                case 1:
                    nameType = "Game";
                break;

                case 2:
                    nameType = "Scene";
                break;

                case 3:
                    nameType = "Game & Scene";
                break;
            }
            
            passFoldouts[id] = EditorGUILayout.Foldout(passFoldouts[id], "Pass " + id + " (" + nameType+ ")", true);

            if (!passFoldouts[id])
            {
                return;
            }
            
            EditorGUI.indentLevel++;

            if (typeId == 1 || typeId == 3)
            {
                Rect guiRect = EditorGUILayout.GetControlRect();

                Vector4 rect = Shader.GetGlobalVector("_GameRect" + id);

                float rot = Shader.GetGlobalFloat("_GameRotation" + id);
               
                EditorGUI.LabelField(new Rect(guiRect.x, guiRect.y, guiRect.width, guiRect.height), "Game Vector " + Mathf.Ceil(rect.x) + " " + Mathf.Ceil(rect.y) + " " + Mathf.Ceil(rect.z) + " " + Mathf.Ceil(rect.w) + " " + rot);
                
                Texture texture = Shader.GetGlobalTexture("_GameTexture" + id);

                if (texture != null)
                {
                    float ratio = (float)texture.width / texture.height;

                    float width = 100 * ratio;
                    float height = 100;

                    Rect drawRect = new Rect(guiRect.x + guiRect.width - width, guiRect.y, width, height);

                    EditorGUI.DrawPreviewTexture(drawRect, texture, null);
                   
                    GUIStyle GUIStyle = new GUIStyle();
                    GUIStyle.fontSize = 12;
                    GUIStyle.normal.textColor = Color.white;
                    GUIStyle.alignment = TextAnchor.LowerRight;

                    EditorGUI.LabelField(drawRect, texture.width + "x" + texture.height, GUIStyle);

                    GUILayout.Space(height);
                }
            }

            if (typeId == 2 || typeId == 3)
            {
                Rect guiRect = EditorGUILayout.GetControlRect();

                Vector4 rect = Shader.GetGlobalVector("_SceneRect" + id);

                float rot = Shader.GetGlobalFloat("_SceneRotation" + id);

                EditorGUI.LabelField(new Rect(guiRect.x, guiRect.y, guiRect.width, guiRect.height), "Scene Vector " + Mathf.Ceil(rect.x) + " " + Mathf.Ceil(rect.y) + " " + Mathf.Ceil(rect.z) + " " + Mathf.Ceil(rect.w) + " " + rot);
                
                Texture texture = Shader.GetGlobalTexture("_SceneTexture" + id);

                if (texture != null)
                {
                    float ratio = (float)texture.width / texture.height;

                    float width = 100 * ratio;
                    float height = 100;

                    Rect drawRect = new Rect(guiRect.x + guiRect.width - width, guiRect.y, width, height);
            
                    EditorGUI.DrawPreviewTexture(drawRect, texture, null);

                    GUIStyle GUIStyle = new GUIStyle();
                    GUIStyle.fontSize = 12;
                    GUIStyle.normal.textColor = Color.white;
                    GUIStyle.alignment = TextAnchor.LowerRight;

                    EditorGUI.LabelField(drawRect, texture.width + "x" + texture.height, GUIStyle);

                    GUILayout.Space(height);
                }
                
                EditorGUI.indentLevel--;	
            }       
        }

        public static int NameType(int id)
        {
            bool gameExist = Shader.GetGlobalVector("_GameRect" + id).z > 0;

            gameExist = gameExist || (Shader.GetGlobalTexture("_GameTexture" + id) != null);

            bool sceneExist = Shader.GetGlobalVector("_SceneRect" + id).z > 0;

            sceneExist = sceneExist || (Shader.GetGlobalTexture("_SceneTexture" + id) != null);

            if (gameExist && !sceneExist)
            {
                return(1);
            }
                else if (!gameExist && sceneExist)
            {
                return(2);
            }
                else if (gameExist && sceneExist)
            {
                return(3);
            }
                else
            {
                return(0);
            }
        }
    }
}
