using UnityEngine;

namespace FunkyCode.Rendering.Lightmap
{        
    public class ParticleRenderer
	{
		static public void Draw(LightParticleSystem2D id, Camera camera)
		{
			ParticleSystem.Particle particle;
			Vector2 size, pos;

			var particleSystem = id.GetParticleSystem();
			if (particleSystem == null)
				return;

			var particleSystemRenderer = id.GetParticleSystemRenderer();
			if (particleSystemRenderer == null)
				return;

			var simulationSpace = particleSystem.main.simulationSpace;
			if (id.particleArray == null || id.particleArray.Length < particleSystem.main.maxParticles)
				id.particleArray = new ParticleSystem.Particle[particleSystem.main.maxParticles];

			Texture texture;
			if (id.customParticle)
			{
				texture = id.customParticle;
			}
			else
			{
				texture = particleSystemRenderer.sharedMaterial.mainTexture;
			}

			Vector2 pOffset = -camera.transform.position;
			float rotation = id.transform.eulerAngles.z * Mathf.Deg2Rad;
			Color color = id.color;
			Vector3 localScale = id.transform.localScale;

			switch(simulationSpace)
			{
				case ParticleSystemSimulationSpace.Local:
					pOffset.x += id.transform.position.x;
					pOffset.y += id.transform.position.y;
					break;
			}

			var material = Lighting2D.Materials.GetAdditive();
			material.mainTexture = texture;
			
			material.SetPass (0); 

			GL.Begin (GL.QUADS);

			GL.Color(color);

			int particlesAlive = particleSystem.GetParticles (id.particleArray);

			for (int p = 0; p < particlesAlive; p++)
			{
				particle = id.particleArray [p];

				if (particle.remainingLifetime < 0.1f )
				{
					continue;
				}

				size.x = (particle.GetCurrentSize(particleSystem) * id.scale) / 2;
				size.y = size.x;

				switch(simulationSpace)
				{
					case ParticleSystemSimulationSpace.Local:
					
						pos = particle.position;

						float angle = Mathf.Atan2(pos.y, pos.x) + rotation;
						float distance = pos.magnitude;

						pos.x = Mathf.Cos(angle) * distance;
						pos.y = Mathf.Sin(angle) * distance;

						pos.x *= localScale.x;
						pos.y *= localScale.y;

						break;

					case ParticleSystemSimulationSpace.World:

						pos = particle.position;

						break;

					default:

						pos = Vector2.zero;

						break;
				}

				pos.x += pOffset.x;
				pos.y += pOffset.y;

				//if (InCamera(camera, pos, size.x) == false) {
				//continue;
				//}

				if (id.useParticleColor)
				{
					Color pColor = particle.GetCurrentColor(particleSystem) * color;

					GL.Color(pColor);
				}

				Particle.DrawPass(pos, size, particle.rotation);
			}

			GL.End (); 
		}
    }
}