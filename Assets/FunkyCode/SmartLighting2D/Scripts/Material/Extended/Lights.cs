using UnityEngine;

namespace FunkyCode.Lighting2DMaterial
{
	[System.Serializable]
	public class Lights
	{
		private LightingMaterial pointLight = null;
		private LightingMaterial spriteLight = null;
		private LightingMaterial freeFormLight = null;
		private LightingMaterial freeFormLightEdge = null;

		private LightingMaterial pointOcclusion = null;
		private LightingMaterial lightOcclusion = null;
		private LightingMaterial freeformOcclusion = null;

		public void Reset()
		{
			pointOcclusion = null;
			lightOcclusion = null;
			freeformOcclusion = null;


			spriteLight = null;
			pointLight = null;
			freeFormLight = null;
			freeFormLightEdge = null;
		}

		public void Initialize()
		{
			Reset();

			GetPointOcclusion();
			GetLightOcclusion(); // sprite light occlusion
			GetFreeFormOcclusion();

			GetPointLight();
			GetSpriteLight();
			GetFreeFormLight();
			GetFreeFormEdgeLight();
		}

		public Material GetPointLight()
		{
			if (pointLight == null || pointLight.Get() == null)
			{
				pointLight = LightingMaterial.Load("Light2D/Internal/Light/PointLight");
			}

			return(pointLight.Get());
		}

		public Material GetSpriteLight()
		{
			if (spriteLight == null || spriteLight.Get() == null)
			{
				spriteLight = LightingMaterial.Load("Light2D/Internal/Light/SpriteLight");
			}

			return(spriteLight.Get());
		}

		public Material GetFreeFormLight()
		{
			if (freeFormLight == null || freeFormLight.Get() == null)
			{
				freeFormLight = LightingMaterial.Load("Light2D/Internal/Light/FreeFormLight");
			}

			return(freeFormLight.Get());
		}

		public Material GetFreeFormEdgeLight()
		{
			if (freeFormLightEdge == null || freeFormLightEdge.Get() == null)
			{
				freeFormLightEdge = LightingMaterial.Load("Light2D/Internal/Light/FreeFormFalloff");
			}
		
			return(freeFormLightEdge.Get());
		}

		public Material GetLightOcclusion()
		{
			if (lightOcclusion == null || lightOcclusion.Get() == null)
			{
				lightOcclusion = LightingMaterial.Load("Light2D/Internal/Light/SpriteLightOcclusion");
			}

			return(lightOcclusion.Get());
		}

		public Material GetPointOcclusion()
		{
			if (pointOcclusion == null || pointOcclusion.Get() == null)
			{
				pointOcclusion = LightingMaterial.Load("Light2D/Internal/Light/PointOcclusion");
			}

			return(pointOcclusion.Get());
		}

		public Material GetFreeFormOcclusion()
		{
			if (freeformOcclusion == null || freeformOcclusion.Get() == null)
			{
				freeformOcclusion = LightingMaterial.Load("Light2D/Internal/Light/FreeFormOcclusion");
			}

			return(freeformOcclusion.Get());
		}
	}
}