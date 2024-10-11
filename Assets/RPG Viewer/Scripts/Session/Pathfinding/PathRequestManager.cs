using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [RequireComponent(typeof(Pathfinding))]
    public class PathRequestManager : MonoBehaviour
    {
        private Queue<PathRequest> requestQueue = new Queue<PathRequest>();
        private PathRequest currentPathRequest;

        private static PathRequestManager instance;
        private Pathfinding pathfinding;

        bool processingPath;

        private void Awake()
        {
            instance = this;
            pathfinding = GetComponent<Pathfinding>();
        }

        public static void RequestPath(Vector2 startPos, Vector2 targetPos, Action<Vector2[], bool> callback)
        {
            PathRequest request = new PathRequest(startPos, targetPos, callback);
            instance.requestQueue.Enqueue(request);
            instance.TryProcessNext();
        }

        private void TryProcessNext()
        {
            if (!processingPath && requestQueue.Count > 0)
            {
                currentPathRequest = requestQueue.Dequeue();
                processingPath = true;
                pathfinding.StartFindPath(currentPathRequest.startPos, currentPathRequest.targetPos);
            }
        }

        public void FinishedProcessingPath(Vector2[] path, bool success)
        {
            currentPathRequest.callback(path, success);
            processingPath = false;
            TryProcessNext();
        }
    }

    public struct PathRequest
    {
        public Vector2 startPos;
        public Vector2 targetPos;
        public Action<Vector2[], bool> callback;

        public PathRequest(Vector2 _startPos, Vector2 _targetPos, Action<Vector2[], bool> _callback)
        {
            startPos = _startPos;
            targetPos = _targetPos;
            callback = _callback;
        }
    }
}