using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using FunkyCode;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class Token : MonoBehaviour
    {
        [Header("Data")]
        public TokenData Data;
        [SerializeField] private float moveSpeed;

        [Header("UI")]
        public Image image;
        public Image Selection;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private GameObject rotateButton;

        [Header("Locked")]
        [SerializeField] private Image lockedImage;
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockedSprite;

        [Header("Config")]
        [SerializeField] private TokenConfiguration configPanel;

        [Header("Elevation")]
        [SerializeField] private TMP_InputField elevationInput;
        [SerializeField] private GameObject elevationPanel;

        [Header("Health")]
        [SerializeField] private TMP_InputField healthInput;
        [SerializeField] private GameObject healthPanel;

        [Header("Vision")]
        [SerializeField] private Light2D nightSource;
        [SerializeField] private Light2D visionSource;

        [Header("Light")]
        [SerializeField] private Light2D lightSource;
        [SerializeField] private LightHandler lightHandler;


        [Header("Movement")]
        [SerializeField] private LayerMask blockingLayers;
        [SerializeField] private LayerMask mountLayers;

        [Header("Conditions")]
        [SerializeField] private List<ConditionHolder> conditionHolders = new List<ConditionHolder>();
        [SerializeField] private ConditionHolder deadCondition;
        public ConditionFlag conditionFlags;

        private Token dragObject;
        private TokenConfiguration config;
        private List<Token> mountedTokens = new List<Token>();

        private Vector3 screenPos;
        private SessionGrid grid;
        private StateManager state;
        private float angleoffset;
        private float initialRotation;
        private List<Token> rotatedTokens = new List<Token>();
        private Vector2 startPos;
        private bool editInput;
        private List<Vector2> waypoints = new List<Vector2>();
        public Permission Permission;
        public bool Selected;

        private List<Vector2> movePoints = new List<Vector2>();
        private int currentWaypoint = 0;
        private MeasurementType currentType;

        public float MovementSpeed
        {
            get
            {
                return (grid.CellSize * 0.2f) * moveSpeed;
            }
        }


        private void OnValidate()
        {
            for (int i = 0; i < conditionHolders.Count; i++)
            {
                conditionHolders[i].transform.SetSiblingIndex(i);
            }

            UpdateConditions((int)conditionFlags);
        }
        private void Update()
        {
            uiCanvas.sortingLayerName = canvas.sortingLayerName;
            if (startPos != Vector2.zero) transform.eulerAngles = new Vector3(0, 0, 0);


            if (config != null)
            {
                if (!config.gameObject.activeInHierarchy) Destroy(config.gameObject);
            }
            if (state == null) state = FindObjectOfType<StateManager>(true);

            if (image.raycastTarget && state.ToolState != ToolState.Pan) image.raycastTarget = false;
            else if (!image.raycastTarget && state.ToolState == ToolState.Pan) image.raycastTarget = true;

            if (Selection.gameObject.activeInHierarchy)
            {
                if ((Input.GetKeyUp(KeyCode.Backspace) || Input.GetKeyUp(KeyCode.Delete)))
                {
                    DeleteToken();
                }

                if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
                {
                    SessionManager.Session.CopyToken(this.Data);
                }

                if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown))
                {
                    var elevation = elevationInput.text.TrimEnd('f', 't', ' ');
                    int elevationValue = elevation == "" ? 0 : int.Parse(elevation);
                    elevationValue += Input.GetKey(KeyCode.PageUp) ? 5 : -5;
                    string newElevation = $"{elevationValue} ft";
                    elevationInput.text = newElevation;

                    UpdateElevation();
                }
            }
            if (dragObject != null)
            {
                if (currentType == MeasurementType.Grid && Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    FindObjectOfType<MeasurementManager>().ChangeType(MeasurementType.Precise);
                    currentType = MeasurementType.Precise;
                }
                if (currentType == MeasurementType.Precise && Input.GetKeyUp(KeyCode.LeftAlt))
                {
                    FindObjectOfType<MeasurementManager>().ChangeType(MeasurementType.Grid);
                    currentType = MeasurementType.Grid;
                }
                if (Input.GetMouseButtonDown(1))
                {
                    var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    waypoints.Add(currentType == MeasurementType.Grid ? grid.SnapToGrid(point, Data.dimensions) : point);
                }
            }

            if (Input.GetMouseButtonDown(0) && !RectTransformUtility.RectangleContainsScreenPoint(Selection.GetComponent<RectTransform>(), Camera.main.ScreenToWorldPoint(Input.mousePosition)) && !RectTransformUtility.RectangleContainsScreenPoint(rotateButton.GetComponent<RectTransform>(), Camera.main.ScreenToWorldPoint(Input.mousePosition)) && Selection.gameObject.activeInHierarchy)
            {
                ToggleSelection();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (!Selection.gameObject.activeInHierarchy || (!Data.enabled && !SessionManager.IsMaster)) return;
                FindObjectOfType<Camera2D>().FollowTarget(transform);
                var inputX = Input.GetAxisRaw("Horizontal");
                var inputY = Input.GetAxisRaw("Vertical");
                var currentCell = grid.PointToCell(transform.position);
                var targetPoint = new Vector2Int(currentCell.cell.x + Mathf.RoundToInt(inputX), currentCell.cell.y + Mathf.RoundToInt(inputY));
                var targetCell = grid.GetCell(targetPoint.y, targetPoint.x);
                if (targetCell.position != Vector2.zero)
                {
                    waypoints.Add(transform.position);
                    waypoints.Add(targetCell.position);

                    if (!CheckMovement(false))
                    {
                        waypoints.Clear();
                        return;
                    }

                    if (Data.type == TokenType.Mount)
                    {
                        var colliders = Physics2D.OverlapBoxAll(transform.localPosition, new Vector2(0.009f * (Data.dimensions.x >= Data.dimensions.y ? canvas.GetComponent<RectTransform>().sizeDelta.x : canvas.GetComponent<RectTransform>().sizeDelta.y), 0.009f * (Data.dimensions.x >= Data.dimensions.y ? canvas.GetComponent<RectTransform>().sizeDelta.x : canvas.GetComponent<RectTransform>().sizeDelta.y)), 360, mountLayers);
                        var tokens = new List<Token>();
                        for (var i = 0; i < colliders.Length; i++)
                        {
                            if (colliders[i].GetComponent<Token>() != this && colliders[i].GetComponent<Token>() != null) tokens.Add(colliders[i].GetComponent<Token>());
                        }

                        for (int i = 0; i < tokens.Count; i++)
                        {
                            var token = tokens[i].GetComponent<Token>();
                            List<Vector2> wp = new List<Vector2>(waypoints);
                            if (token == null) continue;
                            if (token.Data.type == TokenType.Character)
                            {
                                Vector2 offset = token.transform.localPosition - transform.localPosition;
                                for (int j = 0; j < wp.Count; j++)
                                {
                                    wp[j] += offset;
                                }
                                token.MoveToken(wp);
                            }
                        }
                        tokens.Clear();
                    }

                    MoveToken(waypoints);
                    waypoints.Clear();
                }
            }
        }

        private void FixedUpdate()
        {
            if (movePoints.Count > 0) MoveToPosition();
        }

        public void Move(MovementData data)
        {
            Data.position = data.points[data.points.Count - 1];
            if (gameObject.activeInHierarchy)
            {
                currentWaypoint = 1;
                movePoints = data.points;
            }
            else transform.localPosition = new Vector3(Data.position.x, Data.position.y, 0);
        }

        private bool CheckMovement(bool useEndPoint)
        {
            bool collided = false;

            if (useEndPoint) waypoints.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                float dist = Vector2.Distance(waypoints[i], waypoints[i + 1]);
                Vector2 dir = (waypoints[i + 1] - waypoints[i]).normalized;

                if (Physics2D.Raycast(waypoints[i], dir, dist, blockingLayers).collider != null) collided = true;
            }

            if (collided)
            {
                if (SessionManager.IsMaster)
                {
                    MessageManager.QueueMessage("This movement would collide with a wall");
                    return true;
                }
                else
                {
                    MessageManager.QueueMessage("Movement blocked");
                    return false;
                }
            }
            else return true;
        }

        #region Mouse Events
        public void OnBeginDrag(BaseEventData eventData)
        {
            if (FindObjectOfType<StateManager>().ToolState == ToolState.Measure) return;
            if (Data.locked || (Permission.permission != PermissionType.Owner && !SessionManager.IsMaster)) return;
            ClosePanel();

            if (Selection.gameObject.activeInHierarchy) ToggleSelection();
            PointerEventData pointerData = eventData as PointerEventData;

            if (pointerData.button != PointerEventData.InputButton.Left) return;
            dragObject = Instantiate(this, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
            dragObject.transform.SetParent(transform.parent);

            dragObject.lightSource.gameObject.SetActive(false);
            dragObject.nightSource.gameObject.SetActive(false);
            dragObject.visionSource.gameObject.SetActive(false);
            dragObject.GetComponent<BoxCollider2D>().enabled = false;

            dragObject.image.color = new Color(1, 1, 1, 0.5f);
            waypoints.Add(transform.position);

            currentType = FindObjectOfType<MeasurementManager>().StartMeasurement(Camera.main.ScreenToWorldPoint(Input.mousePosition), Input.GetKey(KeyCode.LeftAlt) ? MeasurementType.Precise : MeasurementType.Grid);
        }
        public void OnDrag(BaseEventData eventData)
        {
            if (dragObject != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos = new Vector3(mousePos.x, mousePos.y, 0);
                dragObject.transform.position = mousePos;
            }
        }
        public void OnEndDrag(BaseEventData eventData)
        {
            if (dragObject != null)
            {
                Vector2 pos = dragObject.transform.position;
                if (!Input.GetKey(KeyCode.LeftAlt)) pos = grid.SnapToGrid(dragObject.transform.position, Data.dimensions);
                Destroy(dragObject.gameObject);
                
                waypoints.Add(pos);
                if (!CheckMovement(false))
                {
                    waypoints.Clear();
                    return;
                }

                if (Data.type == TokenType.Mount)
                {
                    var colliders = Physics2D.OverlapBoxAll(transform.localPosition, new Vector2(0.009f * (Data.dimensions.x >= Data.dimensions.y ? canvas.GetComponent<RectTransform>().sizeDelta.x : canvas.GetComponent<RectTransform>().sizeDelta.y), 0.009f * (Data.dimensions.x >= Data.dimensions.y ? canvas.GetComponent<RectTransform>().sizeDelta.x : canvas.GetComponent<RectTransform>().sizeDelta.y)), 360, mountLayers);
                    var tokens = new List<Token>();
                    for (var i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i].GetComponent<Token>() != this && colliders[i].GetComponent<Token>() != null) tokens.Add(colliders[i].GetComponent<Token>());
                    }

                    for (int i = 0; i < tokens.Count; i++)
                    {
                        var token = tokens[i].GetComponent<Token>();
                        List<Vector2> wp = new List<Vector2>(waypoints);
                        if (token == null) continue;
                        if (token.Data.type == TokenType.Character)
                        {
                            Vector2 offset = token.transform.localPosition - transform.localPosition;
                            for (int j = 0; j < wp.Count; j++)
                            {
                                wp[j] += offset;
                            }
                            token.MoveToken(wp);
                        }
                    }
                    tokens.Clear();
                }
                MoveToken(waypoints);
                waypoints.Clear();
            }
        }
        public void OnPointerClick(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (!GetComponent<BoxCollider2D>().enabled) return;

            if (Permission.permission != PermissionType.Owner && !SessionManager.IsMaster) return;
            if (!pointerData.dragging && pointerData.clickCount == 1 && config == null) ToggleSelection();
            if (!pointerData.dragging && pointerData.clickCount == 2 && config == null) OpenConfig();
        }

        public void BeginRotate(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;

            if (Permission.permission != PermissionType.Owner && !SessionManager.IsMaster) return;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            initialRotation = image.transform.eulerAngles.z;
            screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 vec3 = Input.mousePosition - screenPos;
            angleoffset = (Mathf.Atan2(transform.right.y, transform.right.x) - Mathf.Atan2(vec3.y, vec3.x)) * Mathf.Rad2Deg;

            if (Data.type != TokenType.Mount) return;

            var colliders = Physics2D.OverlapBoxAll(transform.localPosition, new Vector2(0.009f * (Data.dimensions.x >= Data.dimensions.y ? canvas.GetComponent<RectTransform>().sizeDelta.x : canvas.GetComponent<RectTransform>().sizeDelta.y), 0.009f * (Data.dimensions.x >= Data.dimensions.y ? canvas.GetComponent<RectTransform>().sizeDelta.x : canvas.GetComponent<RectTransform>().sizeDelta.y)), 360, mountLayers);
            rotatedTokens = new List<Token>();
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Token>() != this && colliders[i].GetComponent<Token>() != null) rotatedTokens.Add(colliders[i].GetComponent<Token>());
            }

            for (int i = 0; i < rotatedTokens.Count; i++)
            {
                var token = rotatedTokens[i].GetComponent<Token>();
                if (token == null) continue;
                if (token.Data.type == TokenType.Character)
                {
                    token.InitialiseRotation();
                    token.transform.parent = image.transform;
                }
            }
        }
        public void Rotate(BaseEventData eventData)
        {
            Vector3 vec3 = Input.mousePosition - screenPos;
            float angle = Mathf.Atan2(vec3.y, vec3.x) * Mathf.Rad2Deg;
            UpdateRotation(angle);
        }
        public async void EndRotate(BaseEventData eventData)
        {
            for (int i = 0; i < rotatedTokens.Count; i++)
            {
                var token = rotatedTokens[i].GetComponent<Token>();
                if (token == null) continue;
                token.transform.parent = transform.parent;
                token.MoveRotation();
            }

            rotatedTokens.Clear();

            await SocketManager.Socket.EmitAsync("rotate-token", (callback) =>
            {
                if (!callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    UpdateRotation(initialRotation);
                }
            }, Data.id, image.transform.eulerAngles.z);
        }
        #endregion

        public async void MoveToken(List<Vector2> points)
        {
            var movement = new MovementData()
            {
                id = Data.id,
                points = points
            };

            await SocketManager.Socket.EmitAsync("move-token", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, JsonUtility.ToJson(movement));
        }

        public void InitialiseRotation()
        {
            startPos = transform.position;
        }
        public void MoveRotation()
        {
            var points = new List<Vector2>();
            points.Add(startPos);
            points.Add(transform.position);
            startPos = Vector2.zero;

            MoveToken(points);
        }

        #region Data
        public void LoadData(TokenData data, Sprite sprite)
        {
            if (grid == null) grid = FindObjectOfType<SessionGrid>();

            Data = data;
            if (SessionManager.IsMaster)
            {
                Permission = new Permission()
                {
                    user = SocketManager.UserId,
                    permission = PermissionType.Owner
                };
            }
            else
            {
                var perm = Data.permissions.FirstOrDefault(x => x.user == SocketManager.UserId);
                if (perm.user != SocketManager.UserId)
                {
                    Permission = new Permission()
                    {
                        user = SocketManager.UserId,
                        permission = PermissionType.None
                    };
                }
                else Permission = perm;
            }
            if (Permission.permission != PermissionType.Owner && !SessionManager.IsMaster)
            {
                canvas.GetComponent<GraphicRaycaster>().enabled = false;
            }
            else canvas.GetComponent<GraphicRaycaster>().enabled = true;
            transform.localPosition = new Vector3(data.position.x, data.position.y, 0);

            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = new Color(1, 1, 1, 1);
            }

            label.transform.parent.gameObject.SetActive(data.name != "");
            label.text = data.name;
            elevationInput.text = data.elevation;
            healthInput.text = data.health.ToString();
            UpdateConditions(data.conditions);
            LoadLights();
            UpdateRotation(data.rotation);
            SetHealth(Data.health);

            EnableToken(data.enabled);

            if (grid.CellSize > 0) Resize(grid.CellSize);

            lockedImage.sprite = Data.locked ? lockedSprite : unlockedSprite;

            GetComponent<BoxCollider2D>().size = canvas.GetComponent<RectTransform>().sizeDelta;

            HandleSorting();
        }
        public void HandleSorting()
        {
            switch (Data.type)
            {
                case TokenType.Character:
                    canvas.sortingLayerName = "Characters";
                    if (Data.highlighted) canvas.sortingLayerName = "Highlighted Characters";
                    if (conditionFlags.HasFlag(deadCondition.condition.flag)) canvas.sortingLayerName = "Dead Characters";
                    if (Permission.permission == PermissionType.Owner) canvas.sortingLayerName = "Owned Characters";
                    break;
                case TokenType.Mount:
                    canvas.sortingLayerName = "Mounts";
                    if (Data.highlighted) canvas.sortingLayerName = "Highlighted Mounts";
                    if (conditionFlags.HasFlag(deadCondition.condition.flag)) canvas.sortingLayerName = "Dead Mounts";
                    if (Permission.permission == PermissionType.Owner) canvas.sortingLayerName = "Owned Mounts";

                    break;
                case TokenType.Item:
                    canvas.sortingLayerName = "Items";
                    if (Data.highlighted) canvas.sortingLayerName = "Highlighted Items";
                    if (Permission.permission == PermissionType.Owner) canvas.sortingLayerName = "Owned Items";

                    break;
            }
        }

        public void Resize(float cellSize)
        {
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * cellSize * (Data.dimensions.x / 5.0f), 100 * cellSize * (Data.dimensions.y / 5.0f));
            if (Data.dimensions.x >= Data.dimensions.y)
            {
                uiCanvas.transform.localScale = new Vector3(cellSize * (Data.dimensions.y / 5.0f), cellSize * (Data.dimensions.y / 5.0f), 1.0f);
            }
            else
            {
                uiCanvas.transform.localScale = new Vector3(cellSize * (Data.dimensions.x / 5.0f), cellSize * (Data.dimensions.x / 5.0f), 1.0f);
            }
        }
        public void LoadLights()
        {
            lightHandler.Init(Data.lightEffect, Data.lightColor, Data.lightIntensity, Data.flickerFrequency, Data.flickerAmount, Data.pulseInterval, Data.pulseAmount);

            visionSource.enabled = Data.enabled && Data.hasVision && Permission.permission != PermissionType.None && Selected;
            lightSource.enabled = Data.enabled && Data.lightRadius > 0;
            nightSource.enabled = Data.enabled && Data.nightVision && Permission.permission != PermissionType.None && Selected;
            nightSource.color.a = SessionManager.Session.Settings.fogOfWar.nightVisionStrength;
            if (grid.CellSize > 0)
            {
                lightSource.size = Data.lightRadius * 0.2f * grid.CellSize + (grid.CellSize * 0.5f);
                nightSource.size = 35.0f * 0.2f * grid.CellSize + (grid.CellSize * 0.5f);
                visionSource.size = grid.CellSize * 40.5f;
            }
        }
        public void EnableToken(bool enabled)
        {
            Data.enabled = enabled;
            image.color = new Color(1, 1, 1, enabled ? 1.0f : 0.5f);
            visionSource.enabled = Data.enabled && Data.hasVision && Permission.permission != PermissionType.None && Selected;
            lightSource.enabled = Data.enabled && Data.lightRadius > 0;
            nightSource.enabled = Data.enabled && Data.nightVision && Permission.permission != PermissionType.None && Selected;
        }
        public void SetElevation(string elevation)
        {
            Data.elevation = elevation;
            elevationInput.text = elevation.ToString();

            if (Data.elevation.ToCharArray().Length >= 1)
            {
                if (Data.elevation.ToCharArray()[0].ToString() == "0") elevationPanel.SetActive(Selection.gameObject.activeInHierarchy);
                else elevationPanel.SetActive(true);
            }
            else elevationPanel.SetActive(Selection.gameObject.activeInHierarchy);
        }
        public void SetLocked(bool locked)
        {
            Data.locked = locked;
            lockedImage.sprite = Data.locked ? lockedSprite : unlockedSprite;
        }
        public void SetHealth(int health)
        {
            healthInput.text = health.ToString();
            Data.health = health;

            if (SessionManager.IsMaster)
            {
                if (Data.health == 0) healthPanel.SetActive(Selection.gameObject.activeInHierarchy);
                else healthPanel.SetActive(true);
            }
            else
            {
                if (Data.type == TokenType.Character && Permission.permission == PermissionType.Owner)
                {
                    if (Data.health == 0) healthPanel.SetActive(Selection.gameObject.activeInHierarchy);
                    else healthPanel.SetActive(true);
                }
                else healthPanel.SetActive(false);
            }
        }
        public void UpdateConditions(int conditions)
        {
            Data.conditions = conditions;
            this.conditionFlags = (ConditionFlag)Data.conditions;

            for (int i = 0; i < conditionHolders.Count; i++)
            {
                var con = conditionHolders[i];
                con.gameObject.SetActive(conditionFlags.HasFlag(con.condition.flag));
            }
            deadCondition.gameObject.SetActive(conditionFlags.HasFlag(deadCondition.condition.flag));

            HandleSorting();
        }
        public void UpdateRotation(float angle)
        {
            image.transform.eulerAngles = new Vector3(0, 0, angle);
            Data.rotation = angle;
        }

        public async void ModifyToken(TokenData _data, byte[] _bytes, bool _imageChanged)
        {
            _data.position = Data.position;
            _data.health = Data.health;
            _data.elevation = Data.elevation;
            _data.locked = Data.locked;
            _data.enabled = Data.enabled;
            _data.rotation = Data.rotation;
            _data.permissions = Data.permissions;

            ClosePanel();

            await SocketManager.Socket.EmitAsync("modify-token", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(_data), _imageChanged ? Convert.ToBase64String(_bytes) : null);
        }

        private void MoveToPosition()
        {
            if (Vector2.Distance(movePoints[currentWaypoint], transform.position) < 0.01f)
            {
                currentWaypoint++;
                if (currentWaypoint >= movePoints.Count)
                {
                    currentWaypoint = 1;
                    movePoints.Clear();
                }
            }
            if (movePoints.Count > 0)
            {
                transform.position = Vector2.MoveTowards(transform.position, movePoints[currentWaypoint], Time.fixedDeltaTime * MovementSpeed);
            }
        }
        #endregion

        #region Buttons
        public void ToggleSelection()
        {
            Selection.gameObject.SetActive(!Selection.gameObject.activeInHierarchy);
            if (SessionManager.IsMaster) panel.SetActive(Selection.gameObject.activeInHierarchy);
            rotateButton.SetActive(Selection.gameObject.activeInHierarchy);
            if (Selection.gameObject.activeInHierarchy) SessionManager.Session.SelectToken(this);
            else if (SessionManager.IsMaster) SessionManager.Session.SelectToken(null);

            SetHealth(Data.health);
            SetElevation(Data.elevation);

            if (Selection.gameObject.activeInHierarchy) canvas.sortingLayerName = "Above Fog";
            else HandleSorting();
        }
        public void OpenConfig()
        {
            if (config == null) config = Instantiate(configPanel, GameObject.Find("Main Canvas").transform);
            else config.gameObject.SetActive(true);

            config.transform.SetAsLastSibling();
            config.LoadData(Data, this, image.sprite.texture.GetRawTextureData());
        }
        public void ClosePanel()
        {
            if (config != null) Destroy(config.gameObject);
        }
        public async void UpdateVisibility()
        {
            await SocketManager.Socket.EmitAsync("update-visibility", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, !Data.enabled);
        }
        public async void UpdateHealth()
        {
            await SocketManager.Socket.EmitAsync("update-health", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    healthInput.text = Data.health.ToString();
                }
            }, Data.id, int.Parse(healthInput.text));
        }
        public async void LockToken()
        {
            await SocketManager.Socket.EmitAsync("lock-token", async (callback) =>
            {
                await UniTask.SwitchToThreadPool();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, !Data.locked);
        }
        public void SelectInput() => editInput = true;
        public void DeselectInput() => editInput = false;
        public async void UpdateElevation()
        {
            if (string.IsNullOrEmpty(elevationInput.text)) elevationInput.text += "0";
            if (!elevationInput.text.Contains("ft")) elevationInput.text += " ft";

            await SocketManager.Socket.EmitAsync("update-elevation", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    elevationInput.text = Data.elevation;
                }
            }, Data.id, elevationInput.text);
        }
        public void DeleteToken()
        {
            if (config != null)
            {
                if (config.gameObject.activeInHierarchy || editInput) return;
            }
            if (elevationInput.isFocused || healthInput.isFocused) return;

            bool askConfirmation = false;
            for (int i = 0; i < Data.permissions.Count; i++)
            {
                if (Data.permissions[i].permission == PermissionType.Owner)
                {
                    askConfirmation = true;
                    break;
                }
            }

            if (askConfirmation)
            {
                confirmPanel.SetActive(true);
                confirmPanel.transform.SetParent(GameObject.Find("Main Canvas").transform);
                confirmPanel.transform.SetAsLastSibling();
                confirmPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                confirmPanel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
            else ConfirmDeletion();
        }

        public async void ConfirmDeletion()
        {
            Destroy(confirmPanel);
            await SocketManager.Socket.EmitAsync("remove-token", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id);
        }
        #endregion
    }

    [Serializable]
    public struct TokenData
    {
        public string id;
        public string name;
        public TokenType type;
        public List<Permission> permissions;
        public Vector2Int dimensions;
        public bool hasVision;
        public bool nightVision;
        public bool highlighted;
        public int lightRadius;
        public LightEffect lightEffect;
        public Color lightColor;
        public float lightIntensity;
        public float flickerFrequency;
        public float flickerAmount;
        public float pulseInterval;
        public float pulseAmount;
        public string image;
        public Vector2 position;
        public bool enabled;
        public int health;
        public string elevation;
        public int conditions;
        public bool locked;
        public float rotation;
    }

    [Serializable]
    public enum TokenType
    {
        Character,
        Mount,
        Item
    }

    [Serializable]
    public struct Permission
    {
        public string user;
        public PermissionType permission;
    }

    [Serializable]
    public enum PermissionType
    {
        None,
        Observer,
        Owner
    }

    [Serializable]
    public struct MovementData
    {
        public string id;
        public List<Vector2> points;
    }
}