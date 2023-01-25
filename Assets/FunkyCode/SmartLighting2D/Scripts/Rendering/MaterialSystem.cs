using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode
{
	public struct MaterialPass
	{
		public Material material;
		public Vector4 rect;
		public Texture texture;
		public Color color;
		public float rotation;
		public bool isSceneView;
		public int passId;
	}

    public static class MaterialSystem
	{
		public static MaterialPass[] materialPasses = new MaterialPass[32];

		public static int Count = 0;

       	public static void Clear()
		{
			Count = 0;
		}

		public static void Add(Material material, bool isSceneView, int passId, Camera camera, LightTexture lightTexture, LightmapPreset lightmapPreset)
		{
			float ratio = (float)camera.pixelRect.width / camera.pixelRect.height;

			float x = camera.transform.position.x;
			float y = camera.transform.position.y;

			// z = width ; w = height
			float w = camera.orthographicSize * 2;
			float z = w * ratio;

			float rotation = camera.transform.eulerAngles.z * Mathf.Deg2Rad;

			var rect = new Vector4(x, y, z, w);

			var c = lightmapPreset.darknessColor;

			var color = new Vector4(c.r, c.g, c.b, c.a);

			if (lightTexture == null)
			{
				Debug.Log("light texture null");
				return;
			}

			Texture texture = lightTexture.renderTexture;

			if (Lighting2D.ProjectSettings.shaderPreview == ShaderPreview.Enabled)
			{
				texture = LightmapShaders.GetPreviewTexture();
				color = Color.black;
				rect = new Vector4(0, 0, 1, 1);
			}

			var materialPass = new MaterialPass();

			materialPass.isSceneView = isSceneView;
			materialPass.color = color;
			materialPass.texture = texture;
			materialPass.rect = rect;
			materialPass.material = material;
			materialPass.rotation = rotation;
			materialPass.passId = passId;

			materialPasses[Count] = materialPass;

			Count ++;
		}
    }
}