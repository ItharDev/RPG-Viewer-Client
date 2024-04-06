using System.Collections.Generic;
using System.Linq;
using Networking;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class TokenMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;

        [Space]
        [SerializeField] private LayerMask blockingLayers;

        [Space]
        [SerializeField] private Transform rotateParent;

        public float MovementSpeed
        {
            // Transform feet per second to units per second
            get { return Session.Instance.Settings.grid.cellSize * moveSpeed; }
        }

        private Token token;
        private Token dragObject;
        private bool dragging;
        private List<Vector2> waypoints = new List<Vector2>();
        private List<Vector2> dragPoints = new List<Vector2>();
        private int currentWaypoint = 0;
        private float angleToAdd;
        private float angleTimer;

        private void OnEnable()
        {
            // Get reference of main class
            if (token == null) token = GetComponent<Token>();
        }

        private void Update()
        {
            if (dragging)
            {
                if (Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftControl))
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos = new Vector3(mousePos.x, mousePos.y, 0);

                    dragPoints.Add(mousePos);
                }

                if (Input.GetKeyDown(KeyCode.LeftAlt)) MeasurementManager.Instance.ChangeType(MeasurementType.Precise);
                if (Input.GetKeyUp(KeyCode.LeftAlt)) MeasurementManager.Instance.ChangeType(MeasurementType.Grid);

                if (Input.GetKeyUp(KeyCode.LeftControl))
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos = new Vector3(mousePos.x, mousePos.y, 0);
                    dragPoints.Clear();
                    dragPoints.Add(transform.position);
                    MeasurementManager.Instance.StartMeasurement(transform.position, Input.GetKey(KeyCode.LeftAlt) ? MeasurementType.Precise : MeasurementType.Grid);
                }
            }

            // Return if token is not selected or we are edting any fields
            if (!token.Selected || token.UI.Editing) return;

            HandleRotation();
            if (!token.Data.locked) HandleMovement();
        }
        private void FixedUpdate()
        {
            // Move towards next waypoint
            if (waypoints.Count > 0) UpdatePosition();
        }

        private void HandleRotation()
        {
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) return;

            // Define rotation direction A = left, D = right
            if (Input.GetKey(KeyCode.A)) angleToAdd += 100.0f * Time.deltaTime;
            if (Input.GetKey(KeyCode.D)) angleToAdd -= 100.0f * Time.deltaTime;

            angleTimer += Time.deltaTime;

            if (angleTimer >= 1.0f / 30.0f)
            {
                angleTimer = 0.0f;
                // Proceed to rotate if this is not a mount
                if (token.Data.type != TokenType.Mount)
                {
                    FinishRotation(angleToAdd);
                    angleToAdd = 0.0f;
                    return;
                }

                // Check nearby tokens
                List<Token> nearby = token.GetNearbyTokens();

                bool rotationUpdated = false;
                for (int i = 0; i < nearby.Count; i++)
                {
                    // Before updating rotation
                    if (!rotationUpdated)
                    {
                        // Initialise rotation
                        nearby[i].transform.parent = rotateParent;

                        // Update tokens rotation in last iteration
                        if (i == nearby.Count - 1)
                        {
                            FinishRotation(angleToAdd);
                            rotationUpdated = true;

                            // Start the loop over
                            i = 0;
                        }

                        // Prevent updating nearby tokens position before updation our own rotation
                        return;
                    }

                    // Update nearby tokens position, after our own rotation has been updated
                    nearby[i].transform.parent = transform.parent;
                    nearby[i].Movement.MountedRotation();

                }

                angleToAdd = 0.0f;
            }
        }
        public void FinishRotation(float angle, bool addAngle = true)
        {
            float targetAngle = addAngle ? token.Data.rotation + angle : angle;
            token.Data.rotation = targetAngle;
            token.UI.PreviewRotation(targetAngle);
            SocketManager.EmitAsync("rotate-token", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                token.Data.rotation = token.UI.OriginalRotation;
                token.UI.SetRotation(token.UI.OriginalRotation);
                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Data.id, targetAngle, GameData.User.id);
        }
        public void MountedRotation()
        {
            // Return if rotation would collide with walls
            List<Vector2> points = new List<Vector2>() { token.Data.position, transform.position };
            if (CheckCollisions(points))
            {
                transform.position = token.Data.position;
                return;
            }

            FinishMovement(points);
        }

        private void HandleMovement()
        {
            // Check if any of the arrow keys are pressed down
            if (!CheckManualMovement()) return;

            // Apply new target for the camera to follow
            FindObjectOfType<Camera2D>().FollowTarget(transform);

            // Get movement direction
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");

            // Get current and target cells
            Cell currentCell = Session.Instance.Grid.WorldPosToCell(transform.position);
            Cell targetCell = Session.Instance.Grid.GetCell(currentCell.gridPosition.x + Mathf.RoundToInt(inputX), currentCell.gridPosition.y + Mathf.RoundToInt(inputY));

            // Return if no target cell was found
            if (targetCell.worldPosition == Vector2.zero) return;

            // Return if movement would collide with walls
            List<Vector2> movePoints = new List<Vector2>() { currentCell.worldPosition, targetCell.worldPosition };
            if (CheckCollisions(movePoints)) return;

            // Proceed to movement if this is not a mount
            if (token.Data.type != TokenType.Mount)
            {
                FinishMovement(movePoints);
                return;
            }

            // Check tokens nearby
            List<Token> nearby = token.GetNearbyTokens();

            for (int i = 0; i < nearby.Count; i++)
            {
                // Generate new list from points and calculate offset
                List<Vector2> points = new List<Vector2>(movePoints);
                Vector2 offset = nearby[i].transform.position - transform.position;

                // Add offset for each point
                for (int j = 0; j < points.Count; j++)
                {
                    points[j] += offset;
                }

                // Move mounted token if the path is valid 
                if (!CheckCollisions(points)) nearby[i].Movement.FinishMovement(points);
            }

            // Proceed to movement
            FinishMovement(movePoints);
        }
        private bool CheckManualMovement()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) return true;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) return true;

            return false;
        }

        public void OnBeginDrag(BaseEventData eventData)
        {
            // Clear old drag points
            dragPoints.Clear();

            // Return if this token is locked
            if (token.Data.locked) return;

            // Return if we aren't dragging whit LMB
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            // Create clone of this token
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragObject = Instantiate(token, mousePos, Quaternion.identity);
            dragObject.transform.SetParent(transform.parent);

            // Disable clone's vision and light
            dragObject.Vision.ToggleVision(false);
            dragObject.Vision.ToggleLight(false);
            dragObject.Vision.EnableHighlight(false);
            dragObject.DisableCollider();
            dragObject.UI.SetAlpha(0.5f);
            dragObject.UI.DisableOutline();
            dragObject.UI.EnableRaycasting(false);

            // Toggle conditions panel
            if (token.Conditions.IsOpen)
            {
                token.Conditions.ToggleConditions();
                dragObject.Conditions.ToggleConditions(true);
            }

            bool startMeasurement = !Input.GetKey(KeyCode.LeftControl);
            if (startMeasurement)
            {
                MeasurementManager.Instance.StartMeasurement(mousePos, Input.GetKey(KeyCode.LeftAlt) ? MeasurementType.Precise : MeasurementType.Grid);
                dragPoints.Add(transform.position);
            }
            dragging = true;
        }
        public void OnDrag(BaseEventData eventData)
        {
            // Return if drag object is null
            if (dragObject == null) return;

            // Update drag object's position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 0);
            if (!Input.GetKey(KeyCode.LeftAlt)) mousePos = Session.Instance.Grid.SnapToGrid(mousePos, token.Data.dimensions);
            dragObject.transform.position = mousePos;

            if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt)) PathRequestManager.RequestPath(token.Data.position, dragObject.transform.position, UpdatePath);
        }

        private void UpdatePath(Vector2[] waypoints, bool pathFound)
        {
            if (!pathFound) return;

            dragPoints = waypoints.ToList();
            MeasurementManager.Instance.MeasureDistance(dragPoints);
        }

        public void OnEndDrag(BaseEventData eventData)
        {
            dragging = false;
            // Return if drag object is null
            if (dragObject == null) return;

            // Store drag object's position 
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = new Vector3(pos.x, pos.y, 0);

            // Snap to grid if Alt key was not held down
            if (!Input.GetKey(KeyCode.LeftAlt)) pos = Session.Instance.Grid.SnapToGrid(pos, token.Data.dimensions);

            if (!Input.GetKey(KeyCode.LeftControl)) dragPoints.Add(pos);
            Destroy(dragObject.gameObject);

            Session.Instance.TokenManager.HandleMultiMovement(token, dragPoints);
        }
        public void EndMovement(List<Vector2> _dragPoints)
        {
            // Return if movement would collide with walls
            if (CheckCollisions(_dragPoints)) return;

            // Proceed to movement if this is not a mount
            if (token.Data.type != TokenType.Mount)
            {
                FinishMovement(_dragPoints);
                return;
            }

            // Check tokens nearby
            List<Token> nearby = token.GetNearbyTokens();

            for (int i = 0; i < nearby.Count; i++)
            {
                // Generate new list from points and calculate offset
                List<Vector2> points = new List<Vector2>(_dragPoints);
                Vector2 offset = nearby[i].transform.position - transform.position;

                // Add offset for each point
                for (int j = 0; j < points.Count; j++)
                {
                    points[j] += offset;
                }

                // Move mounted token if the path is valid 
                if (!CheckCollisions(points)) nearby[i].Movement.FinishMovement(points);
            }

            // Proceed to movement
            FinishMovement(_dragPoints);
        }

        private void UpdatePosition()
        {
            // Check if we are close enough to teh next waypoint
            if (currentWaypoint >= waypoints.Count) currentWaypoint = 1;

            if (Vector2.Distance(waypoints[currentWaypoint], transform.position) < 0.01f)
            {
                // Move to next waypoint
                currentWaypoint++;

                // Move back to starting point when all points have been gone through
                if (currentWaypoint >= waypoints.Count)
                {
                    currentWaypoint = 1;
                    waypoints.Clear();
                }
            }

            // Check if we have any new waypoints left
            if (waypoints.Count == 0) return;

            // Move towards current waypoint
            transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypoint], Time.fixedDeltaTime * MovementSpeed);
        }
        private bool CheckCollisions(List<Vector2> points)
        {
            // Loop through each waypoint
            for (int i = 0; i < points.Count - 1; i++)
            {
                // Calculate next waypoint's distance and direction
                float distance = Vector2.Distance(points[i], points[i + 1]);
                Vector2 direction = (points[i + 1] - points[i]).normalized;

                // Continue if there's nothing blocking our movement
                if (Physics2D.Raycast(points[i], direction, distance, blockingLayers).collider == null) continue;

                // We have collided with something
                // Allow movement if we are the master client
                if (ConnectionManager.Info.isMaster)
                {
                    MessageManager.QueueMessage("This move would have collided with at least one wall");
                    return false;
                }

                MessageManager.QueueMessage("Movement blocked by a wall");
                return true;
            }

            // Detected no collisions
            return false;
        }

        public void FinishMovement(List<Vector2> points)
        {
            // Generate data
            MovementData movement = new MovementData(token.Data.id, points);

            SocketManager.EmitAsync("move-token", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(movement));
        }
        public void AddWaypoints(MovementData data)
        {
            // Update data on parent class
            token.Data.position = data.points[data.points.Count - 1];

            // Teleport to new location if we are deactivated
            if (!gameObject.activeInHierarchy)
            {
                transform.position = new Vector3(token.Data.position.x, token.Data.position.y, 0);
                return;
            }

            // Add waypoints
            waypoints = data.points;
        }
        public void LockToken()
        {
            SocketManager.EmitAsync("lock-token", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Id, !token.Data.locked);
        }
    }
}