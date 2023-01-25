using FunkyCode.LightSettings;
using UnityEngine;

namespace FunkyCode.Rendering.Light
{
	public static class Main
	{
		private static Pass pass = new Pass();

		public static void Draw(Light2D light)
		{
			ShadowEngine.Prepare(light);

            LayerSetting[] layerSettings = light.GetLightPresetLayers();

			if (layerSettings == null)
			{
				return;
			}

			if (layerSettings.Length < 1)
			{
				return;
			}

			for (int layerID = 0; layerID < layerSettings.Length; layerID++)
			{
				LayerSetting layerSetting = layerSettings[layerID];

				if (layerSetting == null)
				{
					continue;
				}

				if (!pass.Setup(light, layerSetting))
				{
					continue;
				}

				ShadowEngine.SetPass(light, layerSetting);

				if (layerSetting.sorting == LightLayerSorting.None)
				{
					NoSort.Draw(pass);
				}
					else
				{
					pass.sortPass.SortObjects();

					Sorted.Draw(pass);
				}
			}

			// LightSource.Angle.Draw(light, 0);
		}
		
		public static void DrawTranslucency(Light2D light)
		{
			ShadowEngine.Prepare(light);

            LayerSetting[] layerSettings = light.GetTranslucencyPresetLayers();

			if (layerSettings == null)
			{
				return;
			}

			if (layerSettings.Length < 1)
			{
				return;
			}

			for (int layerID = 0; layerID < layerSettings.Length; layerID++)
			{
				LayerSetting layerSetting = layerSettings[layerID];

				if (layerSetting == null)
				{
					continue;
				}

				if (!pass.Setup(light, layerSetting))
				{
					continue;
				}

				ShadowEngine.SetPass(light, layerSetting);

				NoSort.Shadows.Draw(pass);
			}
		}
	}
}