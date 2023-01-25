using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode
{
	public static class LightmapShaders
	{
		public static Texture previewTexture;

		public static LightmapPreset[] ActivePassLightmaps = new LightmapPreset[10];

		public static Texture GetPreviewTexture()
		{
			if (previewTexture == null) {
				previewTexture = Resources.Load<Texture>("Sprites/gfx_light2");
			}

			return(previewTexture);
		}

		public static void ResetShaders()
		{
			for(int i = 0; i < 10; i++)
				ActivePassLightmaps[i] = null;
			
			Vector4 zero = new Vector4(0, 0, 0, 0);

			float offscreen = Lighting2D.ProjectSettings.materialOffScreen == MaterialOffScreen.Lit ? 1 : 0;
			Shader.SetGlobalFloat("_OffScreen", offscreen);

			// game
			Shader.SetGlobalTexture("_GameTexture1", null);
			Shader.SetGlobalVector("_GameRect1", zero);
			Shader.SetGlobalVector("_GameColor1", zero);
			Shader.SetGlobalFloat("_GameRotation1", 0);

			Shader.SetGlobalTexture("_GameTexture2", null);
			Shader.SetGlobalVector("_GameRect2", zero);
			Shader.SetGlobalVector("_GameColor2", zero);
			Shader.SetGlobalFloat("_GameRotation2", 0);

			Shader.SetGlobalTexture("_GameTexture3", null);
			Shader.SetGlobalVector("_GameRect3", zero);
			Shader.SetGlobalVector("_GameColor3", zero);
			Shader.SetGlobalFloat("_GameRotation3", 0);

			Shader.SetGlobalTexture("_GameTexture4", null);
			Shader.SetGlobalVector("_GameRect4", zero);
			Shader.SetGlobalVector("_GameColor4", zero);
			Shader.SetGlobalFloat("_GameRotation4", 0);

			Shader.SetGlobalTexture("_GameTexture5", null);
			Shader.SetGlobalVector("_GameRect5", zero);
			Shader.SetGlobalVector("_GameColor5", zero);
			Shader.SetGlobalFloat("_GameRotation5", 0);

			Shader.SetGlobalTexture("_GameTexture6", null);
			Shader.SetGlobalVector("_GameRect6", zero);
			Shader.SetGlobalVector("_GameColor6", zero);
			Shader.SetGlobalFloat("_GameRotation6", 0);

			Shader.SetGlobalTexture("_GameTexture7", null);
			Shader.SetGlobalVector("_GameRect7", zero);
			Shader.SetGlobalVector("_GameColor7", zero);
			Shader.SetGlobalFloat("_GameRotation7", 0);

			Shader.SetGlobalTexture("_GameTexture8", null);
			Shader.SetGlobalVector("_GameRect8", zero);
			Shader.SetGlobalVector("_GameColor8", zero);
			Shader.SetGlobalFloat("_GameRotation8", 0);


			// scene
			Shader.SetGlobalTexture("_SceneTexture1", null);
			Shader.SetGlobalVector("_SceneRect1", zero);
			Shader.SetGlobalVector("_SceneColor1", zero);
			Shader.SetGlobalFloat("_SceneRotation1", 0);

			Shader.SetGlobalTexture("_SceneTexture2", null);
			Shader.SetGlobalVector("_SceneRect2", zero);
			Shader.SetGlobalVector("_SceneColor2", zero);
			Shader.SetGlobalFloat("_SceneRotation2", 0);

			Shader.SetGlobalTexture("_SceneTexture3", null);
			Shader.SetGlobalVector("_SceneRect3", zero);
			Shader.SetGlobalVector("_SceneColor3", zero);
			Shader.SetGlobalFloat("_SceneRotation3", 0);

			Shader.SetGlobalTexture("_SceneTexture4", null);
			Shader.SetGlobalVector("_SceneRect4", zero);
			Shader.SetGlobalVector("_SceneColor4", zero);
			Shader.SetGlobalFloat("_SceneRotation4", 0);

			Shader.SetGlobalTexture("_SceneTexture5", null);
			Shader.SetGlobalVector("_SceneRect5", zero);
			Shader.SetGlobalVector("_SceneColor5", zero);
			Shader.SetGlobalFloat("_SceneRotation5", 0);

			Shader.SetGlobalTexture("_SceneTexture6", null);
			Shader.SetGlobalVector("_SceneRect6", zero);
			Shader.SetGlobalVector("_SceneColor6", zero);
			Shader.SetGlobalFloat("_SceneRotation6", 0);

			Shader.SetGlobalTexture("_SceneTexture7", null);
			Shader.SetGlobalVector("_SceneRect7", zero);
			Shader.SetGlobalVector("_SceneColor7", zero);
			Shader.SetGlobalFloat("_SceneRotation7", 0);

			Shader.SetGlobalTexture("_SceneTexture8", null);
			Shader.SetGlobalVector("_SceneRect8", zero);
			Shader.SetGlobalVector("_SceneColor8", zero);
			Shader.SetGlobalFloat("_SceneRotation8", 0);
		}

		public static void SetShaders(bool isSceneView, int id, Camera camera, LightTexture lightTexture, LightmapPreset lightmapPreset)
		{
			ActivePassLightmaps[id] = lightmapPreset;

			float ratio = (float)camera.pixelRect.width / camera.pixelRect.height;

			float x = camera.transform.position.x;
			float y = camera.transform.position.y;

			// z = width ; w = height
			float w = camera.orthographicSize * 2;
			float z = w * ratio;

			float rotation = camera.transform.eulerAngles.z * Mathf.Deg2Rad;

			Vector4 rect = new Vector4(x, y, z, w);

			Color c = lightmapPreset.darknessColor;

			Vector4 color = new Vector4(c.r, c.g, c.b, c.a);

			if (lightTexture == null)
			{
				Debug.Log("light texture null");
				return;
			}

			Texture texture = lightTexture.renderTexture;

			if (Lighting2D.ProjectSettings.shaderPreview == ShaderPreview.Enabled)
			{
				texture = GetPreviewTexture();
				color = Color.black;
				rect = new Vector4(0, 0, 1, 1);
			}

			bool gameView = !isSceneView;

			if (gameView)
			{
				switch(id)
				{
					case 1:
						Shader.SetGlobalTexture("_GameTexture1", texture);
						Shader.SetGlobalVector("_GameRect1", rect);
						Shader.SetGlobalVector("_GameColor1", color);
						Shader.SetGlobalFloat("_GameRotation1", rotation);
					break;

					case 2:
						Shader.SetGlobalTexture("_GameTexture2", texture);
						Shader.SetGlobalVector("_GameRect2", rect);
						Shader.SetGlobalVector("_GameColor2", color);
						Shader.SetGlobalFloat("_GameRotation2", rotation);
					break;

					case 3:
						Shader.SetGlobalTexture("_GameTexture3", texture);
						Shader.SetGlobalVector("_GameRect3", rect);
						Shader.SetGlobalVector("_GameColor3", color);
						Shader.SetGlobalFloat("_GameRotation3", rotation);
					break;

					case 4:
						Shader.SetGlobalTexture("_GameTexture4", texture);
						Shader.SetGlobalVector("_GameRect4", rect);
						Shader.SetGlobalVector("_GameColor4", color);
						Shader.SetGlobalFloat("_GameRotation4", rotation);
					break;

					case 5:
						Shader.SetGlobalTexture("_GameTexture5", texture);
						Shader.SetGlobalVector("_GameRect5", rect);
						Shader.SetGlobalVector("_GameColor5", color);
						Shader.SetGlobalFloat("_GameRotation5", rotation);
					break;

					case 6:
						Shader.SetGlobalTexture("_GameTexture6", texture);
						Shader.SetGlobalVector("_GameRect6", rect);
						Shader.SetGlobalVector("_GameColor6", color);
						Shader.SetGlobalFloat("_GameRotation6", rotation);
					break;

					case 7:
						Shader.SetGlobalTexture("_GameTexture7", texture);
						Shader.SetGlobalVector("_GameRect7", rect);
						Shader.SetGlobalVector("_GameColor7", color);
						Shader.SetGlobalFloat("_GameRotation7", rotation);
					break;

					case 8:
						Shader.SetGlobalTexture("_GameTexture8", texture);
						Shader.SetGlobalVector("_GameRect8", rect);
						Shader.SetGlobalVector("_GameColor8", color);
						Shader.SetGlobalFloat("_GameRotation8", rotation);
					break;
				}
			}
				else
			{
				switch(id)
				{
					case 1:
						Shader.SetGlobalTexture("_SceneTexture1", texture);
						Shader.SetGlobalVector("_SceneRect1", rect);
						Shader.SetGlobalVector("_SceneColor1", color);
						Shader.SetGlobalFloat("_SceneRotation1", rotation);
					break;

					case 2:
						Shader.SetGlobalTexture("_SceneTexture2", texture);
						Shader.SetGlobalVector("_SceneRect2", rect);
						Shader.SetGlobalVector("_SceneColor2", color);
						Shader.SetGlobalFloat("_SceneRotation2", rotation);
					break;

					case 3:
						Shader.SetGlobalTexture("_SceneTexture3", texture);
						Shader.SetGlobalVector("_SceneRect3", rect);
						Shader.SetGlobalVector("_SceneColor4", color);
						Shader.SetGlobalFloat("_SceneRotation3", rotation);
					break;

					case 4:
						Shader.SetGlobalTexture("_SceneTexture4", texture);
						Shader.SetGlobalVector("_SceneRect4", rect);
						Shader.SetGlobalVector("_SceneColor4", color);
						Shader.SetGlobalFloat("_SceneRotation4", rotation);
					break;

					case 5:
						Shader.SetGlobalTexture("_SceneTexture5", texture);
						Shader.SetGlobalVector("_SceneRect5", rect);
						Shader.SetGlobalVector("_SceneColor5", color);
						Shader.SetGlobalFloat("_SceneRotation5", rotation);
					break;

					case 6:
						Shader.SetGlobalTexture("_SceneTexture6", texture);
						Shader.SetGlobalVector("_SceneRect6", rect);
						Shader.SetGlobalVector("_SceneColor6", color);
						Shader.SetGlobalFloat("_SceneRotation6", rotation);
					break;

					case 7:
						Shader.SetGlobalTexture("_SceneTexture7", texture);
						Shader.SetGlobalVector("_SceneRect7", rect);
						Shader.SetGlobalVector("_SceneColor7", color);
						Shader.SetGlobalFloat("_SceneRotation7", rotation);
					break;

					case 8:
						Shader.SetGlobalTexture("_SceneTexture8", texture);
						Shader.SetGlobalVector("_SceneRect8", rect);
						Shader.SetGlobalVector("_SceneColor8", color);
						Shader.SetGlobalFloat("_SceneRotation8", rotation);
					break;
				}
			}
		}

		public static void SetDayLight()
		{
			float direction = -(Lighting2D.DayLightingSettings.direction - 180) * Mathf.Deg2Rad;
			float height = Lighting2D.DayLightingSettings.bumpMap.height;
			float alpha = Lighting2D.DayLightingSettings.ShadowColor.a;

			Shader.SetGlobalFloat("_Day_Direction", direction);
			Shader.SetGlobalFloat("_Day_Height", height);
			Shader.SetGlobalFloat("_Day_Alpha", alpha);
		}
	}
}