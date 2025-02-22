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
            Events.OnWallCreated.AddListener(CreateWall);
            Events.OnWallModified.AddListener(ModifyWall);
            Events.OnWallRemoved.AddListener(RemoveWall);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnStateChanged.RemoveListener(ReloadWalls);
            Events.OnSceneLoaded.RemoveListener(LoadWalls);
            Events.OnWallCreated.RemoveListener(CreateWall);
            Events.OnWallModified.RemoveListener(ModifyWall);
            Events.OnWallRemoved.RemoveListener(RemoveWall);
        }

        private void ReloadWalls(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadWalls();
            }
            else
            {
                // Unload tokens if syncing was disabled
                if (!newState.synced)
                {
                    UnloadWalls();
                    return;
                }

                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadWalls();
            }

            Events.ReloadPathfinder?.Invoke();
        }
        private void UnloadWalls()
        {
            // Clear lists
            walls.Clear();
            foreach (Transform child in wallParent) Destroy(child.gameObject);
        }
        private void LoadWalls(SceneData settings)
        {
            // Instantiate walls
            List<WallData> list = settings.walls;
            for (int i = 0; i < list.Count; i++)
            {
                CreateWall(list[i], false);
            }

            GetComponent<WallTools>().LoadWalls(list);
            Events.ReloadPathfinder?.Invoke();
        }

        private void CreateWall(WallData data, bool reloadPathfinder)
        {
            // Instantiate wall and load its data
            Wall wall = Instantiate(wallPrefab, wallParent);
            wall.LoadData(data);

            // Add wall to dictionary
            walls.Add(data.id, wall);

            if (reloadPathfinder) Events.ReloadPathfinder?.Invoke();
        }
        private void ModifyWall(string id, WallData data)
        {
            if (!walls.ContainsKey(id)) return;
            walls[id].LoadData(data);

            Events.ReloadPathfinder?.Invoke();
        }
        private void RemoveWall(string id)
        {
            if (!walls.ContainsKey(id)) return;
            Destroy(walls[id].gameObject);
            walls.Remove(id);

            Events.ReloadPathfinder?.Invoke();
        }
    }
}
