using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode
{
    [ExecuteInEditMode]
    public class LightEventListener : MonoBehaviour
    {
        public bool useDistance = false;

        public float visability = 0;
        
        public LightCollision2D? CollisionInfo = null;

        private LightCollider2D lightCollider;

        private void OnEnable()
        {
            lightCollider = GetComponent<LightCollider2D>();

            lightCollider?.AddEvent(CollisionEvent);
        }

        private void OnDisable()
        {
            lightCollider?.RemoveEvent(CollisionEvent);
        }

        private void CollisionEvent(LightCollision2D collision)
        {
            if (collision.points != null)
            {
                if (CollisionInfo == null)
                {
                    CollisionInfo = collision;
                }
                    else
                {
                    if (CollisionInfo.Value.points != null)
                    {
                        if (collision.points.Count >= CollisionInfo.Value.points.Count)
                        {
                            CollisionInfo = collision;
                        }
                            else if (CollisionInfo.Value.light == collision.light)
                        {
                            CollisionInfo = collision;
                        }
                    }
                }

            }
                else
            {
                CollisionInfo = null;
            }
        }

        private void Update()
        {
            visability = 0;

            if (CollisionInfo == null)
            {
                return;
            }

            if (CollisionInfo.Value.points != null)
            {
                Polygon2 polygon = lightCollider.mainShape.GetPolygonsLocal()[0];

                int pointsCount = polygon.points.Length;
                int pointsInView = CollisionInfo.Value.points.Count;

                visability = (((float)pointsInView / pointsCount));

                if (useDistance)
                {
                    if (CollisionInfo.Value.points.Count > 0)
                    {
                        float multiplier = 0;

                        for(int i = 0; i < CollisionInfo.Value.points.Count; i++)
                        {
                            Vector2 point = CollisionInfo.Value.points[i];

                            float distance = Vector2.Distance(Vector2.zero, point);
                            float pointMultipler = ( 1 - (distance / CollisionInfo.Value.light.size) ) * 2;

                            pointMultipler = pointMultipler > 1 ? 1 : pointMultipler;
                            pointMultipler = pointMultipler < 0 ? 0 : pointMultipler;
    
                            multiplier += pointMultipler;
                        }

                        visability *= ( multiplier / CollisionInfo.Value.points.Count );
                    }
                }
            }
        
            CollisionInfo = null;
        }
    }
}