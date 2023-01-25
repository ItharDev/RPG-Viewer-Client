using UnityEngine;

namespace FunkyCode
{
	[System.Serializable]
	public class LightmapMaterials
	{
		public Material[] materials = new Material[1];

		public static void ClearMaterial(Material material)
		{
			var zero = new Vector4(0, 0, 0, 0);

			// game
			material.SetTexture("_GameTexture1", null);
			material.SetVector("_GameRect1", zero);
			material.SetVector("_GameColor1", zero);
			material.SetFloat("_GameRotation1", 0);

			material.SetTexture("_GameTexture2", null);
			material.SetVector("_GameRect2", zero);
			material.SetVector("_GameColor2", zero);
			material.SetFloat("_GameRotation2", 0);

			material.SetTexture("_GameTexture3", null);
			material.SetVector("_GameRect3", zero);
			material.SetVector("_GameColor3", zero);
			material.SetFloat("_GameRotation3", 0);

			material.SetTexture("_GameTexture4", null);
			material.SetVector("_GameRect4", zero);
			material.SetVector("_GameColor4", zero);
			material.SetFloat("_GameRotation4", 0);

			material.SetTexture("_GameTexture5", null);
			material.SetVector("_GameRect5", zero);
			material.SetVector("_GameColor5", zero);
			material.SetFloat("_GameRotation5", 0);

			material.SetTexture("_GameTexture6", null);
			material.SetVector("_GameRect6", zero);
			material.SetVector("_GameColor6", zero);
			material.SetFloat("_GameRotation6", 0);

			material.SetTexture("_GameTexture7", null);
			material.SetVector("_GameRect7", zero);
			material.SetVector("_GameColor7", zero);
			material.SetFloat("_GameRotation7", 0);

			material.SetTexture("_GameTexture8", null);
			material.SetVector("_GameRect8", zero);
			material.SetVector("_GameColor8", zero);
			material.SetFloat("_GameRotation8", 0);


			// scene
			material.SetTexture("_SceneTexture1", null);
			material.SetVector("_SceneRect1", zero);
			material.SetVector("_SceneColor1", zero);
			material.SetFloat("_SceneRotation1", 0);

			material.SetTexture("_SceneTexture2", null);
			material.SetVector("_SceneRect2", zero);
			material.SetVector("_SceneColor2", zero);
			material.SetFloat("_SceneRotation2", 0);

			material.SetTexture("_SceneTexture3", null);
			material.SetVector("_SceneRect3", zero);
			material.SetVector("_SceneColor3", zero);
			material.SetFloat("_SceneRotation3", 0);

			material.SetTexture("_SceneTexture4", null);
			material.SetVector("_SceneRect4", zero);
			material.SetVector("_SceneColor4", zero);
			material.SetFloat("_SceneRotation4", 0);

			material.SetTexture("_SceneTexture5", null);
			material.SetVector("_SceneRect5", zero);
			material.SetVector("_SceneColor5", zero);
			material.SetFloat("_SceneRotation5", 0);

			material.SetTexture("_SceneTexture6", null);
			material.SetVector("_SceneRect6", zero);
			material.SetVector("_SceneColor6", zero);
			material.SetFloat("_SceneRotation6", 0);

			material.SetTexture("_SceneTexture7", null);
			material.SetVector("_SceneRect7", zero);
			material.SetVector("_SceneColor7", zero);
			material.SetFloat("_SceneRotation7", 0);

			material.SetTexture("_SceneTexture8", null);
			material.SetVector("_SceneRect8", zero);
			material.SetVector("_SceneColor8", zero);
			material.SetFloat("_SceneRotation8", 0);
		}

