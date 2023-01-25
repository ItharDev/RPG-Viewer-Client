using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightingSettings;
using FunkyCode.LightSettings;

namespace FunkyCode.Rendering.Day
{
    public class Pass
    {
        public Sorting.SortList sortList = new Sorting.SortList();
        public Sorting.SortObject sortObject;
        public int layerId;
        public LightmapLayer layer;

        public Camera camera;
        public Vector2 offset;

        public List<DayLightCollider2D> colliderList;
        public int colliderCount;

        public bool drawShadows = false;
        public bool drawMask = false;

        public List<DayLightTilemapCollider2D> tilemapColliderList;
        public int tilemapColliderCount;

        public void SortObjects()
        {
            sortList.Reset();

            var colliderList = DayLightCollider2D.List;

            for(int id = 0; id < colliderList.Count; id++)
            {
                var collider = colliderList[id];
                if (collider.shadowLayer != layerId && collider.maskLayer != layerId)
                    continue;

                switch(layer.sorting)
                {
                    case LayerSorting.ZAxisLower:
                        sortList.Add(collider, -collider.transform.position.z);
                        break;

                    case LayerSorting.ZAxisHigher:
                        sortList.Add(collider, collider.transform.position.z);
                        break;
                }

                switch(layer.sorting)
                {
                    case LayerSorting.YAxisLower:
                        sortList.Add(collider, -collider.transform.position.y);
                        break;

                    case LayerSorting.YAxisHigher:
                        sortList.Add(collider, collider.transform.position.y);
                        break;
                }
            }

            var tilemapColliderList = DayLightTilemapCollider2D.List;
            for(int id = 0; id < tilemapColliderList.Count; id++)
            {
                var tilemap = tilemapColliderList[id];

                if (tilemap.shadowLayer != layerId && tilemap.maskLayer != layerId)
                    continue;

                switch(layer.sorting)
                {
                    case LayerSorting.ZAxisLower:
                        sortList.Add(tilemap, -tilemap.transform.position.z);
                        break;

                    case LayerSorting.ZAxisHigher:
                        sortList.Add(tilemap, tilemap.transform.position.z);
                        break;
                }

                switch(layer.sorting)
                {
                    case LayerSorting.YAxisLower:
                        sortList.Add((object)tilemap, -tilemap.transform.position.y);
                        break;

                    case LayerSorting.YAxisHigher:
                        sortList.Add(tilemap, tilemap.transform.position.y);
                        break;
                }
            }

            sortList.Sort();
        }

        public bool Setup(LightmapLayer slayer, Camera camera)
        {
            if (slayer.id < 0)
            {
                return false;
            }

            layerId = (int)slayer.id;
            layer = slayer;

            this.camera = camera;
            offset = -camera.transform.position;

            colliderList = DayLightCollider2D.List;
            colliderCount = colliderList.Count;

            tilemapColliderList  = DayLightTilemapCollider2D.List;
            tilemapColliderCount = tilemapColliderList.Count;

            drawShadows = slayer.type != LayerType.MaskOnly;
            drawMask = slayer.type != LayerType.ShadowsOnly;
            
            return true;
        }
    }
}