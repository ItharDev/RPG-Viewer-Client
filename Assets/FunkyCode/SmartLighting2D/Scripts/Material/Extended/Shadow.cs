using UnityEngine;

namespace FunkyCode.Lighting2DMaterial
{
	[System.Serializable]
	public class Shadow
	{
		private Sprite penumbraSprite;
		private Sprite penumbraSprite2;

		private LightingMaterial softShadow = null;
		private LightingMaterial softShadowDefault = null;

		private LightingMaterial alphaShadow = null;
		private LightingMaterial legacyGPUShadow = null;
		private LightingMaterial legacyCPUShadow = null;

		private LightingMaterial spriteProjection = null;

		private LightingMaterial dayCPUShadow = null;
		private LightingMaterial spriteShadow = null;

		private LightingMaterial depthDayShadow = null;

		private LightingMaterial softDistanceShadow = null;

		private LightingMaterial fastShadow = null;

		public void Reset()
		{
			penumbraSprite = null;

			softShadow = null;
			softShadowDefault = null;
			fastShadow = null;

			alphaShadow = null;
			legacyGPUShadow = null;
			legacyCPUShadow = null;

			depthDayShadow = null;
		
			dayCPUShadow = null;
			spriteProjection = null;

			spriteShadow = null;

			softDistanceShadow = null;
		}

		public void Initialize()
		{
			GetPenumbraSprite();
			GetPenumbraSprite2();

			GetSoftShadow();
			GetSoftDistanceShadow();

			GetLegacyGPUShadow();
			GetLegacyCPUShadow();

			GetDayCPUShadow();
			GetSpriteShadow();
		}

		public Material GetDepthDayShadow()
		{
			if (depthDayShadow == null || depthDayShadow.Get() == null)
			{
				depthDayShadow = LightingMaterial.Load("Light2D/Internal/Depth/DayShadow");
			}

			return(depthDayShadow.Get());
		}

		public Material GetAlphaShadow()
		{
			if (alphaShadow == null || alphaShadow.Get() == null)
			{
				alphaShadow = LightingMaterial.Load("Light2D/Internal/AlphaShadow");

				alphaShadow.SetTexture("textures/white");
			}

			return(alphaShadow.Get());
		}
	
		public Material GetSoftShadow()
		{
			if (softShadow == null || softShadow.Get() == null)
			{
				softShadow = LightingMaterial.Load("Light2D/Internal/Shadow/SoftShadow");
			}

			return(softShadow.Get());
		}

		public Material GetSoftShadowDefault()
		{
			if (softShadowDefault == null || softShadowDefault.Get() == null)
			{
				softShadowDefault = LightingMaterial.Load("Light2D/Internal/Shadow/SoftDefault");
			}

			return(softShadowDefault.Get());
		}

		public Material GetFastShadow()
		{
			if (fastShadow == null || fastShadow.Get() == null)
			{
				fastShadow = LightingMaterial.Load("Light2D/Internal/Shadow/Fast");
			}

			return(fastShadow.Get());
		}


		public Material GetLegacyGPUShadow()
		{
			if (legacyGPUShadow == null || legacyGPUShadow.Get() == null)
			{
				legacyGPUShadow = LightingMaterial.Load("Light2D/Internal/Shadow/LegacyGPU");

				if (legacyGPUShadow.Get() != null)
				{
					legacyGPUShadow.Get().mainTexture = GetPenumbraSprite().texture;
				}
			}

			return(legacyGPUShadow.Get());
		}

		public Material GetSoftDistanceShadow()
		{
			if (softDistanceShadow == null || softDistanceShadow.Get() == null)
			{
				softDistanceShadow = LightingMaterial.Load("Light2D/Internal/Shadow/SoftDistance");

				if (softDistanceShadow.Get() != null)
				{
					softDistanceShadow.Get().mainTexture = GetPenumbraSprite2().texture;
				}
			}

			return(softDistanceShadow.Get());
		}

		public Material GetLegacyCPUShadow()
		{
			if (legacyCPUShadow == null || legacyCPUShadow.Get() == null)
			{
				legacyCPUShadow = LightingMaterial.Load("Light2D/Internal/Shadow/LegacyCPU");

				if (legacyCPUShadow.Get() != null)
				{
					legacyCPUShadow.Get().mainTexture = GetPenumbraSprite().texture;
				}
			}

			return(legacyCPUShadow.Get());
		}

		public Sprite GetPenumbraSprite()
		{
			if (penumbraSprite == null)
			{
				penumbraSprite = Resources.Load<Sprite>("textures/penumbra"); 
			}

			return(penumbraSprite);
		}

		
		public Sprite GetPenumbraSprite2()
		{
			if (penumbraSprite2 == null)
			{
				penumbraSprite2 = Resources.Load<Sprite>("textures/penumbra2"); 
			}

			return(penumbraSprite2);
		}

		public Material GetDayCPUShadow()
		{
			if (dayCPUShadow == null || dayCPUShadow.Get() == null)
			{
				dayCPUShadow = LightingMaterial.Load("Light2D/Internal/Day/SoftShadow");
			}

			return(dayCPUShadow.Get());
		}

		public Material GetSpriteShadow()
		{
			if (spriteShadow == null || spriteShadow.Get() == null)
			{
				spriteShadow = LightingMaterial.Load("Light2D/Internal/SpriteShadow");
			}

			return(spriteShadow.Get());
		}

		public Material GetSpriteProjectionMaterial()
		{
			if (spriteProjection == null || spriteProjection.Get() == null)
			{
				spriteProjection = LightingMaterial.Load("Light2D/Internal/SpriteProjection");
			}
			
			return(spriteProjection.Get());
		}

	}
}