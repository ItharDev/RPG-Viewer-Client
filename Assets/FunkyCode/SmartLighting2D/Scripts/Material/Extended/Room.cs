using UnityEngine;

namespace FunkyCode.Lighting2DMaterial
{
	[System.Serializable]
	public class Room
	{
		private LightingMaterial roomMask = null;
		private LightingMaterial roomMultiply = null;

		public void Reset() {
			roomMask = null;
			roomMultiply = null;
		}

		public void Initialize() {
			GetRoomMask();
			GetRoomMultiply();
		}

		public Material GetRoomMask() {
			if (roomMask == null || roomMask.Get() == null) {
				roomMask = LightingMaterial.Load("Light2D/Internal/RoomMask");
			}
			return(roomMask.Get());
		}

		public Material GetRoomMultiply() {
			if (roomMultiply == null ||roomMultiply.Get() == null) {
			
				roomMultiply = LightingMaterial.Load("Light2D/Internal/RoomMultiply");
			
			}
			return(roomMultiply.Get());
		}
	}
}