using UnityEngine;

namespace FunkyCode.LightingSettings
{
    [CreateAssetMenu(fileName = "Data", menuName = "Light 2D/Project", order = 2)]

    public class ProjectSettings : ScriptableObject
	{
		public RenderingMode renderingMode = RenderingMode.OnRender;

		public EditorView editorView;
		public Gizmos gizmos;

		public Chunks chunks;

		public ColorSpace colorSpace = ColorSpace.Gamma;

		public ManagerInternal managerInternal = ManagerInternal.HideInHierarchy;

		public ManagerInstance managerInstance = ManagerInstance.Static;
		
		public ShaderPreview shaderPreview = ShaderPreview.Disabled;

		public int MaxLightSize = 100;

		public MaterialOffScreen materialOffScreen = MaterialOffScreen.Unlit;

		public bool disable;

		public Profile profile;
        public Profile Profile
		{
			get
			{
				if (profile != null)
				{
					return(profile);
				}
		
				profile = Resources.Load("Profiles/Default Profile") as Profile;

				if (profile == null)
				{
					Debug.LogError("Light 2D Project Settings: Default Profile not found");
				}
			
				return(profile);
			}

			set
			{
				profile = value;
			}
		}

		public ProjectSettings()
		{
			chunks = new Chunks();

			gizmos = new Gizmos();

			colorSpace = ColorSpace.Gamma;

			disable = false;
		}
    }
}