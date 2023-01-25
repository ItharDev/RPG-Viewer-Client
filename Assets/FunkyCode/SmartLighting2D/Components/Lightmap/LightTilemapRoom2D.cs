using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightTilemapCollider;
using FunkyCode.LightingSettings;

namespace FunkyCode
{
	[ExecuteInEditMode]
	public class LightTilemapRoom2D : MonoBehaviour
	{
		public int lightLayer = 0;
		public enum MaskType {Sprite}  // Separate For Each Map Type!
		public enum ShaderType {ColorMask, MultiplyTexture};
	
		public MapType mapType = MapType.UnityRectangle;
		public MaskType maskType = MaskType.Sprite;
		public ShaderType shaderType = ShaderType.ColorMask;
		public Color color = Color.black;

		public SuperTilemapEditorSupport.TilemapRoom2D superTilemapEditor = new SuperTilemapEditorSupport.TilemapRoom2D();
		public Rectangle rectangle = new Rectangle();

		public LightingTilemapRoomTransform lightingTransform = new LightingTilemapRoomTransform();
	
		public static List<LightTilemapRoom2D> List = new List<LightTilemapRoom2D>();

		public void OnEnable()
		{
			List.Add(this);

			LightingManager2D.Get();

			rectangle.SetGameObject(gameObject);
			superTilemapEditor.SetGameObject(gameObject);

			Initialize();
		}

		public void OnDisable()
		{
			List.Remove(this);
		}

		public LightTilemapCollider.Base GetCurrentTilemap()
		{
			switch(mapType)
			{
				case MapType.SuperTilemapEditor:
					return(superTilemapEditor);
				case MapType.UnityRectangle:
					return(rectangle);
			}
			return(null);
		}

		public void Initialize()
		{
			TilemapEvents.Initialize();
			
			GetCurrentTilemap().Initialize();
		}

		public void Update()
		{
			lightingTransform.Update(this);

			if (lightingTransform.UpdateNeeded)
			{
				GetCurrentTilemap().ResetWorld();

				Light2D.ForceUpdateAll();
			}
		}

		public TilemapProperties GetTilemapProperties()
		{
			return(GetCurrentTilemap().Properties);
		}

		public List<LightTile> GetTileList()
		{
			return(GetCurrentTilemap().MapTiles);
		}

		public float GetRadius()
		{
			return(GetCurrentTilemap().GetRadius());
		}

		void OnDrawGizmosSelected()
		{
			if (Lighting2D.ProjectSettings.gizmos.drawGizmos != EditorDrawGizmos.Selected)
			{
				return;
			}

			DrawGizmos();
		}

		private void OnDrawGizmos()
		{
			if (Lighting2D.ProjectSettings.gizmos.drawGizmos != EditorDrawGizmos.Always)
			{
				return;
			}

			DrawGizmos();
		}

		private void DrawGizmos()
		{
			if (!isActiveAndEnabled)
			{
				return;
			}

			// Gizmos.color = new Color(1f, 0.5f, 0.25f);

			UnityEngine.Gizmos.color = new Color(0, 1f, 1f);

			switch(Lighting2D.ProjectSettings.gizmos.drawGizmosBounds)
			{
				case EditorGizmosBounds.Enabled:

					GizmosHelper.DrawRect(transform.position, GetCurrentTilemap().GetRect());

				break;
			}
		}
	}
}
