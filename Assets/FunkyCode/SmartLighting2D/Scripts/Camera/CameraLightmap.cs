using UnityEngine;

namespace FunkyCode
{
	[System.Serializable]
	public struct CameraLightmap
	{
		public enum Rendering
		{
			Enabled,
			Disabled
		}

		public enum Overlay
		{
			Enabled,
			Disabled
		}

		public enum OverlayMaterial
		{
			Multiply,
			Additive,
			Custom,
			Reference
		};

		public enum OverlayPosition
		{
			Camera, 
			Custom
		};

		public enum OverlayLayerType { LightingLayer, UnityLayer };

		public enum SceneView
		{
			Disabled,
			Enabled
		}
		
		public enum Output {None, Shaders, Materials, Pass1, Pass2, Pass3, Pass4, Pass5, Pass6, Pass7, Pass8}

		public enum MaterialType {Incremental, Pass1, Pass2, Pass3, Pass4, Pass5, Pass6, Pass7, Pass8}

		public Rendering rendering;

		public SceneView sceneView;

		public Overlay overlay;
		public OverlayLayerType overlayLayerType;
		public OverlayMaterial overlayMaterial;
		public OverlayPosition overlayPosition;
	
		public Output output;
		
		public LightingSettings.SortingLayer sortingLayer;

		public Material customMaterial;
		public Material customMaterialInstance;


		// Output Materials
		public MaterialType materialsType;
		public LightmapMaterials materials;

		public int renderLayerId;

		public int id;

		public int presetId;

		public float customPosition;

		public CameraLightmap(int id = 0)
		{
			this.id = id;

			this.presetId = 0;

			this.rendering = Rendering.Enabled;

			this.overlay = Overlay.Enabled;

			this.overlayMaterial = OverlayMaterial.Multiply;

			this.overlayLayerType = OverlayLayerType.LightingLayer;

			this.customMaterial = null;

			this.customMaterialInstance = null;

			this.renderLayerId = 0;

			this.output = Output.None;

			this.overlayPosition = OverlayPosition.Camera;
			
			this.materials = new LightmapMaterials();

			this.sortingLayer = new LightingSettings.SortingLayer();

			this.materialsType = MaterialType.Incremental;

			this.sceneView = SceneView.Enabled;

			this.customPosition = 0;
		}

		public LightmapMaterials GetMaterials()
		{
			if (materials == null)
				materials = new LightmapMaterials();
	
			return materials;
		}

		public Material GetMaterial()
		{
			if (!customMaterialInstance && customMaterial)
				customMaterialInstance = new Material(customMaterial);

			return customMaterialInstance;
		}
	}
}
