using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FunkyCode;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPG
{
    public class Session : MonoBehaviour
    {
        [Header("Walls")]
        [SerializeField] private WallManager wallManager;

        [Header("Lights")]
        [SerializeField] private LightManager lightManager;

        [Header("Notes")]
        [SerializeField] private NoteManager noteManager;

        [Header("Journals")]
        [SerializeField]
        private JournalManager journalManager;

        [Header("Tokens")]
        [SerializeField] private Token tokenPrefab;
        [SerializeField] private Transform tokenTransform;

        [Header("Grid")]
        [SerializeField] private SessionGrid grid;

        [Header("Sprite")]
        public SpriteRenderer sprite;
        [SerializeField] private Image background;

        public List<Token> Tokens = new List<Token>();
        private TokenData copyData;

        private List<Token> myTokens = new List<Token>();
        private int selectedToken = -1;

        public int Loaders;

        public SceneSettings Settings { get; private set; }


        [System.Obsolete]
        private void Start()
        {
            background.sprite = SessionManager.BackgroundSprite;
            if (SocketManager.SceneSettings != null)
            {
                SocketManager.SceneSettings = null;
                if (!String.IsNullOrEmpty(SessionManager.Scene)) LoadScene(SessionManager.Scene);
            }
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                if (FindObjectOfType<StateManager>(true).ToolState == ToolState.Pan && !string.IsNullOrEmpty(copyData.id))
                {
                    PasteToken(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
            }

            if (myTokens.Count != 0 && Input.GetKeyDown(KeyCode.Tab))
            {
                if (selectedToken > myTokens.Count - 1) selectedToken = 0;
                if (selectedToken >= 0)
                {
                    if (myTokens[selectedToken].Selection.gameObject.activeInHierarchy) myTokens[selectedToken].ToggleSelection();
                }
                selectedToken++;
                if (selectedToken > myTokens.Count - 1) selectedToken = 0;
                myTokens[selectedToken].ToggleSelection();
                FindObjectOfType<Camera2D>().FollowTarget(myTokens[selectedToken].transform);
            }
        }

        public async void LeaveSession()
        {
            await SocketManager.Socket.EmitAsync("leave-session", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean()) SceneManager.LoadScene("Menu");
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }

        public void CopyToken(TokenData data)
        {
            copyData = data;
        }
        private async void PasteToken(Vector2 pos)
        {
            copyData.position = pos;
            await SocketManager.Socket.EmitAsync("create-token", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(copyData));
        }

        [Obsolete]
        public async void LoadScene(string id)
        {
            UnloadScene();
            if (string.IsNullOrEmpty(id)) return;

            await SocketManager.Socket.EmitAsync("get-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    SceneSettings settings = JsonUtility.FromJson<SceneSettings>(callback.GetValue(1).ToString());
                    settings.id = id;

                    MessageManager.QueueMessage("Loading scene");
                    Settings = settings;

                    WebManager.Download(settings.data.image, true, async (bytes) =>
                    {
                        await UniTask.SwitchToMainThread();
                        Texture2D texture = new Texture2D(1, 1);
                        texture.LoadImage(bytes);
                        sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        FindObjectOfType<Camera2D>().UpdateSettings(texture);

                        await UniTask.WaitUntil(() => Loaders == 1);
                        LoadGrid(settings.grid);

                        await UniTask.WaitUntil(() => Loaders == 2);
                        LoadWalls(settings.walls);

                        await UniTask.WaitUntil(() => Loaders == 3);
                        LoadLights(settings.lights);

                        await UniTask.WaitUntil(() => Loaders == 4);
                        LoadTokens();

                        await UniTask.WaitUntil(() => Loaders == 5);
                        LoadNight(settings.data.nightStrength);

                        await UniTask.WaitUntil(() => Loaders == 6);
                        FindObjectOfType<InitiativeController>(true).LoadHolders(settings.initiatives);

                        await UniTask.WaitUntil(() => Loaders == 7);
                        LoadNotes();

                        await UniTask.WaitUntil(() => Loaders == 8);
                        background.gameObject.SetActive(false);
                        ChangeFog(FogState.Player);
                        if (SessionManager.IsMaster) FindObjectOfType<StateManager>(true).SelectHidden();
                        MessageManager.RemoveMessage("Loading scene");
                    });
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        public void UnloadScene()
        {
            Loaders = 0;
            background.gameObject.SetActive(true);

            Settings = null;
            for (int i = 0; i < Tokens.Count; i++) Destroy(Tokens[i].gameObject);
            Tokens.Clear();
            myTokens.Clear();
            wallManager.UnloadWalls();
            lightManager.UnloadLights();
            noteManager.UnloadNotes();

            sprite.sprite = null;

            grid.UpdateColor(Color.black);
            grid.GenerateGrid(Vector2Int.zero, Vector2.zero, 0);
            FindObjectOfType<InitiativeController>(true).UnloadHolders();
        }
        public void MoveCenter()
        {
            FindObjectOfType<Camera2D>().MoveToPosition(Vector2.zero);
        }

        private void LoadGrid(GridData data)
        {
            grid.UpdateColor(data.color);
            grid.GenerateGrid(data.dimensions, data.position, data.cellSize);
            Loaders++;
        }
        private void LoadWalls(List<WallData> data)
        {
            wallManager.GenerateWalls(data);
            Loaders++;
        }
        private void LoadLights(List<LightData> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                lightManager.AddLight(data[i]);
            }

            Loaders++;
        }
        private async void LoadNotes()
        {
            await SocketManager.Socket.EmitAsync("get-notes", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue(0).GetBoolean())
                {
                    var notes = callback.GetValue(1).EnumerateArray().ToArray();
                    if (notes.Length == 0)
                    {
                        Loaders++;
                        return;
                    }

                    for (int i = 0; i < notes.Length; i++)
                    {
                        var data = JsonUtility.FromJson<NoteData>(notes[i].ToString());
                        data.id = notes[i].GetProperty("_id").GetString();
                        CreateNote(data);
                    }

                    Loaders++;
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Settings.id);

        }

        private async void LoadTokens()
        {
            await SocketManager.Socket.EmitAsync("get-tokens", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue(0).GetBoolean())
                {
                    var tokens = callback.GetValue(1).EnumerateArray().ToArray();
                    if (tokens.Length == 0)
                    {
                        Loaders++;
                        return;
                    }

                    for (int i = 0; i < tokens.Length; i++)
                    {
                        var data = JsonUtility.FromJson<TokenData>(tokens[i].ToString());
                        data.id = tokens[i].GetProperty("_id").GetString();
                        CreateToken(data);
                    }

                    Loaders++;
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Settings.id);

        }
        public void LoadNight(float strength)
        {
            float s = 0.004f * strength;
            float v = 1.0f - 0.008f * strength;
            sprite.color = Color.HSVToRGB(240.0f / 360.0f, s, v);

            Loaders++;
        }
        public void ChangeFog(FogState state)
        {
            if (Settings == null) return;
            if (!Settings.fogOfWar.enabled)
            {
                Lighting2D.LightmapPresets[0].darknessColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                Lighting2D.LightmapPresets[1].darknessColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                return;
            }

            if (state == FogState.Player)
            {
                Lighting2D.LightmapPresets[0].darknessColor = Settings.fogOfWar.color;
                Lighting2D.LightmapPresets[1].darknessColor = Settings.fogOfWar.color;

                if (Settings.fogOfWar.globalLighting) Lighting2D.LightmapPresets[0].darknessColor = new Color(Settings.fogOfWar.color.r, Settings.fogOfWar.color.g, Settings.fogOfWar.color.b, 0.0f);
            }
            else if (state == FogState.Vision)
            {
                Lighting2D.LightmapPresets[0].darknessColor = new Color(Settings.fogOfWar.color.r, Settings.fogOfWar.color.g, Settings.fogOfWar.color.b, 0.0f);
                Lighting2D.LightmapPresets[1].darknessColor = new Color(Settings.fogOfWar.color.r, Settings.fogOfWar.color.g, Settings.fogOfWar.color.b, 0.9f);
            }
            else
            {
                Lighting2D.LightmapPresets[0].darknessColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                Lighting2D.LightmapPresets[1].darknessColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }
        }

        public void ToggleDoor(string id, bool state) => wallManager.ToggleWall(id, state);
        public void ModifyWall(WallData data) => wallManager.ModifyWall(data);

        public void CreateLight(LightData data) => lightManager.AddLight(data);
        public void ModifyLight(LightData data) => lightManager.ModifyLight(data);
        public void RemoveLight(string id) => lightManager.RemoveLight(id);

        public void CreateToken(TokenData data)
        {
            WebManager.Download(data.image, true, async (bytes) =>
            {
                await UniTask.SwitchToMainThread();

                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);

                Sprite s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Token token = Instantiate(tokenPrefab, tokenTransform);
                if (!SessionManager.IsMaster && !data.enabled) token.gameObject.SetActive(false);

                token.LoadData(data, s);
                Tokens.Add(token);
                if (token.Permission.permission == PermissionType.Owner) myTokens.Add(token);
            });
        }
        public void MoveToken(string id, MovementData data)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null) token.Move(data);
        }
        public void ModifyToken(string id, TokenData data)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null)
            {
                if (token.Data.image != data.image)
                {
                    WebManager.Download(data.image, true, async (bytes) =>
                    {
                        await UniTask.SwitchToMainThread();
                        Texture2D texture = new Texture2D(1, 1);
                        texture.LoadImage(bytes);

                        Sprite s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        token.LoadData(data, s);
                    });
                }
                else token.LoadData(data, null);
            }
        }

        [Obsolete]
        public async void RemoveToken(string id)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null)
            {
                if (myTokens.Contains(token)) myTokens.Remove(token);
                Tokens.Remove(token);
                Destroy(token.gameObject);
            }

            await UniTask.WaitForEndOfFrame();
        }
        public void UpdateVisibility(string id, bool enabled)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null)
            {
                if (!SessionManager.IsMaster) token.gameObject.SetActive(enabled);
                token.EnableToken(enabled);
            }
        }
        public void UpdateElevation(string id, string elevation)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null) token.SetElevation(elevation);
        }
        public void UpdateConditions(string id, int conditions)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null) token.UpdateConditions(conditions);
        }
        public void LockToken(string id, bool locked)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null) token.SetLocked(locked);
        }
        public void UpdateHealth(string id, int health)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null) token.SetHealth(health);
        }
        public void RotateToken(string id, float angle)
        {
            Token token = Tokens.FirstOrDefault(x => x.Data.id == id);
            if (token != null) token.UpdateRotation(angle);
        }

        public void SelectToken(Token token)
        {
            for (int i = 0; i < Tokens.Count; i++)
            {
                Tokens[i].Permission.permission = PermissionType.Owner;
                if (Tokens[i] != token && token != null)
                {
                    Tokens[i].Permission.permission = PermissionType.None;
                }

                Tokens[i].LoadLights();
            }

            FindObjectOfType<Camera2D>().FollowTarget(token == null ? null : token.transform);
        }

        public void CreateNote(NoteData data) => noteManager.AddNote(data);
        public void ModifyNoteText(string id, string newText) => noteManager.ModifyText(id, newText);
        public void ModifyNoteHeader(string id, string newText) => noteManager.ModifyHeader(id, newText);
        public void ModifyNoteImage(string id, string newImage) => noteManager.ModifyImage(id, newImage);
        public void MoveNote(string id, Vector2 newPosition) => noteManager.Move(id, newPosition);
        public void SetNotePublic(string id, bool publicState) => noteManager.SetPublic(id, publicState);
        public void RemoveNote(string id) => noteManager.Remove(id);
        public void ShowNote(string id) => noteManager.Show(id);

        public void ModifyJournalText(string id, string newText) => journalManager.ModifyText(id, newText);
        public void ModifyJournalHeader(string id, string newText) => journalManager.ModifyHeader(id, newText);
        public void ModifyJournalImage(string id, string newImage) => journalManager.ModifyImage(id, newImage);
        public void RemoveJournal(string id) => journalManager.RemoveJournal(id);
        public void ShowJournal(JournalData data) => journalManager.ShowJournal(data);
        public void SetCollaborators(string id, List<Collaborator> collaborators) => journalManager.SetCollaborators(id, collaborators);
    }
}