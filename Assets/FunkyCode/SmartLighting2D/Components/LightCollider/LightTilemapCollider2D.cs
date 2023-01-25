using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightingSettings;
using FunkyCode.LightTilemapCollider;

namespace FunkyCode
{
	public enum ShadowTileType {AllTiles, ColliderOnly};

	[ExecuteInEditMode]
	public class LightTilemapCollider2D : MonoBehaviour
	{
		public MapType mapType = MapType.UnityRectangle;

		public int shadowLayer = 0;
		public int maskLayer = 0;
		
		public float shadowTranslucency = 0;
		
		public ShadowTileType shadowTileType = ShadowTileType.AllTiles;

		public BumpMapMode bumpMapMode = new BumpMapMode();

		public Rectangle rectangle = new Rectangle();
		public Isometric isometric = new Isometric();
		public Hexagon hexagon = new Hexagon();

		public SuperTilemapEditorSupport.TilemapCollider2D superTilemapEditor = new SuperTilemapEditorSupport.TilemapCollider2D();

		public LightTilemapTransform lightingTransform = new LightTilemapTransform();

		public static List<LightTilemapCollider2D> List = new List<LightTilemapCollider2D>();
		public static LightColliderLayer<LightTilemapCollider2D> layerManagerMask = new LightColliderLayer<LightTilemapCollider2D>();
		public static LightColliderLayer<LightTilemapCollider2D> layerManagerCollision = new LightColliderLayer<LightTilemapCollider2D>();

		private int listMaskLayer = -1;
		private int listCollisionLayer = -1;

		static public List<LightTilemapCollider2D> GetMaskList(int layer)
		{
			return(layerManagerMask.layerList[layer]);
		}

		static public List<LightTilemapCollider2D> GetShadowList(int layer)
		{
			return(layerManagerCollision.layerList[layer]);
		}

		// Layer List
		void ClearLayerList()
		{
			layerManagerMask.Remove(listMaskLayer, this);
			layerManagerCollision.Remove(listCollisionLayer, this);
		
			listMaskLayer = -1;
			listCollisionLayer = -1;
		}

		void UpdateLayerList()
		{
			listMaskLayer = layerManagerMask.Update(listMaskLayer, maskLayer, this);
			listCollisionLayer = layerManagerCollision.Update(listCollisionLayer, shadowLayer, this);
		}

		public bool ShadowsDisabled()
		{
			return(GetCurrentTilemap().ShadowsDisabled());
		}

		public bool MasksDisabled()
		{
			return(GetCurrentTilemap().MasksDisabled());
		}

		public bool InLight(Light2D light)
		{
			Rect tilemapRect = GetCurrentTilemap().GetRect();
			Rect lightRect = light.transform2D.WorldRect;

			return(tilemapRect.Overlaps(lightRect));
		}

		public void RefreshTile(Vector3Int position)
		{
			switch(mapType)
			{
				case MapType.UnityRectangle:
					rectangle.RefreshTile(position);
				break;
			}
		}

		public void OnEnable() {
			List.Add(this);

			UpdateLayerList();

			LightingManager2D.Get();

			rectangle.SetGameObject(gameObject);
			isometric.SetGameObject(gameObject);
			hexagon.SetGameObject(gameObject);

			superTilemapEditor.eventsInit = false;
			superTilemapEditor.SetGameObject(gameObject);

			Initialize();

			Light2D.ForceUpdateAll();
		}

		public void OnDisable()
		{
			List.Remove(this);

			ClearLayerList();

			Light2D.ForceUpdateAll();
		}

		public void Update()
		{
			UpdateLayerList();

			lightingTransform.Update(this);

			if (lightingTransform.UpdateNeeded)
			{
				GetCurrentTilemap().ResetWorld();

				// Update if light is in range
				foreach(Light2D light in Light2D.List)
				{
					//if (IsInRange(light)) {
						light.ForceUpdate();
					//}
				}
			}
		}

		/*
		public bool IsInRange(Light2D light) {
			float radius = GetCurrentTilemap().GetRadius() + light.size;
			float distance = Vector2.Distance(light.transform.position, transform.position);

			return(distance < radius);
		}*/

		//public bool IsNotInRange(Light2D light) {
			//float radius = GetCurrentTilemap().GetRadius() + light.size;
			//float distance = Vector2.Distance(light.transform.position, transform.position);

			//return(distance > radius);

		//	return(false);
		//}

		public LightTilemapCollider.Base GetCurrentTilemap()
		{
			switch(mapType)
			{
				case MapType.SuperTilemapEditor:
					return(superTilemapEditor);

				case MapType.UnityRectangle:
					return(rectangle);

				case MapType.UnityIsometric:
					return(isometric);

				case MapType.UnityHexagon:
					return(hexagon);
			}

			return(null);
		}

		public void Initialize()
		{
			rectangle.SetGameObject(gameObject);
			isometric.SetGameObject(gameObject);
			hexagon.SetGameObject(gameObject);

			TilemapEvents.Initialize();

			GetCurrentTilemap().Initialize();
		}

		public List<LightTile> GetTileList()
		{
			return(GetCurrentTilemap().MapTiles);
		}

		public TilemapProperties GetTilemapProperties()
		{
			return(GetCurrentTilemap().Properties);
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

			LightTilemapCollider.Base tilemap = GetCurrentTilemap();

			switch(Lighting2D.ProjectSettings.gizmos.drawGizmosShadowCasters)
			{
				case EditorShadowCasters.Enabled:

					UnityEngine.Gizmos.color = new Color(1f, 0.5f, 0.25f);

					foreach(LightTile tile in GetTileList())
					{
						GizmosHelper.DrawPolygons(tile.GetWorldPolygons(tilemap), transform.position);
					}

					// GizmosHelper.DrawPolygons(superTilemapEditor.GetWorldColliders(), transform.position);

				break;
			}

			switch(Lighting2D.ProjectSettings.gizmos.drawGizmosChunks)
			{
				case EditorChunks.Enabled:

					UnityEngine.Gizmos.color = new Color(1, 0.5f, 0.75f);

					Rect rect = GetCurrentTilemap().GetRect();

					Vector2Int pos0 = Chunks.TilemapManager.TransformBounds(new Vector2(rect.x, rect.y));
					Vector2Int pos1 = Chunks.TilemapManager.TransformBounds(new Vector2(rect.x + rect.width, rect.y + rect.height));

					// Lighting2D.ProjectSettings.chunks.chunkSize
					int chunkSize = Chunks.TilemapManager.ChunkSize;

					for(int i = pos0.x; i <= pos1.x + 1; i++ ) {
						Vector2 lineA = new Vector2(i * chunkSize, pos0.y * chunkSize);
						Vector2 lineB = new Vector2(i * chunkSize, (pos1.y + 1) * chunkSize);
						UnityEngine.Gizmos.DrawLine(lineA, lineB);
					}

					for(int i = pos0.y; i <= pos1.y + 1; i++ ) {
						Vector2 lineA = new Vector2(pos0.x * chunkSize, i * chunkSize);
						Vector2 lineB = new Vector2((pos1.x + 1) * chunkSize, i * chunkSize);
						UnityEngine.Gizmos.DrawLine(lineA, lineB);
					}

				break;
			}

			switch(Lighting2D.ProjectSettings.gizmos.drawGizmosBounds)
			{
				case EditorGizmosBounds.Enabled:

					UnityEngine.Gizmos.color = new Color(0, 1f, 1f);
		
					Rect rect = GetCurrentTilemap().GetRect();

					GizmosHelper.DrawRect(transform.position, rect);

				break;
			}
		}
	}
}