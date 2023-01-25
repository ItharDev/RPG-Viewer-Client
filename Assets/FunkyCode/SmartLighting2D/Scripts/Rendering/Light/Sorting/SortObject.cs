namespace FunkyCode.Rendering.Light.Sorting
{
	public struct SortObject : System.Collections.Generic.IComparer<SortObject>
	{
		public float Value; // value

		public object LightObject;

		public LightTilemapCollider2D Tilemap;

		public SortObject(float value, object lightObject, LightTilemapCollider2D tilemap = null)
		{
			this.Value = value;
			this.LightObject = lightObject;
			this.Tilemap = tilemap;
		}

		public int Compare(SortObject a, SortObject b)
		{
			if (a.Value > b.Value)
				return 1;

			return a.Value < b.Value ? -1 : 0;
		}

		private static System.Collections.Generic.IComparer<SortObject> comparer = (System.Collections.Generic.IComparer<SortObject>) new SortObject();

		public static System.Collections.Generic.IComparer<SortObject> Sort()
		{ 
			return comparer;
		}
	}
}