		public static void SetMaterial(int id, MaterialPass materialPass)
		{
			Material material = materialPass.material;
			Vector4 rect = materialPass.rect;
			bool isSceneView = materialPass.isSceneView;
			float rotation = materialPass.rotation;
			Texture texture = materialPass.texture;
			Color color = materialPass.color;
			
			bool isGameView = !isSceneView;

			// scene?

			if (isGameView)
			{
				switch(id)
				{
					case 1:
						material.SetTexture("_GameTexture1", texture);
						material.SetVector("_GameRect1", rect);
						material.SetVector("_GameColor1", color);
						material.SetFloat("_GameRotation1", rotation);
					break;

					case 2:
						material.SetTexture("_GameTexture2", texture);
						material.SetVector("_GameRect2", rect);
						material.SetVector("_GameColor2", rect);
						material.SetFloat("_GameRotation2", rotation);
					break;

					case 3:
						material.SetTexture("_GameTexture3", texture);
						material.SetVector("_GameRect3", rect);
						material.SetVector("_GameColor3", color);
						material.SetFloat("_GameRotation3", rotation);
					break;

					case 4:
						material.SetTexture("_GameTexture4", texture);
						material.SetVector("_GameRect4", rect);
						material.SetVector("_GameColor4", rect);
						material.SetFloat("_GameRotation4", rotation);
					break;

					case 5:
						material.SetTexture("_GameTexture5", texture);
						material.SetVector("_GameRect5", rect);
						material.SetVector("_GameColor5", color);
						material.SetFloat("_GameRotation5", rotation);
					break;

					case 6:
						material.SetTexture("_GameTexture6", texture);
						material.SetVector("_GameRect6", rect);
						material.SetVector("_GameColor6", rect);
						material.SetFloat("_GameRotation6", rotation);
					break;

					case 7:
						material.SetTexture("_GameTexture7", texture);
						material.SetVector("_GameRect7", rect);
						material.SetVector("_GameColor7", color);
						material.SetFloat("_GameRotation7", rotation);
					break;

					case 8:
						material.SetTexture("_GameTexture8", texture);
						material.SetVector("_GameRect8", rect);
						material.SetVector("_GameColor8", rect);
						material.SetFloat("_GameRotation8", rotation);
					break;
				}
			}
				else
			{
				switch(id)
				{
					case 1:
						material.SetTexture("_SceneTexture1", texture);
						material.SetVector("_SceneRect1", rect);
						material.SetVector("_SceneColor1", color);
						material.SetFloat("_SceneRotation1", rotation);
					break;

					case 2:
						material.SetTexture("_SceneTexture2", texture);
						material.SetVector("_SceneRect2", rect);
						material.SetVector("_SceneColor2", rect);
						material.SetFloat("_SceneRotation2", rotation);
					break;

					case 3:
						material.SetTexture("_SceneTexture3", texture);
						material.SetVector("_SceneRect3", rect);
						material.SetVector("_SceneColor3", color);
						material.SetFloat("_SceneRotation3", rotation);
					break;

					case 4:
						material.SetTexture("_SceneTexture4", texture);
						material.SetVector("_SceneRect4", rect);
						material.SetVector("_SceneColor4", rect);
						material.SetFloat("_SceneRotation4", rotation);
					break;

					case 5:
						material.SetTexture("_SceneTexture5", texture);
						material.SetVector("_SceneRect5", rect);
						material.SetVector("_SceneColor5", color);
						material.SetFloat("_SceneRotation5", rotation);
					break;

					case 6:
						material.SetTexture("_SceneTexture6", texture);
						material.SetVector("_SceneRect6", rect);
						material.SetVector("_SceneColor6", rect);
						material.SetFloat("_SceneRotation6", rotation);
					break;

					case 7:
						material.SetTexture("_SceneTexture7", texture);
						material.SetVector("_SceneRect7", rect);
						material.SetVector("_SceneColor7", color);
						material.SetFloat("_SceneRotation7", rotation);
					break;

					case 8:
						material.SetTexture("_SceneTexture8", texture);
						material.SetVector("_SceneRect8", rect);
						material.SetVector("_SceneColor8", rect);
						material.SetFloat("_SceneRotation8", rotation);
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

		public void Add(Material material)
		{
			foreach(var m in  materials)
			{
				if (m == material)
				{
					Debug.Log("Lighting Manager 2D: Failed to add material (material already added!");
					return;
				}
			}

			for(int i = 0 ; i < materials.Length; i++)
			{
				if (materials[i] != null)
				{
					continue;
				}

				materials[i] = material;

				return;
			}

			System.Array.Resize(ref materials, materials.Length + 1);

			materials[materials.Length - 1] = material;
		}

		public void Remove(Material material)
		{
			for(int i = 0 ; i < materials.Length; i++)
			{
				if (materials[i] != material)
					continue;
				
				materials[i] = null;

				return;
			}

			Debug.LogWarning("Lighting Manager 2D: Removing material that does not exist");
		}
	}
}