using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightingSettings;
using FunkyCode.LightSettings;
using FunkyCode.EventHandling;
namespace FunkyCode
{
	[ExecuteInEditMode]
	public class Light2D : LightingMonoBehaviour
	{
		public enum LightType
		{
			Point,
			Sprite, 
			FreeForm,
		}

		public enum LightSprite {Default, Custom};
		public enum LitMode {Everything, MaskOnly}
		public enum WhenInsideCollider {Draw, DontDraw, Ignore}
		public enum Rotation {Disabled, World, Local}
		public enum MaskTranslucencyQuality {Disabled, LowQuality, MediumQuality, HighQuality}

		public LightType lightType = LightType.Sprite;

		// settings
		public int lightPresetId = 0;
		public int eventPresetId = 0;
		
		// light layer
		public int lightLayer = 0;
		public int occlusionLayer = 0;
		public int translucentLayer = 0;
		
		public int translucentPresetId = 0;

		public Color color = new Color(.5f, .5f, .5f, 1);

		public float size = 5f;

		public float spotAngleInner = 360;
		public float spotAngleOuter = 360;

		// soft shadow
		public float coreSize = 0.5f;
		public float falloff = 0;

		public float lightStrength = 0;

		// legacy shadow
		public float outerAngle = 15;

		public float lightRadius = 1;

		public float shadowDistanceClose = 0;
		public float shadowDistanceFar = 5;

		public MaskTranslucencyQuality maskTranslucencyQuality = MaskTranslucencyQuality.LowQuality;
		public float maskTranslucencyStrength = 0.5f;

		public Rotation applyRotation = Rotation.Disabled;

		public LightingSourceTextureSize textureSize = LightingSourceTextureSize.px2048;

		public MeshMode meshMode = new MeshMode();
		public BumpMap bumpMap = new BumpMap();

		public WhenInsideCollider whenInsideCollider = WhenInsideCollider.Draw;

		public LightSprite lightSprite = LightSprite.Default;
		public Sprite sprite;
		public bool spriteFlipX = false;
		public bool spriteFlipY = false;

		public LightTransform transform2D;

		public LightFreeForm freeForm;
		public float freeFormFalloff = 1;
		public float freeFormPoint = 1;
		public float freeFormFalloffStrength = 1;
		public FreeFormPoints freeFormPoints = new FreeFormPoints();

		public LightEventHandling eventHandling = new LightEventHandling();

		[System.Serializable]
		public class LightEventHandling
		{
			public EventHandling.Object eventHandlingObject = new EventHandling.Object();
		}

		// Internal
		private List<LightCollider2D> collidersInside = new List<LightCollider2D>();
		private List<LightCollider2D> collidersInsideRemove = new List<LightCollider2D>();

		public static List<Light2D> List = new List<Light2D>();	
		private bool inScreen = false;
		public bool drawingEnabled = false;
		public bool drawingTranslucencyEnabled = false;
		private LightBuffer2D buffer = null;
		private static Sprite defaultSprite = null;

		public LightBuffer2D Buffer
		{
			get => buffer;
			set => buffer = value;
		}

		public void AddEvent(CollisionEvent2D collisionEvent)
		{
			eventHandling.eventHandlingObject.collisionEvents += collisionEvent;
		}
	
		public void AddCollider(LightCollider2D id)
		{
			if (collidersInside.Contains(id))
			{
				if (lightPresetId > 0)
				{
					id.lightOnEnter?.Invoke(this);
				}
				
				collidersInside.Add(id);
			}
		}

		[System.Serializable]
		public class BumpMap
		{
			public float intensity = 1;
			public float depth = 1;
		}

		public LayerSetting[] GetLightPresetLayers()
		{
			var presetList = Lighting2D.Profile.lightPresets;
			
			if (lightPresetId >= presetList.list.Length)
			{
				return null;
			}

			var lightPreset = presetList.Get()[lightPresetId];

			return lightPreset.layerSetting.Get();
		}

		public LayerSetting[] GetTranslucencyPresetLayers()
		{
			var presetList = Lighting2D.Profile.lightPresets;
			
			if (translucentPresetId >= presetList.list.Length)
			{
				return null;
			}

			var lightPreset = presetList.Get()[translucentPresetId];

			return lightPreset.layerSetting.Get();
		}

