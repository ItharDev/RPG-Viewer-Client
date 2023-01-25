using UnityEngine;

namespace FunkyCode.Rendering.Light
{
    public class SkinnedMesh
	{
        public static void Mask(Light2D light, LightCollider2D id, Material material, LayerSetting layerSetting)
		{
			if (!id.InLight(light))
			{
				return;
			}

			var shape = id.mainShape;
			var skinnedMeshRenderer = shape.skinnedMeshShape.GetSkinnedMeshRenderer();
			if (!skinnedMeshRenderer)
				return;

			var meshObject = shape.GetMeshes();
			if (meshObject == null)
				return;

			if (skinnedMeshRenderer.sharedMaterial)
				material.mainTexture = skinnedMeshRenderer.sharedMaterial.mainTexture;
			else
				material.mainTexture = null;

			Vector2 position = shape.transform2D.Position - light.transform2D.position;
			Vector2 pivotPosition = shape.GetPivotPoint() - light.transform2D.position;
			GLExtended.color = LayerSettingColor.Get(pivotPosition, layerSetting, id.maskLit, 1, id.maskLitCustom);

			material.SetPass(0);

			GLExtended.DrawMesh(meshObject, position, id.mainShape.transform2D.Scale, shape.transform2D.Rotation);

			material.mainTexture = null;
		}
    }
}