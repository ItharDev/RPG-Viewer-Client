using UnityEngine;
using FunkyCode.LightingSettings;
using FunkyCode.LightSettings;

namespace FunkyCode.Rendering.Lightmap
{
    public class Pass
    {    
        public Sorting.SortList sortList = new Sorting.SortList();
        public Sorting.SortObject sortObject;
        public int layerId;
        public LightmapLayer layer;

        public Camera camera;
        public Vector2 offset;

        public void SortObjects()
        {
            sortList.Reset();

            var lightList = Light2D.List;

            for(int id = 0; id < lightList.Count; id++)
            {
                var light = lightList[id];
                if (light.lightLayer != layerId)
                    continue;

                switch(layer.sorting)
                {
                    case LayerSorting.ZAxisLower:
                        sortList.Add(light, -light.transform.position.z);
                        break;

                    case LayerSorting.ZAxisHigher:
                        sortList.Add(light, light.transform.position.z);
                        break;
                }
            }

            var roomList = LightRoom2D.List;
            for(int id = 0; id < roomList.Count; id++)
            {
                var room = roomList[id];
                if (room.lightLayer != layerId)
                    continue;

                switch(layer.sorting)
                {
                    case LayerSorting.ZAxisLower:
                        sortList.Add(room, -room.transform.position.z);
                        break;

                    case LayerSorting.ZAxisHigher:
                        sortList.Add(room, room.transform.position.z);
                        break;
                }
            }

            var roomTilemapList = LightTilemapRoom2D.List;
            for(int id = 0; id < roomTilemapList.Count; id++)
            {
                var tilemapRoom = roomTilemapList[id];
                if (tilemapRoom.lightLayer != layerId)
                    continue;

                switch(layer.sorting)
                {
                    case LayerSorting.ZAxisLower:
                        sortList.Add(tilemapRoom, -tilemapRoom.transform.position.z);
                        break;

                    case LayerSorting.ZAxisHigher:
                        sortList.Add(tilemapRoom, tilemapRoom.transform.position.z);
                        break;
                }
            }

            var spriteList = LightSprite2D.List;
            for(int id = 0; id < spriteList.Count; id++)
            {
                var lightSprite = spriteList[id];
                if (lightSprite.lightLayer != layerId)
                    continue;

                switch(layer.sorting)
                {
                    case LayerSorting.ZAxisLower:
                        sortList.Add(lightSprite, - lightSprite.transform.position.z);
                        break;

                    case LayerSorting.ZAxisHigher:
                        sortList.Add(lightSprite, lightSprite.transform.position.z);
                        break;
                }
            }

            sortList.Sort();
        }

        public bool Setup(LightmapLayer slayer, Camera camera)
        {
            if (slayer.id < 0)
                return false;

            layerId = (int)slayer.id;
            layer = slayer;

            this.camera = camera;
            offset = -camera.transform.position;

            return true;
        }
    }
}