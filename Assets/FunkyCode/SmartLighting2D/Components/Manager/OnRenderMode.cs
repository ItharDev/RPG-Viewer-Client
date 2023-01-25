using FunkyCode.LightingSettings;
using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode
{
    [ExecuteInEditMode]
    public class OnRenderMode : LightingMonoBehaviour
    {
        public LightMainBuffer2D mainBuffer;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;

        public static List<OnRenderMode> List = new List<OnRenderMode>();

        public void OnEnable()
        {
            List.Add(this);
        }

        public void OnDisable()
        {
            List.Remove(this);   
        }

        public static OnRenderMode Get(LightMainBuffer2D buffer)
        {
            foreach(OnRenderMode meshModeObject in List)
            {
                if (meshModeObject.mainBuffer == buffer)
                {
                    return(meshModeObject);
                }
            }

            GameObject meshRendererMode = new GameObject("On Render");
            OnRenderMode onRenderMode = meshRendererMode.AddComponent<OnRenderMode>();

            onRenderMode.mainBuffer = buffer;
            onRenderMode.Initialize(buffer);
            onRenderMode.UpdateLayer();

            if (Lighting2D.ProjectSettings.managerInternal == LightingSettings.ManagerInternal.HideInHierarchy)
            {
                meshRendererMode.hideFlags = meshRendererMode.hideFlags | HideFlags.HideInHierarchy;
            }

            onRenderMode.name = "On Render: " + buffer.name;

            return(onRenderMode);
        }

        public void Initialize(LightMainBuffer2D mainBuffer)
        {
            if (mainBuffer == null)
            {
                Debug.Log("main buffer null");
            }

            gameObject.transform.parent = LightingManager2D.Get().transform;
            
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = mainBuffer.GetMaterial();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.allowOcclusionWhenDynamic = false;

            LightmapPreset lightmapPreset = mainBuffer.GetLightmapPreset();

            if (lightmapPreset != null)
            {
                mainBuffer.cameraLightmap.sortingLayer.ApplyToMeshRenderer(meshRenderer);
            }
                else
            {
                Debug.Log("light preset null");
            }

            UpdatePosition();

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = LightingRender2D.GetMesh();
        }

        private void Update()
        {
            if (mainBuffer == null || !mainBuffer.IsActive)
            {
                DestroySelf();
                return;
            }

            if (mainBuffer.cameraSettings.GetCamera() == null)
            {
                DestroySelf();
                return;
            }

            if (Lighting2D.RenderingMode != RenderingMode.OnRender)
            {
                DestroySelf();
                return;
            }
        }

        public void UpdateLoop()
        {
            if (mainBuffer == null || !mainBuffer.IsActive)
            {
                return;
            }

            if (mainBuffer.cameraSettings.GetCamera() == null)
            {
                return;
            }

            if (Lighting2D.RenderingMode != RenderingMode.OnRender)
            {
                return;
            }

            UpdateLayer();

            if (Lighting2D.Disable)
            {
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
                }
            }
            
            if (mainBuffer.cameraLightmap.overlay != CameraLightmap.Overlay.Enabled)
            {
                meshRenderer.enabled = false;
            }
            
            if (mainBuffer.cameraLightmap.rendering != CameraLightmap.Rendering.Enabled)
            {
                
                meshRenderer.enabled = false;
            }
        
            if (Lighting2D.RenderingMode == RenderingMode.OnRender)
            {
                UpdatePosition();
            }
        }

        void UpdateLayer()
        {
            gameObject.layer = (mainBuffer != null) ? mainBuffer.cameraSettings.GetLayerId(mainBuffer.cameraLightmap.id) : 0;
        }

        public void UpdatePosition()
        {
            Camera camera = mainBuffer.cameraSettings.GetCamera();
            
            if (camera == null)
            {
                return;
            }

            switch(mainBuffer.cameraLightmap.overlayPosition)
            {
                case CameraLightmap.OverlayPosition.Camera:

                    transform.position = LightingPosition.GetCameraPlanePosition(camera);

                break;

                case CameraLightmap.OverlayPosition.Custom:

                    transform.position = LightingPosition.GetCameraCustomPosition(camera, mainBuffer.cameraLightmap.customPosition);

                break;
            }
            
            transform.rotation = camera.transform.rotation;

            transform.localScale = LightingRender2D.GetSize(camera);
        }
    }
}