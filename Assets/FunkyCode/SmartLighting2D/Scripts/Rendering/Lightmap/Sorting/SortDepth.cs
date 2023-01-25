namespace FunkyCode.Rendering.Lightmap.Sorting
{
	public struct SortObject : System.Collections.Generic.IComparer<SortObject>
	{
		public float Distance;

		public object LightObject;

		public SortObject(float distance, object lightObject)
		{
			this.Distance = distance;
			this.LightObject = lightObject;
		}

		public int Compare(SortObject a, SortObject b)
		{
			if (a.Distance > b.Distance)
				return 1;
		
			return a.Distance < b.Distance ? -1 : 0;
		}

		public static System.Collections.Generic.IComparer<SortObject> Sort()
		{      
			return (System.Collections.Generic.IComparer<SortObject>) new SortObject();
		}
	}
}