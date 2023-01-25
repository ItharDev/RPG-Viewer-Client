using UnityEditor;

namespace FunkyCode
{
	[CustomEditor(typeof(LightingSettings.Profile))]
	public class ProfileEditor2 : Editor
	{
		override public void OnInspectorGUI()
		{
			LightingSettings.Profile profile = target as LightingSettings.Profile;

			ProfileEditor.DrawProfile(profile);
		}
	}
}