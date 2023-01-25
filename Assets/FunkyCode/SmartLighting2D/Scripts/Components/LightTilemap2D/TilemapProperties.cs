using UnityEngine;
using UnityEngine.Tilemaps;

namespace FunkyCode
{
	public class TilemapProperties
	{
		public Vector2 cellSize = new Vector2(1, 1);
		public Vector2 cellAnchor = new Vector2(0.5f, 0.5f);
		public Vector2 cellGap = new Vector2(1, 1);
		public Vector2 colliderOffset = new Vector2(0, 0);
		public BoundsInt area;

		public Tilemap tilemap;
		public Grid grid;

		public Transform transform;
	}
}
