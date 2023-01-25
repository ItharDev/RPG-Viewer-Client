using UnityEngine.Events;

namespace FunkyCode.LightSettings
{
	public class LightEvent : UnityEvent <Light2D> {}

	public enum MaskLit {Lit, Unlit, LitAbove, Isometric, Custom}

	public enum LightLayerType {ShadowAndMask, ShadowOnly, MaskOnly}

	public enum LightLayerSorting {None, SortingLayerAndOrder, DistanceToLight, YDistanceToLight, YAxisLower, YAxisHigher, ZAxisLower, ZAxisHigher, Isometric};
	public enum LightLayerSortingIgnore {None, IgnoreAbove};

	public enum LightLayerShadowEffect {Default, Soft, LegacyCPU, LegacyGPU, PerpendicularProjection, SoftConvex, SoftVertex, SpriteProjection, Fast};
	public enum LightLayerMaskLit {AlwaysLit, AboveLit, NeverLit};

	public enum LayerSorting {None, ZAxisLower, ZAxisHigher, YAxisLower, YAxisHigher};
	public enum LayerType {ShadowsAndMask, ShadowsOnly, MaskOnly}

	public enum NormalMapTextureType
	{
		Texture,
		Sprite
	}

	public enum NormalMapType
	{
		PixelToLight,
		ObjectToLight
	}
}