using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightTilemapCollider;
using FunkyCode.LightingSettings;
using FunkyCode.Utilities;

namespace FunkyCode
{
	public class DayLightingTile
	{
		public List<Polygon2> polygons;
		public float height = 1;

		public Rect rect = new Rect();

		public Rect GetDayRect()
		{
			if (rect.width <= 0) {
				rect = Polygon2Helper.GetDayRect(polygons, height);
			}

			return(rect);
		}

		public bool InCamera(Camera camera)
		{
			Rect cameraRect = CameraTransform.GetWorldRect(camera);
			Rect tileRect = GetDayRect();

			if (cameraRect.Overlaps(tileRect))
			{
				return(true);
			}
	
			return(false);
		}

	}

	[ExecuteInEditMode]
	public class DayLightTilemapCollider2D : MonoBehaviour
	{
		public enum MaskLit {Lit, Unlit};
		public MapType tilemapType = MapType.UnityRectangle;

		public int shadowLayer = 0;

		public float shadowTranslucency = 0;

		public float shadowSoftness = 0;

		public ShadowTileType shadowTileType = ShadowTileType.AllTiles;

		public float height = 1;

		public int maskLayer = 0;

		public MaskLit maskLit = MaskLit.Lit;

		public DayLightTilemapColliderTransform transform2D = new DayLightTilemapColliderTransform();

		public Rectangle rectangle = new Rectangle();
		public Isometric isometric = new Isometric();
		public Hexagon hexagon = new Hexagon();

		public SuperTilemapEditorSupport.TilemapCollider2D superTilemapEditor = new SuperTilemapEditorSupport.TilemapCollider2D();

		public List<DayLightingTile> dayTiles = new List<DayLightingTile>();
	
		private static List<DayLightTilemapCollider2D> list = new List<DayLightTilemapCollider2D>();
		public static List<DayLightTilemapCollider2D> List => list;

		public bool ShadowsDisabled()
		{
			return(GetCurrentTilemap().ShadowsDisabled());
		}

		public bool MasksDisabled()
		{
			return(GetCurrentTilemap().MasksDisabled());
		}

		public void OnEnable()
		{
			list.Add(this);

			rectangle.SetGameObject(gameObject);
			isometric.SetGameObject(gameObject);
			hexagon.SetGameObject(gameObject);

			superTilemapEditor.eventsInit = false;
			superTilemapEditor.SetGameObject(gameObject);

			LightingManager2D.Get();

			Initialize();
		}

		public void OnDisable() {
			list.Remove(this);
		}

		void Update()
		{
			transform2D.Update(this);

			if (transform2D.moved)
			{
				transform2D.moved = false;

				foreach(DayLightingTile dayTile in dayTiles)
				{
					dayTile.height = height;
				}
			}
		}

		public LightTilemapCollider.Base GetCurrentTilemap()
		{
			switch(tilemapType) {
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
			TilemapEvents.Initialize();

			GetCurrentTilemap().Initialize();

			dayTiles.Clear();

			switch(tilemapType) {
				case MapType.SuperTilemapEditor:

					switch(superTilemapEditor.shadowTypeSTE)
					{
						case SuperTilemapEditorSupport.TilemapCollider.ShadowType.Grid:
						case SuperTilemapEditorSupport.TilemapCollider.ShadowType.TileCollider:
							foreach(LightTile tile in GetTileList())
							{
							DayLightingTile dayTile = new DayLightingTile();
							dayTile.height = height;

							dayTile.polygons = tile.GetWorldPolygons(GetCurrentTilemap());

							dayTiles.Add(dayTile);
						}

						break;

						#if (SUPER_TILEMAP_EDITOR)

						case SuperTilemapEditorSupport.TilemapCollider.ShadowType.Collider:
						
								foreach(Polygon2 polygon in superTilemapEditor.GetWorldColliders()) {
									DayLightingTile dayTile = new DayLightingTile();

									dayTile.height = height;

									dayTile.polygons = new List<Polygon2>();
									
									Polygon2 poly = polygon.Copy();
									poly.ToOffsetSelf(transform.position);

									dayTile.polygons.Add(poly);
								
									dayTiles.Add(dayTile);

								}

							
						break;

						#endif
					}

					
				break;

				case MapType.UnityRectangle:

					switch(rectangle.shadowType)
					{
						case LightTilemapCollider.ShadowType.Grid:
						case LightTilemapCollider.ShadowType.SpritePhysicsShape:

							foreach(LightTile tile in GetTileList()) {
								DayLightingTile dayTile = new DayLightingTile();
								dayTile.height = height;

								dayTile.polygons = tile.GetWorldPolygons(GetCurrentTilemap());

								dayTiles.Add(dayTile);
							}

						break;

						case LightTilemapCollider.ShadowType.CompositeCollider:

							foreach(Polygon2 polygon in rectangle.compositeColliders)
							{
								DayLightingTile dayTile = new DayLightingTile();
								dayTile.height = height;

								dayTile.polygons = new List<Polygon2>();
								
								Polygon2 poly = polygon.Copy();
								poly.ToOffsetSelf(transform.position);

								dayTile.polygons.Add(poly);
							
								dayTiles.Add(dayTile);
							}

						break;
					}
					
				break;
			}
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

			UnityEngine.Gizmos.color = new Color(1f, 0.5f, 0.25f);

			LightTilemapCollider.Base tilemap = GetCurrentTilemap();

			foreach(DayLightingTile dayTile in dayTiles)
			{
				GizmosHelper.DrawPolygons(dayTile.polygons, transform.position);
			}
			
			switch(Lighting2D.ProjectSettings.gizmos.drawGizmosBounds)
			{
				case EditorGizmosBounds.Enabled:
				
					UnityEngine.Gizmos.color = new Color(0, 1f, 1f, 0.25f);

					foreach(DayLightingTile dayTile in dayTiles)
					{
						GizmosHelper.DrawRect(Vector2.zero, dayTile.GetDayRect());
					}

					UnityEngine.Gizmos.color = new Color(0, 1f, 1f, 0.5f);

					GizmosHelper.DrawRect(transform.position, tilemap.GetRect());

				break;
			}
		}
	}
}