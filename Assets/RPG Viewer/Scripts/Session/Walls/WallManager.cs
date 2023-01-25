using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG
{
    public class WallManager : MonoBehaviour
    {
        [SerializeField] private Wall wallPrefab;

        private List<Wall> walls = new List<Wall>();

        public void GenerateWalls(List<WallData> data)
        {
            RemoveWalls();
            foreach (var wall in data)
            {
                Wall w = Instantiate(wallPrefab, transform);
                w.GenerateWall(wall);
                walls.Add(w);
            }
        }
        public void UnloadWalls()
        {
            RemoveWalls();
        }
        public void ToggleWall(string id, bool state)
        {
            Wall wall = walls.FirstOrDefault(x => x.Data.wallId == id);
            if (wall != null)
            {
                wall.SetState(state);
            }
        }
        public void ModifyWall(WallData data)
        {
            Wall wall = walls.FirstOrDefault(x => x.Data.wallId == data.wallId);
            if (wall != null)
            {
                wall.ModifyDoor(data);
            }
        }

        public List<WallData> GetWalls()
        {
            List<WallData> data = new List<WallData>();

            foreach (var wall in walls) data.Add(wall.Data);
            return data;
        }

        private void RemoveWalls()
        {
            for (int i = walls.Count - 1; i >= 0; i--)
            {
                if (walls[i] != null) Destroy(walls[i].gameObject);
            }
            walls.Clear();
        }
    }
}