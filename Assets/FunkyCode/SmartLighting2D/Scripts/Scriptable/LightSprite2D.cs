using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode.Scriptable
{
    [System.Serializable]
    public class LightSprite2D
    {
        public static List<LightSprite2D> List = new List<LightSprite2D>();

        public SpriteMeshObject spriteMeshObject = new SpriteMeshObject();

        public LightSpriteShape lightSpriteShape = new LightSpriteShape();

        [SerializeField] private Vector2 position = Vector2.zero;
        [SerializeField] private Vector2 scale = Vector2.one;
        [SerializeField] private int lightLayer;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private float rotation;

        public int LightLayer
        {
            set => lightLayer = value;
            get => lightLayer;
        }

        public Sprite Sprite
        {
            set => sprite = value;
            get => sprite;
        }

        public Vector2 Position
        {
            set => position = value;
            get => position;
        }

        public Vector2 Scale
        {
            set => scale = value;
            get => scale;
        }

        public Color Color
        {
            set => color = value;
            get => color;
        }

        public float Rotation
        {
            set => rotation = value;
            get => rotation;
        }

        public bool InCamera(Camera camera)
        {
            Rect cameraRect = CameraTransform.GetWorldRect(camera);

            lightSpriteShape.Update(this);

            if (cameraRect.Overlaps(lightSpriteShape.GetWorldRect()))
            {
                return true;
            }

            return false;
        }

	    public LightSprite2D()
        {
            List.Add(this);
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                if (!List.Contains(this))
                {
                     List.Add(this);
                }
            }
            else
            {
                List.Remove(this);
            }
        }
    }
        
    [System.Serializable]
    public class LightSpriteTransform {
        public Vector2 scale = new Vector2(1, 1);
        public float rotation = 0;
        public Vector2 position = new Vector2(0, 0);	
    }

    [System.Serializable]
    public class LightSpriteShape
    {
        public bool update = false;

        private Sprite sprite;

        private Vector2 position = Vector2.zero;
        private float rotation = 0;
        private Vector2 scale = Vector2.one;

        public void Update(LightSprite2D light)
        {
            Vector2 position2D = light.Position;
            float rotation2D = light.Rotation;
            Vector2 scale2D = light.Scale;

            sprite = light.Sprite;

            if (position != position2D)
            {
                position = position2D;

                update = true;
            }

            if (rotation != rotation2D)
            {
                rotation = rotation2D;

                update = true;
            }

            if (scale != scale2D)
            {
                scale = scale2D;

                update = true;
            }

            if (update)
            {
                worldPolygon = null;

                update = false;
            }
        }

        public Rect GetWorldRect()
        {
            GetSpriteWorldPolygon();
            return(worldrect);
        }

        private Polygon2 worldPolygon = null;
        private Rect worldrect = new Rect();

        public Polygon2 GetSpriteWorldPolygon()
        {
            if (worldPolygon != null)
            {
                return(worldPolygon);
            }

            Vector2 position = this.position;
            Vector2 scale = this.scale;
            float rotation = this.rotation;

            VirtualSpriteRenderer virtualSprite = new VirtualSpriteRenderer();
            virtualSprite.sprite = sprite;

            SpriteTransform spriteTransform = new SpriteTransform(virtualSprite, position, scale, rotation);

            float rot = spriteTransform.rotation;
            Vector2 size = spriteTransform.scale;
            Vector2 pos = spriteTransform.position;

            rot = rot * Mathf.Deg2Rad + Mathf.PI;

            float rectAngle = Mathf.Atan2(size.y, size.x);
            float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

            Vector2 v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
            Vector2 v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
            Vector2 v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
            Vector2 v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
        
            worldPolygon = GetPolygon();
            
            worldPolygon.points[0].x = v1.x;
            worldPolygon.points[0].y = v1.y;

            worldPolygon.points[1].x = v2.x;
            worldPolygon.points[1].y = v2.y;

            worldPolygon.points[2].x = v3.x;
            worldPolygon.points[2].y = v3.y;

            worldPolygon.points[3].x = v4.x;
            worldPolygon.points[3].y = v4.y;

            worldrect = worldPolygon.GetRect();

            return(worldPolygon);
        }

        private Polygon2 polygon = null;
        private Polygon2 GetPolygon()
        {
            if (polygon == null)
            {
                polygon = new Polygon2(4);
                polygon.points[0] = Vector2.zero;
                polygon.points[1] = Vector2.zero;
                polygon.points[2] = Vector2.zero;
                polygon.points[3] = Vector2.zero;
            }

            return(polygon);
        }
    }
}