using UnityEngine;

//#if UNITY_EDITOR

//    using UnityEditor;

//#endif

namespace FunkyCode
{
	[System.Serializable]
	public struct CameraSettings
	{
		public enum CameraType
		{
			MainCamera,
			Custom,
			SceneView
		};

		public static int initCount = 0;

		public int id;

		[SerializeField]
		private CameraLightmap[] lightmaps;

		public CameraLightmap[] Lightmaps
		{
			get
			{
				if (lightmaps == null)
				{
					lightmaps = new CameraLightmap[1];

					var manager = LightingManager2D.Get();
					var cameras = manager.cameras;

					cameras.Set(id, this);
				}

				return(lightmaps);
			}

			set => lightmaps = value;
		}

		public CameraLightmap GetLightmap(int index)
		{
			var buffer = lightmaps[index];
			buffer.id = index;
			return(buffer);
		}

		public CameraType cameraType;
		public Camera customCamera;

		public string GetTypeName()
		{
			switch(cameraType)
			{
				case CameraType.SceneView:

					return("Scene View");

				case CameraType.MainCamera:

					return("Main Camera Tag");

				case CameraType.Custom:

					return("Custom");

				default:

					return("Unknown");
			}
		}

		public int GetLayerId(int bufferId)
		{
			var lightmap = GetLightmap(bufferId);

			if (lightmap.overlayLayerType == CameraLightmap.OverlayLayerType.UnityLayer)
			{
				return lightmap.renderLayerId;
			}
			else
			{
				var camera = GetCamera();

				if (camera != null && cameraType == CameraType.SceneView)
				{
					return Lighting2D.ProjectSettings.editorView.sceneViewLayer;
				}
				else
				{
					return Lighting2D.ProjectSettings.editorView.gameViewLayer;
				}
			}
		}

		public CameraSettings(int id)
		{
			this.id = id;

			cameraType = CameraType.MainCamera;

			customCamera = null;
			
			lightmaps = new CameraLightmap[1];

			lightmaps[0] = new CameraLightmap(0);

			initCount ++;
		}

		public Camera GetCamera()
		{
			Camera camera = null;

			switch(cameraType)
			{
				case CameraType.MainCamera:

					camera = Camera.main;
					if (camera && !camera.orthographic)
							return null;

					return(camera);

				case CameraType.Custom:
					camera = customCamera;
					if (camera && !camera.orthographic)
						return null;
				
					return(customCamera);

				case CameraType.SceneView:
				
					#if UNITY_EDITOR

						var sceneView = UnityEditor.SceneView.lastActiveSceneView;

						if (sceneView != null)
						{
							camera = sceneView.camera; // .GetComponent<Camera>();

							#if UNITY_2019_1_OR_NEWER
							
								if (!UnityEditor.SceneView.lastActiveSceneView.sceneLighting)
								{
									camera = null;
								}

							#else
							
								if (!UnityEditor.SceneView.lastActiveSceneView.m_SceneLighting)
								{
									camera = null;
								}

							#endif
						}
		
						if (camera != null && !camera.orthographic)
							camera = null;

						return camera;

					#else
					
						return null;

					#endif
					
			}

			return null;
		}

		public override int GetHashCode()
		{
			return this.GetHashCode();
		}
	}
}