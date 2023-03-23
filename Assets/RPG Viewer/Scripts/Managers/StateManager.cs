using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class StateManager : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private GameObject panSelection;

        [Header("Measure")]
        [SerializeField] private GameObject measureSelection;
        [SerializeField] private GameObject measurePanel;
        [SerializeField] private GameObject preciseSelection;
        [SerializeField] private GameObject gridSelection;

        [Header("Fog")]
        [SerializeField] private GameObject fogSelection;
        [SerializeField] private GameObject fogPanel;
        [SerializeField] private GameObject playerSelection;
        [SerializeField] private GameObject visionSelection;
        [SerializeField] private GameObject hiddenSelection;
        [SerializeField] private GameObject fogButton;

        [Header("Lights")]
        [SerializeField] private GameObject lightSelection;
        [SerializeField] private GameObject lightPanel;
        [SerializeField] private GameObject lightButton;
        [SerializeField] private GameObject createLightSelection;
        [SerializeField] private GameObject deleteLightSelection;

        [Header("Ping")]
        [SerializeField] private GameObject pingSelection;
        [SerializeField] private GameObject pingPanel;
        [SerializeField] private GameObject regularSelection;
        [SerializeField] private GameObject pointerSelection;

        [Header("Notes")]
        [SerializeField] private GameObject noteSelection;
        [SerializeField] private GameObject notePanel;
        [SerializeField] private GameObject createNoteSelection;
        [SerializeField] private GameObject deleteNoteSelection;

        [Header("Sync")]
        [SerializeField] private GameObject stateButton;
        [SerializeField] private Image stateSprite;
        [SerializeField] private Sprite openState;
        [SerializeField] private Sprite closeState;

        [Header("State")]
        public ToolState ToolState;
        public FogState FogState;
        public LightState LightState;
        public NoteState NoteState;
        public MeasurementType MeasureType;
        public PingType PingType;
        public bool allowMeaure;

        [SerializeField] private List<GameObject> hiddenButtons = new List<GameObject>();

        private void Start()
        {
            UsePan();
            AllowMeasure();
        }
        private void Update()
        {
            var isMaster = SessionManager.IsMaster;

            if (hiddenButtons[0].activeInHierarchy && SessionManager.Session.sprite.sprite == null)
            {
                for (int i = 0; i < hiddenButtons.Count; i++)
                {
                    hiddenButtons[i].SetActive(false);
                }
            }

            if (!hiddenButtons[0].activeInHierarchy && SessionManager.Session.sprite.sprite != null)
            {
                for (int i = 0; i < hiddenButtons.Count; i++)
                {
                    hiddenButtons[i].SetActive(true);
                }
            }

            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) && panSelection.transform.parent.gameObject.activeInHierarchy) UsePan();
                if (Input.GetKeyDown(KeyCode.Alpha2) && measureSelection.transform.parent.gameObject.activeInHierarchy) UseMeasure();
                if (Input.GetKeyDown(KeyCode.Alpha3) && pingSelection.transform.parent.gameObject.activeInHierarchy) UsePing();
                if (Input.GetKeyDown(KeyCode.Alpha4) && pingSelection.transform.parent.gameObject.activeInHierarchy) UseNotes();
                if (Input.GetKeyDown(KeyCode.Alpha5) && fogSelection.transform.parent.gameObject.activeInHierarchy) UseFog();
                if (Input.GetKeyDown(KeyCode.Alpha6) && lightSelection.transform.parent.gameObject.activeInHierarchy) UseLight();
            }

            fogButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            lightButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            lightButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            stateButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            stateSprite.sprite = SessionManager.Synced ? openState : closeState;
        }

        public void BlockMeasure()
        {
            allowMeaure = false;
        }
        public void AllowMeasure()
        {
            allowMeaure = true;
        }

        public void UsePan()
        {
            ToolState = ToolState.Pan;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(true);
            fogSelection.SetActive(false);
            pingSelection.SetActive(false);
            lightSelection.SetActive(false);
            fogPanel.SetActive(false);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            lightPanel.SetActive(false);
            pingPanel.SetActive(false);
        }

        public void UseMeasure()
        {
            ToolState = ToolState.Measure;
            measurePanel.SetActive(true);
            panSelection.SetActive(false);
            measureSelection.SetActive(true);
            fogSelection.SetActive(false);
            pingSelection.SetActive(false);
            fogPanel.SetActive(false);
            lightSelection.SetActive(false);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            lightPanel.SetActive(false);
            pingPanel.SetActive(false);

            if (!preciseSelection.activeInHierarchy && !gridSelection.activeInHierarchy) SelectPrecise();
        }
        public void SelectPrecise()
        {
            gridSelection.SetActive(false);
            preciseSelection.SetActive(true);

            MeasureType = MeasurementType.Precise;
        }
        public void SelectGrid()
        {
            gridSelection.SetActive(true);
            preciseSelection.SetActive(false);

            MeasureType = MeasurementType.Grid;
        }

        public void UseFog()
        {
            ToolState = ToolState.Pan;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(false);
            fogSelection.SetActive(true);
            pingSelection.SetActive(false);
            lightSelection.SetActive(false);
            fogPanel.SetActive(true);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            lightPanel.SetActive(false);
            pingPanel.SetActive(false);

            if (!playerSelection.activeInHierarchy && !visionSelection.activeInHierarchy && !hiddenSelection.activeInHierarchy) playerSelection.SetActive(true);
        }
        public void SelectPlayer()
        {
            playerSelection.SetActive(true);
            visionSelection.SetActive(false);
            hiddenSelection.SetActive(false);

            FogState = FogState.Player;
            GetComponent<Session>().ChangeFog(FogState);
        }
        public void SelectVision()
        {
            playerSelection.SetActive(false);
            hiddenSelection.SetActive(false);
            visionSelection.SetActive(true);

            FogState = FogState.Vision;
            GetComponent<Session>().ChangeFog(FogState);
        }
        public void SelectHidden()
        {
            playerSelection.SetActive(false);
            hiddenSelection.SetActive(true);
            visionSelection.SetActive(false);

            FogState = FogState.Hidden;
            GetComponent<Session>().ChangeFog(FogState);
        }

        public void UsePing()
        {
            ToolState = ToolState.Ping;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(false);
            fogSelection.SetActive(false);
            fogPanel.SetActive(false);
            lightSelection.SetActive(false);
            pingSelection.SetActive(true);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            lightPanel.SetActive(false);
            pingPanel.SetActive(true);

            if (!regularSelection.activeInHierarchy && !pointerSelection.activeInHierarchy) SelectPing();
        }
        public void SelectPing()
        {
            pointerSelection.SetActive(false);
            regularSelection.SetActive(true);

            PingType = PingType.Ping;
        }
        public void SelectPointer()
        {
            pointerSelection.SetActive(true);
            regularSelection.SetActive(false);

            PingType = PingType.Pointer;
        }

        public void UseLight()
        {
            ToolState = ToolState.Light;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(false);
            fogSelection.SetActive(false);
            fogPanel.SetActive(false);
            pingSelection.SetActive(false);
            lightSelection.SetActive(true);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            lightPanel.SetActive(true);
            pingPanel.SetActive(false);

            if (!createLightSelection.activeInHierarchy && !deleteLightSelection.activeInHierarchy) CreateLight();
        }
        public void CreateLight()
        {
            createLightSelection.SetActive(true);
            deleteLightSelection.SetActive(false);

            LightState = LightState.Create;
        }
        public void DeleteLight()
        {
            createLightSelection.SetActive(false);
            deleteLightSelection.SetActive(true);

            LightState = LightState.Delete;
        }

        public void UseNotes()
        {
            ToolState = ToolState.Notes;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(false);
            fogSelection.SetActive(false);
            fogPanel.SetActive(false);
            pingSelection.SetActive(false);
            lightSelection.SetActive(false);
            noteSelection.SetActive(true);
            notePanel.SetActive(true);
            lightPanel.SetActive(false);
            pingPanel.SetActive(false);

            if (!createNoteSelection.activeInHierarchy && !deleteNoteSelection.activeInHierarchy) CreateNote();
        }
        public void CreateNote()
        {
            createNoteSelection.SetActive(true);
            deleteNoteSelection.SetActive(false);

            NoteState = NoteState.Create;
        }
        public void DeleteNote()
        {
            createNoteSelection.SetActive(false);
            deleteNoteSelection.SetActive(true);

            NoteState = NoteState.Delete;
        }
    }

    public enum ToolState
    {
        Pan,
        Light,
        Measure,
        Ping,
        Notes
    }
    public enum FogState
    {
        Player,
        Vision,
        Hidden
    }
    public enum LightState
    {
        Create,
        Delete
    }
    public enum NoteState
    {
        Create,
        Delete
    }

}