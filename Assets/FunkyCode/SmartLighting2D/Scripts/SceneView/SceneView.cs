using UnityEngine;

//#if UNITY_EDITOR
//	using UnityEditor;
//#endif

namespace FunkyCode
{
    public class SceneView
    {
        public SceneView()
        {
        }

		#if UNITY_EDITOR
			private void OnSceneView(UnityEditor.SceneView sceneView)
			{
				var manager = LightingManager2D.Get();

				if (!IsSceneViewActive())
				{
					return;
				}

				Rendering.Manager.Main.InternalUpdate();

				Rendering.Manager.Main.Render();
			}
		#endif

        public void OnDisable()
        {
            #if UNITY_EDITOR

				#if UNITY_2019_1_OR_NEWER

					UnityEditor.SceneView.beforeSceneGui -= OnSceneView;
					//SceneView.duringSceneGui -= OnSceneView;

				#else

					UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneView;

				#endif

			#endif
        }

        public void OnEnable()
        {
            #if UNITY_EDITOR

				#if UNITY_2019_1_OR_NEWER

					UnityEditor.SceneView.beforeSceneGui += OnSceneView;
					//SceneView.duringSceneGui += OnSceneView;

				#else

					UnityEditor.SceneView.onSceneGUIDelegate += OnSceneView;

				#endif	
			#endif	
        }

		public bool IsSceneViewActive() // overlay
		{
			var manager = LightingManager2D.Get();

			for(int i = 0; i < manager.cameras.Length; i++)
			{
				var cameraSetting = manager.cameras.Get(i);

				for(int b = 0; b < cameraSetting.Lightmaps.Length; b++)
				{
					var cameraLightmap = cameraSetting.GetLightmap(b);

					if (cameraLightmap.sceneView == CameraLightmap.SceneView.Enabled)
					{
						return true;
					}
				}
			}
			
			return false;
		}
    }
}