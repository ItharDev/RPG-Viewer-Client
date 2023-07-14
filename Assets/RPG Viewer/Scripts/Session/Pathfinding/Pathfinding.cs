using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [RequireComponent(typeof(Grid))]
    public class Pathfinding : MonoBehaviour
    {
        [SerializeField] private LayerMask unwalkableMask;

        private PathRequestManager requestManager;
        private Grid grid;

        private void Awake()
        {
            if (requestManager == null) requestManager = GetComponent<PathRequestManager>();
            if (grid == null) grid = GetComponent<Grid>();
        }

        public void StartFindPath(Vector2 startPos, Vector2 targetPos)
        {
            StartCoroutine(FindPath(startPos, targetPos));
        }
        private IEnumerator FindPath(Vector2 startPos, Vector2 targetPos)
        {
            Vector2[] waypoints = new Vector2[0];
            bool pathFound = false;

            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);

            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathFound = true;
                    break;
                }

                foreach (var neighbour in currentNode.neighbours)
                {
                    if (closedSet.Contains(neighbour)) continue;

                    int costToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (costToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = costToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                    }
                }
            }

            yield return null;
            if (pathFound) waypoints = RetracePath(startNode, targetNode);
            requestManager.FinishedProcessingPath(waypoints, pathFound);
        }

        private Vector2[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            if (startNode == endNode) return new Vector2[2] { startNode.worldPosition, endNode.worldPosition };

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Add(currentNode);
            path.Reverse();
            Vector2[] waypoints = SimplifyPath(path);
            grid.Path = waypoints;
            return waypoints;
        }

        private Vector2[] SimplifyPath(List<Node> path)
        {
            List<Vector2> waypoints = new List<Vector2>();

            waypoints.Add(path[0].worldPosition);
            Node currentNode = path[0];

            for (int i = 1; i < path.Count; i++)
            {
                if (!CheckCollision(currentNode, path[i])) continue;

                waypoints.Add(path[i - 1].worldPosition);
                currentNode = path[i - 1];
            }

            waypoints.Add(path[path.Count - 1].worldPosition);
            return waypoints.ToArray();
        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
        private bool CheckCollision(Node nodeA, Node nodeB)
        {
            float distance = Vector2.Distance(nodeA.worldPosition, nodeB.worldPosition);
            Vector2 direction = (nodeB.worldPosition - nodeA.worldPosition).normalized;

            Collider2D collision = Physics2D.Raycast(nodeA.worldPosition, direction, distance, unwalkableMask).collider;
            if (collision == null) return false;
            return collision.enabled;
        }
    }
}