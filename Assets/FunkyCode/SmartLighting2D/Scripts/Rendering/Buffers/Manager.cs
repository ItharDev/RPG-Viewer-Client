using UnityEngine;

namespace FunkyCode.Buffers
{
    public static class Manager
    {
		static public LightBuffer2D AddBuffer(Light2D light)
		{
			Vector2Int textureSize = light.GetTextureSize();

			LightBuffer2D LightBuffer2D = new LightBuffer2D ();
			LightBuffer2D.Light = light;

			LightBuffer2D.Initiate (textureSize);

			return(LightBuffer2D);
		}

		static public LightBuffer2D PullBuffer(Light2D light)
		{
			Vector2Int textureSize = light.GetTextureSize();

			foreach (LightBuffer2D id in LightBuffer2D.List)
			{
				if (id.Free && id.renderTexture.width == textureSize.x && id.renderTexture.height == textureSize.y)
				{
					id.Light = light;

					light.ForceUpdate();
					
					return(id);
				}
			}
				
			return(AddBuffer(light));		
		}

		static public void FreeBuffer(LightBuffer2D buffer)
        {
			if (buffer == null)
			{
				return;
			}

			if (buffer.Light != null)
			{
				buffer.Light.Buffer = null;

				buffer.Light = null;
			}

			buffer.updateNeeded = false;
		}
    }
}