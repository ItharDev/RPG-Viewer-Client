using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node : IHeapItem<Node>
    {
        public Vector2 worldPosition;
        public int gridX;
        public int gridY;

        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }

        public int HeapIndex { get { return heapIndex; } set { heapIndex = value; } }
        private int heapIndex;

        public Node parent;
        public List<Node> neighbours;

        public Node(Vector2 _worldPosition, int _gridX, int _gridY)
        {
            worldPosition = _worldPosition;
            gridX = _gridX;
            gridY = _gridY;

            neighbours = new List<Node>();
        }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0) compare = hCost.CompareTo(nodeToCompare.hCost);

            return -compare;
        }
    }
}