		public EventPreset GetEventPreset()
		{
			var presetList = Lighting2D.Profile.eventPresets;
			
			if (eventPresetId >= presetList.list.Length)
			{
				return null;
			}

			var lightPreset = presetList.Get()[eventPresetId];

			return lightPreset;
		}

		static public Sprite GetDefaultSprite()
		{
			if (!defaultSprite || !defaultSprite.texture)
			{
				defaultSprite = Resources.Load<Sprite>("Sprites/gfx_light");
			}
			
			return defaultSprite;
		}

		public Sprite GetSprite()
		{
			if (!sprite || !sprite.texture)
			{
				sprite = GetDefaultSprite();
			}
			
			return sprite;
		}

		public void ForceUpdate()
		{
			if (transform2D == null)
			{
				return;
			}

			transform2D.ForceUpdate();

			freeForm.ForceUpdate();
		}

		static public void ForceUpdateAll()
		{
			foreach(var light in Light2D.List)
			{
				light.ForceUpdate();
			}
		}

		public void OnEnable()
		{
			List.Add(this);

			if (transform2D == null)
			{
				transform2D = new LightTransform();
			}

			if (freeForm == null)
			{
				freeForm = new LightFreeForm();
			}

			LightingManager2D.Get();

			collidersInside.Clear();

			ForceUpdate();
		}

		public void OnDisable()
		{
			List.Remove(this);

			Free();
		}

		public void Free()
		{
			Buffers.Manager.FreeBuffer(buffer);

			inScreen = false;
		}

		// used to check if camera is used in the system

		public bool InCameras()
		{
			var lightingCameras = CameraTransform.List;
			var lightRect = transform2D.WorldRect;

			for(int i = 0; i < lightingCameras.Count; i++)
			{
				var cameraTransform = lightingCameras[i];
				var camera = cameraTransform.Camera;
				if (!camera)
					continue;

				var cameraRect = cameraTransform.WorldRect();
				if (cameraRect.Overlaps(lightRect))
				{
					return true;
				}
			}

			return false;
		}

		// to check if light is rendered for specific lightmap

		public bool InCamera(Camera camera)
		{
			var lightRect = transform2D.WorldRect;
			var cameraRect = CameraTransform.GetWorldRect(camera);

			return cameraRect.Overlaps(lightRect);
		}

		// light 2D should know what layers id's it is supposed to draw? (include in array)

		public bool IfDrawLightCollider(LightCollider2D lightCollider)
		{	
			var layerSetting = GetLightPresetLayers();
			if (layerSetting == null)
			{
				return false;
			}

			for(int i = 0; i < layerSetting.Length; i++)
			{
				var setting = layerSetting[i];
				if (setting == null)
					continue;

				int layerID = setting.layerID;
				switch(setting.type)
				{
					case LightLayerType.ShadowAndMask:

						if (layerID == lightCollider.shadowLayer || layerID == lightCollider.maskLayer)
						{
							return true;
						}
						break;

					case LightLayerType.MaskOnly:

						if (layerID == lightCollider.maskLayer)
						{
							return true;
						}
						break;

					case LightLayerType.ShadowOnly:

						if (layerID == lightCollider.shadowLayer)
						{
							return true;
						}	
						break;
				}
			}

			return false;
		}

		public Vector2Int GetTextureSize()
		{
			var textureSize2D = LightingRender2D.GetTextureSize(textureSize);

			if (Lighting2D.Profile.qualitySettings.lightTextureSize != LightingSettings.LightingSourceTextureSize.Custom)
			{
				textureSize2D = LightingRender2D.GetTextureSize(Lighting2D.Profile.qualitySettings.lightTextureSize);
			}

			return textureSize2D;
		}
		
		public bool IsPixelPerfect()
		{
			if (Lighting2D.Profile.qualitySettings.lightTextureSize != LightingSettings.LightingSourceTextureSize.Custom)
			{
				return(Lighting2D.Profile.qualitySettings.lightTextureSize == LightingSettings.LightingSourceTextureSize.PixelPerfect);
			}

			return textureSize == LightingSourceTextureSize.PixelPerfect;
		}

