using System.Collections.Generic;
using FunkyCode.LightingSettings;

namespace FunkyCode.EventHandling
{
    public class Object
	{
        public List<LightCollider2D> listenersCache = new List<LightCollider2D>();
		
		public List<LightCollision2D> listenersInLight = new List<LightCollision2D>();
		public List<LightCollider2D> listenersInLightColliders = new List<LightCollider2D>();

		public event CollisionEvent2D collisionEvents;

		public void Update(Light2D light, EventPreset eventPreset)
		{
			listenersInLight.Clear();

			// Get Event Receivers
			LightCollider.GetCollisions(listenersInLight, light);

			// Remove Event Receiver Vertices with Shadows
			LightCollider.RemoveHiddenPoints(listenersInLight, light, eventPreset);
			LightTilemap.RemoveHiddenPoints(listenersInLight, light, eventPreset);

			if (listenersInLight.Count < 1)
			{
				for(int i = 0; i < listenersCache.Count; i++)
				{
					var collider = listenersCache[i];
					
					var collision = new LightCollision2D();
					collision.light = light;
					collision.collider = collider;
					collision.points = null;
					collision.state = LightCollision2D.State.OnCollisionExit;

					collisionEvents?.Invoke(collision);

					collider.CollisionEvent(collision);
				}

				listenersCache.Clear();

				return;
			}
				
			listenersInLightColliders.Clear();

			foreach(var collision in listenersInLight)
			{
				listenersInLightColliders.Add(collision.collider);
			}

			for(int i = 0; i < listenersCache.Count; i++)
			{
				var collider = listenersCache[i];

				if (!listenersInLightColliders.Contains(collider))
				{
					var collision = new LightCollision2D();
					collision.light = light;
					collision.collider = collider;
					collision.points = null;
					collision.state = LightCollision2D.State.OnCollisionExit;

					collider.CollisionEvent(collision);

					collisionEvents?.Invoke(collision);
					
					listenersCache.Remove(collider);
				}
			}

			for(int i = 0; i < listenersInLight.Count; i++)
			{
				var collision = listenersInLight[i];
				
				if (listenersCache.Contains(collision.collider))
				{
					collision.state = LightCollision2D.State.OnCollision;
				}
				else
				{
					collision.state = LightCollision2D.State.OnCollisionEnter;
					listenersCache.Add(collision.collider);
				}
			
				collision.collider.CollisionEvent(collision);

				collisionEvents?.Invoke(collision);
			}
		}
	}
}