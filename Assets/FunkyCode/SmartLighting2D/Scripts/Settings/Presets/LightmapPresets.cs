using UnityEngine;
using FunkyCode.LightSettings;

namespace FunkyCode.LightingSettings
{
	[System.Serializable]
	public class LightmapPresetList
	{
		public LightmapPreset[] list = new LightmapPreset[1];

		public LightmapPreset this[int i] => list[i];

		public string[] GetLightmapLayers()
		{
			string[] layers = new string[list.Length];

			for(int i = 0; i < list.Length; i++)
			{
				if (list[i].name.Length > 0)
				{
					layers[i] = list[i].name;
				}
					else
				{
					layers[i] = "Preset (Id: " + (i + 1) + ")";
				}
			}

			return(layers);
		}
	}

	[System.Serializable]
	public class LightmapPreset
	{
		public enum Type
		{
			RGB24,
			R8,
			RHalf,
			Depth8
		}

		public enum HDR
		{
			Off,
			Half,
			Float
		}

		public string name = "Default";

		public Type type = Type.RGB24;
		public HDR hdr = HDR.Half;

		public Color darknessColor = new Color(0, 0, 0, 1);
		public int depth = -100;

		public float resolution = 1f;

		public LightmapLayerList dayLayers = new LightmapLayerList();
		public LightmapLayerList lightLayers = new LightmapLayerList();

		public LightmapPreset (int id)
		{
			if (id == 0)
			{
				name = "Default";
			}
				else
			{
				name = "Preset (Id: " + (id + 1) + ")";
			}
		}
	}
	
	[System.Serializable]
	public class LightmapLayerList
	{
		public LightmapLayer[] list = new LightmapLayer[1];

		public LightmapLayer this[int i] => list[i];

		public void SetArray(LightmapLayer[] array)
		{
			list = array;
		}

		public LightmapLayer[] Get()
		{
			for(int i = 0; i < list.Length; i++)
			{
				if (list[i] == null)
				{
					list[i] = new LightmapLayer();
				}
			}
	
			return(list);
		}
	}

	[System.Serializable]
	public class LightmapLayer
	{
		public int id = 0;
		public LayerType type = LayerType.ShadowsAndMask;
		public LayerSorting sorting = LayerSorting.None;

		public int GetLayerID()
		{
			int layerId = (int)id;

			layerId = (layerId < 0) ? -1 : layerId;

			return(layerId);
		}
	}
}