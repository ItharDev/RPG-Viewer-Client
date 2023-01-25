using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightingSettings;
using FunkyCode.Utilities;

namespace FunkyCode
{
	[ExecuteInEditMode]
	public class DayLightCollider2D : MonoBehaviour
	{
		public enum ShadowType {None, SpritePhysicsShape, Collider2D, SpriteOffset, SpriteProjection, SpriteProjectionShape, SpriteProjectionCollider, FillCollider2D, FillSpritePhysicsShape}; 
		public enum ShadowEffect {Softness, Falloff}
		public enum MaskType {None, Sprite, BumpedSprite};
		public enum MaskLit {Lit, LitAbove}
		public enum Depth {None, SortingOrder, ZPosition, YPosition, Custom}
		public enum DepthFalloff {Disabled, Enabled}

		public int shadowLayer = 0;
		public int maskLayer = 0;

		public ShadowType shadowType = ShadowType.SpritePhysicsShape;
		public MaskType maskType = MaskType.None;

		public ShadowEffect shadowEffect = ShadowEffect.Softness;

		[Min(0)]
		public float shadowDistance = 1;

		[Min(0)]
		public float shadowThickness = 1;

		[Min(0)]
		public float shadowSoftness = 0;

		[Range(0, 1)]
		public float shadowTranslucency = 0;

		public MaskLit maskLit = MaskLit.Lit;

		public Depth depth = Depth.None;

		public DepthFalloff depthFalloff = DepthFalloff.Disabled;

		public int depthCustomValue = 0;

		public DayLightColliderShape mainShape = new DayLightColliderShape();

		public DayNormalMapMode normalMapMode = new DayNormalMapMode();
		public SpriteMeshObject spriteMeshObject = new SpriteMeshObject();

		public bool isStatic => gameObject.isStatic;

		public static List<DayLightCollider2D> List = new List<DayLightCollider2D>();

		public void OnEnable()
		{
			List.Add(this);

			LightingManager2D.Get();

			Initialize();
		}

		public void OnDisable()
		{
			List.Remove(this);
		}

		public int GetDepth()
		{
			switch(depth)
			{
				case Depth.Custom:

					return(depthCustomValue);

				case Depth.SortingOrder:

					int sortDepth = mainShape.spriteShape.GetSortingOrder();

					sortDepth = Mathf.Max(Mathf.Min(sortDepth, 100), -100);

					return(sortDepth);

				case Depth.ZPosition:

					int zDepth = (int)transform.position.z;

					zDepth = Mathf.Max(Mathf.Min(zDepth, 100), -100);

					return(zDepth);

				case Depth.YPosition:

					int yDepth = (int)transform.position.y;

					zDepth = Mathf.Max(Mathf.Min(yDepth, 100), -100);

					return(zDepth);
			}

			return(0);
		}

		public bool InAnyCamera() // camera transform
		{
			List<CameraTransform> lightingCameras = CameraTransform.List;

			// Rect lightRect = transform2D.WorldRect;

			for(int i = 0; i < lightingCameras.Count; i++)
			{
				CameraTransform cameraTransform = lightingCameras[i];

				Camera camera = cameraTransform.Camera;

				if (camera == null)
				{
					continue;
				}

				float distance = Vector2.Distance(transform.position, camera.transform.position);
				float cameraRadius = CameraTransform.GetRadius(camera);

				// 5 = size
				// why not using rect overlap?
				float radius = cameraRadius + 5; 

				if (distance < radius)
				{
					return(true);
				}
			}

			return(false);
		}

		public static void ForceUpdateAll()
		{
			foreach(DayLightCollider2D collider in List)
			{
				collider.ForceUpdate();
			}
		}

		public void ForceUpdate() 
		{
			Initialize();

			mainShape.transform2D.updateNeeded = true;
		}

		public void UpdateLoop()
		{
			if (isStatic)
			{
				return;
			}
			
			mainShape.transform2D.Update();

			// ???

			if (mainShape.transform2D.updateNeeded)
			{
				mainShape.transform2D.updateNeeded = false;
			}
		}

		public void Initialize()
		{
			mainShape.shadowType = shadowType;
			mainShape.thickness = shadowThickness;
			mainShape.maskType = maskType;
			mainShape.height = shadowDistance;

			mainShape.isStatic = isStatic;

			mainShape.SetTransform(transform);
			mainShape.ResetLocal();

			mainShape.transform2D.Update();
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
			if (mainShape.shadowType != DayLightCollider2D.ShadowType.None)
			{
				UnityEngine.Gizmos.color = new Color(1f, 0.5f, 0.25f);
			
				switch(mainShape.shadowType)
				{
					case DayLightCollider2D.ShadowType.SpriteProjection:

						Vector2 pos = transform.position;
						float rot = Lighting2D.DayLightingSettings.direction * Mathf.Deg2Rad;

						Pair2 pair = Pair2.Zero();

						pair.A = pos + pair.A.Push(-rot + Mathf.PI / 2, shadowThickness);

						pair.B = pos + pair.B.Push(-rot - Mathf.PI / 2, shadowThickness);

						UnityEngine.Gizmos.DrawLine(pair.A, pair.B);

					break;

					case DayLightCollider2D.ShadowType.Collider2D:
					case DayLightCollider2D.ShadowType.SpritePhysicsShape:
					case DayLightCollider2D.ShadowType.SpriteProjectionShape:
					case DayLightCollider2D.ShadowType.SpriteProjectionCollider:
					case DayLightCollider2D.ShadowType.FillCollider2D:
					case DayLightCollider2D.ShadowType.FillSpritePhysicsShape:

						List<Polygon2> polygons = mainShape.GetPolygonsWorld();

						if (polygons != null)
						{
							GizmosHelper.DrawPolygons(polygons, transform.position);
						}

						if (mainShape.shadowType == DayLightCollider2D.ShadowType.SpriteProjectionShape)
						{	
							float direcion = Lighting2D.DayLightingSettings.direction * Mathf.Deg2Rad;

							foreach(Polygon2 polygon in polygons)
							{
								Pair2 axis = Polygon2Helper.GetAxis(polygon, direcion);

								UnityEngine.Gizmos.DrawLine(axis.A, axis.B);
							}
						}

					break;
				}

				switch(Lighting2D.ProjectSettings.gizmos.drawGizmosBounds)
				{
					case EditorGizmosBounds.Enabled:

						UnityEngine.Gizmos.color = new Color(0, 1f, 1f, 0.25f);

						switch(mainShape.shadowType)
						{
							case DayLightCollider2D.ShadowType.Collider2D:
							case DayLightCollider2D.ShadowType.SpritePhysicsShape:

								Rect bound = mainShape.GetShadowBounds();
								GizmosHelper.DrawRect(transform.position, bound);
						
							break;
						}
							
					break;
				}
			}
		}
	}
}