using UnityEngine;
using FunkyCode.LightSettings;

namespace FunkyCode.LightingSettings
{
	[System.Serializable]
	public class QualitySettings
	{
		public static string[] LightingSourceTextureSizeArray = new string[]{"Custom", "2048", "1024", "512", "256", "128",  "PixelPerfect"};

		public LightingSourceTextureSize lightTextureSize = LightingSourceTextureSize.px2048;
		public LightingSourceTextureSize lightEffectTextureSize = LightingSourceTextureSize.px2048;
		
		public FilterMode lightFilterMode = FilterMode.Bilinear;
		public FilterMode lightmapFilterMode = FilterMode.Bilinear;

		public UpdateMethod updateMethod = UpdateMethod.LateUpdate;
		public CoreAxis coreAxis = CoreAxis.XY;
	
		public Projection projection = Projection.Orthographic;
	}

	public enum HDR
	{
		Off,
		Half,
		Float
	}

	[System.Serializable]
	public class Layers
	{
		public LayersList colliderLayers = new LayersList();
		public LayersList lightLayers = new LayersList();
		public LayersList dayLayers = new LayersList();

		public Layers()
		{
			colliderLayers.names[0] = "Default";

			lightLayers.names[0] = "Default";

			dayLayers.names[0] = "Default";
		}
	}

	[System.Serializable]
	public class LayersList
	{
		public string[] names = new string[1];

		public string[] GetNames() {
			string[] layers = new string[names.Length];

			for(int i = 0; i < names.Length; i++)
			{
				layers[i] = names[i];
			}

			return(layers);
		}

		public string[] GetOcclusionNames()
		{
			string[] layers = new string[names.Length + 1];

			layers[0] = "Disabled";

			for(int i = 0; i < names.Length; i++) {
				layers[i + 1] = names[i];
			}

			return(layers);
		}

		public string[] GetTranslucencyNames()
		{
			string[] layers = new string[names.Length + 2];

			layers[0] = "Disabled";

			layers[1] = "Enabled (Masking)";

			for(int i = 0; i < names.Length; i++) {
				layers[i + 2] = names[i];
			}

			return(layers);
		}
	}

	[System.Serializable]
	public class DayLightingSettings
	{
		[Range(0, 360)]
		public float direction = 270;

		public Color ShadowColor = Color.black;

		[Range(0, 10)]
		public float height = 1;

		public BumpMap bumpMap = new BumpMap();

		// Is this only bumpmap settings?
		[System.Serializable]
		public class BumpMap
		{
			[Range(0, 5)]
			public float height = 1;

			[Range(0, 5)]
			public float strength = 1;
		}
	}

	[System.Serializable]
	public class SortingLayer
	{
		[SerializeField]
		private string name = "Default";
		public string Name
		{
			get
			{
				if (name.Length < 1)
				{
					name = "Default";
				}

				return(name);
			} 

			set => name = value;
		}

		public int Order = 0;

		public void ApplyToMeshRenderer(MeshRenderer meshRenderer)
		{
			if (meshRenderer == null)
			{
				return;
			}
			
			if (meshRenderer.sortingLayerName != Name)
			{
				meshRenderer.sortingLayerName = Name;
			}

			if (meshRenderer.sortingOrder != Order)
			{
				meshRenderer.sortingOrder = Order;
			}
		}
	}

	[System.Serializable]
	public class EditorView
	{
		public int sceneViewLayer = 0;

		public int gameViewLayer = 0;
	}

	[System.Serializable]
	public class Gizmos
	{
		public EditorDrawGizmos drawGizmos = EditorDrawGizmos.Selected;
		public EditorGizmosBounds drawGizmosBounds = EditorGizmosBounds.Disabled;
		public EditorChunks drawGizmosChunks = EditorChunks.Disabled;
		public EditorShadowCasters drawGizmosShadowCasters = EditorShadowCasters.Disabled;

		public EditorIcons drawIcons = EditorIcons.Disabled;
	
		public int sceneViewLayer = 0;

		public int gameViewLayer = 0;
	}

	[System.Serializable]
	public class Chunks
	{
		public bool enabled = true;

		public int chunkSize = 10;
	}

	[System.Serializable]
	public class MeshMode
	{
		public bool enable = false;

		[Range(0, 1)]
		public float alpha = 0.5f;

		public MeshModeShader shader = MeshModeShader.Additive;
		public Material[] materials = new Material[1];

		public LightingSettings.SortingLayer sortingLayer = new LightingSettings.SortingLayer();
	}
	
	[System.Serializable]
	public class BumpMapMode
	{
		public NormalMapType type = NormalMapType.PixelToLight;
		
		public NormalMapTextureType textureType = NormalMapTextureType.Texture;
		
		public Texture texture;
		public Sprite sprite;

		public bool invertX = false;
		public bool invertY = false;

		[Range(0, 1)]
		public float depth = 1;

		public SpriteRenderer spriteRenderer;

		public void SetSpriteRenderer(SpriteRenderer spriteRenderer)
		{
			this.spriteRenderer = spriteRenderer;
		}

		public Texture GetBumpTexture()
		{
			switch(textureType)
			{
				case NormalMapTextureType.Sprite:

					if (sprite == null)
					{
						return(null);
					}

					return(sprite.texture);

				case NormalMapTextureType.Texture:

					return(texture);
			}
			
			return(null);
		}

		public Material SelectMaterial(Material pixel, Material direction)
		{
			Material material = pixel;

			if (type == NormalMapType.ObjectToLight)
			{
				material = direction;
			}

			return(material);
		}
	}

	[System.Serializable]
	public class DayNormalMapMode
	{
		public NormalMapTextureType textureType = NormalMapTextureType.Texture;
		
		public Texture texture;
		public Sprite sprite;

		public Texture GetBumpTexture()
		{
			switch(textureType)
			{
				case NormalMapTextureType.Sprite:

					if (sprite == null)
					{
						return(null);
					}

					return(sprite.texture);

				case NormalMapTextureType.Texture:

					return(texture);
			}
			
			return(null);
		}
	}

	[System.Serializable]
	public class GlowMode
	{
		public bool enable = false;

		[Range(0.1f, 20)]
		public float glowRadius = 1;
	}

	public enum MeshModeShader
	{
		Additive, 
		Alpha, 
		Custom
	}

	public enum EditorDrawGizmos
	{
		Disabled, 
		Selected, 
		Always
	}

	public enum EditorChunks
	{
		Disabled,
		Enabled
	}

	public enum EditorShadowCasters
	{
		Disabled,
		Enabled
	}

	public enum EditorIcons
	{
		Disabled,
		Enabled
	}

	public enum EditorGizmosBounds
	{
		Disabled, 
		Enabled
	}

	public enum ShaderPreview
	{
		Disabled,
		Enabled
	}

	public enum ManagerInternal
	{
		HideInHierarchy,
		ShowInHierarchy
	}
	public enum ManagerInstance
	{
		Static,
		DontDestroyOnLoad,
		Dynamic
	}

	public enum UpdateMethod
	{
		LateUpdate,
		Custom
	}
}