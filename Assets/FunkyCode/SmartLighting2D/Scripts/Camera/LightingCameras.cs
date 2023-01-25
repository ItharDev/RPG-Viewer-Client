using UnityEngine;

namespace FunkyCode
{
	[System.Serializable]
	public class LightingCameras
	{
		[SerializeField]
		public CameraSettings[] cameraSettings;

		public int Length => cameraSettings.Length;

		public static int count = 0;

		public LightingCameras()
		{
			cameraSettings = new CameraSettings[1];
			cameraSettings[0] = new CameraSettings(0);

			count++;
		}

		public CameraSettings Get(int id)
		{
			cameraSettings[id].id = id;
			
			return cameraSettings[id];
		}

		public void Set(int id, CameraSettings settings)
		{
			cameraSettings[id] = settings;
		}
	}
}