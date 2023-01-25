using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode
{
	public static class Lighting2D
	{
		public const int VERSION = 20221100;
		public const string VERSION_STRING = "2022.11.0";

		static public Lighting2DMaterials Materials = new Lighting2DMaterials();
		
		static public bool Disable => false;

		// lightmaps
		static public LightmapPreset[] LightmapPresets => Profile.lightmapPresets.list;

		// quality
		static public LightingSettings.QualitySettings QualitySettings => Profile.qualitySettings;
		
		// day lighting
		static public DayLightingSettings DayLightingSettings => Profile.dayLightingSettings;

		static public RenderingMode RenderingMode => ProjectSettings.renderingMode;

		static public CoreAxis CoreAxis => Profile.qualitySettings.coreAxis;

		// set & get
		static public Color DarknessColor
		{
			set => LightmapPresets[0].darknessColor = value;
			get => LightmapPresets[0].darknessColor;
		}

		static public float Resolution
		{
			set => LightmapPresets[0].resolution = value;
			get => LightmapPresets[0].resolution;
		}

		// methods
		static public void UpdateByProfile(Profile setProfile)
		{
			if (setProfile == null)
			{
				Debug.Log("Light 2D: Update Profile is Missing");
				return;
			}
			
			// set profile also
			profile = setProfile;
		}

		static public void RemoveProfile()
		{
			profile = null;
		}

		// profile
		static private Profile profile = null;
		static public Profile Profile
		{
			get
			{
				if (profile != null)
				{
					return(profile);
				}

				if (ProjectSettings != null)
				{
					profile = ProjectSettings.Profile;
				}

				if (profile == null)
				{
					profile = Resources.Load("Profiles/Default Profile") as Profile;

					if (profile == null)
					{
						Debug.LogError("Light 2D: Default Profile not found");
					}
				}

				return(profile);
			}
		}

		static private ProjectSettings projectSettings = null;
		static public ProjectSettings ProjectSettings
		{
			get
			{
				if (projectSettings != null)
				{
					return(projectSettings);
				}

				projectSettings = Resources.Load("Settings/Project Settings") as ProjectSettings;
			
				return projectSettings;
			}
		}
	}
}