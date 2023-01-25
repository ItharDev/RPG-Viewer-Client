using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode
{
	public class LightBuffer2D
	{
		public string name = "Unknown";

		private Light2D light;

		public Light2D Light
		{
			get => light;

			set
			{
				light = value;

				Rendering.LightBuffer.UpdateName(this);
			}
		}

		public bool Free => !light;

		public LightTexture renderTexture;

		public LightTexture translucencyTexture;
		public LightTexture translucencyTextureBlur;

		public LightTexture freeFormTexture;

		public bool updateNeeded = false;

		public static List<LightBuffer2D> List = new List<LightBuffer2D>();

		public LightBuffer2D()
		{
			List.Add(this);
		}

		public static void Clear()
		{
			foreach(var buffer in new List<LightBuffer2D>(List))
			{
				if (buffer.light)
				{
					buffer.light.Buffer = null;
				}

				buffer.DestroySelf();
			}

			List.Clear();
		}

		public void DestroySelf()
		{
			List.Remove(this);

			if (renderTexture == null)
			{
				return;
			}

			if (renderTexture.renderTexture == null)
			{
				return;
			}
				
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(renderTexture.renderTexture);
			}
				else
			{
				UnityEngine.Object.DestroyImmediate(renderTexture.renderTexture);
			}
		}

		public void Initiate(Vector2Int textureSize)
		{
			Rendering.LightBuffer.InitializeRenderTexture(this, textureSize);
		}

		public void Render()
		{
			if (renderTexture.renderTexture == null)
			{
				return;
			}

			if (!updateNeeded)
			{
				return;
			}
			
			updateNeeded = false;

			if (light == null)
			{
				return;
			}

			if (light.translucentLayer > 0)
			{
				if (translucencyTexture == null)
				{
					Vector2Int effectTextureSize = LightingRender2D.GetTextureSize(Lighting2D.Profile.qualitySettings.lightEffectTextureSize);
					
					Rendering.LightBuffer.InitializeTranslucencyTexture(this, effectTextureSize);
				}
				
				if (translucencyTexture != null)
				{
					RenderTexture previous2 = RenderTexture.active;

					RenderTexture.active = translucencyTexture.renderTexture;

					GL.Clear(false, true, Color.black);
			
					Rendering.LightBuffer.RenderTranslucency(light);

					RenderTexture.active = previous2;

					// blur texture blit

					float time = Time.realtimeSinceStartup;

					Material material;

					float textureSize = (float)translucencyTextureBlur.renderTexture.width / 128;

					float strength = light.maskTranslucencyStrength * textureSize * 10;

					switch(light.maskTranslucencyQuality)
					{
						case Light2D.MaskTranslucencyQuality.HighQuality:

							// vertical pass

							material = Lighting2D.Materials.GetMaskBlurVertical();

							material.SetFloat("_Strength", strength);

							Graphics.Blit(translucencyTexture.renderTexture, translucencyTextureBlur.renderTexture, material);

							material.SetFloat("_Strength", strength * 0.5f);

							Graphics.Blit(translucencyTextureBlur.renderTexture, translucencyTexture.renderTexture, material);

							material.SetFloat("_Strength", strength * 0.25f);

							Graphics.Blit(translucencyTexture.renderTexture, translucencyTextureBlur.renderTexture, material);
					
							// horizontal pass

							material = Lighting2D.Materials.GetMaskBlurHorizontal();

							material.SetFloat("_Strength", strength);

							Graphics.Blit(translucencyTextureBlur.renderTexture, translucencyTexture.renderTexture, material);

							material.SetFloat("_Strength", strength * 0.5f);

							Graphics.Blit(translucencyTexture.renderTexture, translucencyTextureBlur.renderTexture, material);

							material.SetFloat("_Strength", strength * 0.25f);

							Graphics.Blit(translucencyTextureBlur.renderTexture, translucencyTexture.renderTexture, material);		

						break;

						case Light2D.MaskTranslucencyQuality.MediumQuality:

							// vertical pass

							material = Lighting2D.Materials.GetMaskBlurVertical();

							material.SetFloat("_Strength", strength);

							Graphics.Blit(translucencyTexture.renderTexture, translucencyTextureBlur.renderTexture, material);

							material.SetFloat("_Strength", strength * 0.5f);

							Graphics.Blit(translucencyTextureBlur.renderTexture, translucencyTexture.renderTexture, material);
					
							// horizontal pass

							material = Lighting2D.Materials.GetMaskBlurHorizontal();

							material.SetFloat("_Strength", strength);

							Graphics.Blit(translucencyTexture.renderTexture, translucencyTextureBlur.renderTexture, material);

							material.SetFloat("_Strength", strength * 0.5f);

							Graphics.Blit(translucencyTextureBlur.renderTexture, translucencyTexture.renderTexture, material);

						break;


						case Light2D.MaskTranslucencyQuality.LowQuality:

							// vertical
		
							material = Lighting2D.Materials.GetMaskBlurVertical();

							material.SetFloat("_Strength", strength);

							Graphics.Blit(translucencyTexture.renderTexture, translucencyTextureBlur.renderTexture, material);

							// horizontal

							material = Lighting2D.Materials.GetMaskBlurHorizontal();

							material.SetFloat("_Strength", strength);

							Graphics.Blit(translucencyTextureBlur.renderTexture, translucencyTexture.renderTexture, material);

						break;
					}
				}
			}

			if (light.lightType == Light2D.LightType.FreeForm)
			{
				if (freeFormTexture == null)
				{
					Rendering.LightBuffer.InitializeFreeFormTexture(this, new Vector2Int(renderTexture.width, renderTexture.height));
				}

				// render only if there are free form changes

				if (freeFormTexture != null)
				{
					if (light.freeForm.UpdateNeeded)
					{
						light.freeForm.UpdateNeeded = false;

						var previous3 = RenderTexture.active;

						RenderTexture.active = freeFormTexture.renderTexture;

						GL.Clear(false, true, Color.black);
				
						Rendering.LightBuffer.RenderFreeForm(light);

						RenderTexture.active = previous3;
					}
				}
			}

			if (light.lightLayer >= 0)
			{
				var previous = RenderTexture.active;

				RenderTexture.active = renderTexture.renderTexture;

				GL.Clear(false, true, Color.black);

				Rendering.LightBuffer.Render(light);

				RenderTexture.active = previous;
			}
		}
	}
}