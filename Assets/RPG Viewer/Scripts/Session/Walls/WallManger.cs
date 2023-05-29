using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class WallManger : MonoBehaviour
    {
        [SerializeField] private Wall wallPrefab;
        [SerializeField] private Transform wallParent;

        private Dictionary<string, Wall> walls = new Dictionary<string, Wall>();

        private void OnEnable()
        {
            // Add event listeners
            Events.OnStateChanged.AddListener(ReloadWalls);
            Events.OnSceneLoaded.AddListener(LoadWalls);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnStateChanged.RemoveListener(ReloadWalls);
            Events.OnSceneLoaded.RemoveListener(LoadWalls);
        }

        private void ReloadWalls(SessionState oldState, SessionState newState)
        {

        }
        private void LoadWalls(SceneData settings)
        {
            // Instantiate walls
            List<WallData> list = settings.walls;
            for (int i = 0; i < list.Count; i++)
            {
                CreateWall(list[i]);
            }
        }

        private void CreateWall(WallData data)
        {
            // Instantiate wall and load its data
            Wall wall = Instantiate(wallPrefab, wallParent);
            wall.LoadData(data);

            // Add wall to dictionary
            walls.Add(data.id, wall);
        }
    }
}