		public LightBuffer2D GetBuffer()
		{
			if (buffer == null)
			{ 
				buffer = Buffers.Manager.PullBuffer (this);
			}
			
			return buffer;
		}

		public void UpdateLoop()
		{
			transform2D.Update(this);

			if (lightType == LightType.FreeForm)
			{
				freeForm.Update(this);
			}

			// if camera moves & pixel perfect
			if (IsPixelPerfect())
			{
				transform2D.ForceUpdate();
			}
			
			UpdateBuffer();

			DrawMeshMode();

			if (eventPresetId > 0)
			{
				var eventPreset = GetEventPreset();
				if (eventPreset != null)
				{
					eventHandling.eventHandlingObject.Update(this, eventPreset);
				}
			}
		}

		void BufferUpdate()
		{
			transform2D.ClearUpdate();

			if (Lighting2D.Disable)
			{
				return;
			}

			if (buffer == null)
				return;
			
			buffer.updateNeeded = true;
		}

		void UpdateCollidersInside()
		{
			foreach(var collider in collidersInside)
			{
				if (collider == null)
				{
					collidersInsideRemove.Add(collider);
					continue;
				}

				if (!collider.isActiveAndEnabled)
				{
					collidersInsideRemove.Add(collider);
					continue;
				}

				if (!collider.InLight(this))
				{
					collidersInsideRemove.Add(collider);
				}
			}

			foreach(var collider in collidersInsideRemove)
			{
				collidersInside.Remove(collider);
				transform2D.ForceUpdate(); 

				if (eventPresetId > 0)
				{
					if (collider)
					{
						collider.lightOnExit?.Invoke(this);
					}
				}
			}

			collidersInsideRemove.Clear();
		}

		void UpdateBuffer()
		{
			UpdateCollidersInside();
			
			if (InCameras())
			{
				if (GetBuffer() == null)
				{
					return;
				}

				if (transform2D.UpdateNeeded || !inScreen)
				{
					BufferUpdate();

					inScreen = true;
				}
			}
				else
			{
				if (buffer != null)
				{
					Buffers.Manager.FreeBuffer(buffer);
				}
				
				inScreen = false;
			}
		}

		public void DrawMeshMode()
		{
			if (!meshMode.enable)
			{
				return;
			}

			if (buffer == null)
			{
				return;
			}

			if (!isActiveAndEnabled)
			{
				return;
			}

			if (!InCameras())
			{
				return;
			}
			
			var lightingMesh = MeshRendererManager.Pull(this);
			if (lightingMesh)
			{
				lightingMesh.UpdateLight(this, meshMode);
			}	
		}

		void OnDrawGizmosSelected()
		{
			if (Lighting2D.ProjectSettings.gizmos.drawGizmos != EditorDrawGizmos.Selected)
			{
				return;
			}
			
			Draw();
		}

		private void OnDrawGizmos()
		{
			if (Lighting2D.ProjectSettings.gizmos.drawGizmos == EditorDrawGizmos.Disabled)
			{
				return;
			}

			if (Lighting2D.ProjectSettings.gizmos.drawIcons == EditorIcons.Enabled)
			{
				UnityEngine.Gizmos.DrawIcon(transform.position, "light_v2", true);
			}

			if (Lighting2D.ProjectSettings.gizmos.drawGizmos != EditorDrawGizmos.Always)
			{
				return;
			}

			Draw();
		}

		void Draw()
		{
			if (!isActiveAndEnabled)
			{
				return;
			}
			
			UnityEngine.Gizmos.color = new Color(1f, 0.5f, 0.25f);
		
			if (applyRotation != Rotation.Disabled)
			{
				GizmosHelper.DrawCircle(transform.position, transform2D.rotation, 360, size); // spotAngle
			}
				else
			{
				GizmosHelper.DrawCircle(transform.position, 0, 360, size); // spotAngle
			}

			UnityEngine.Gizmos.color = new Color(0, 1f, 1f);
			
			switch(Lighting2D.ProjectSettings.gizmos.drawGizmosBounds)
			{
				case EditorGizmosBounds.Enabled:

					GizmosHelper.DrawRect(transform.position, transform2D.WorldRect);
					
				break;
			}
		}
	}
}