using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG
{
    public class MessageManager : MonoBehaviour
    {
        public static MessageManager Instance { get; private set; }
        private static Queue<string> Messages = new Queue<string>();

        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private Transform parent;
        [SerializeField] static private List<GameObject> listOfMessages = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        private void OnEnable()
        {
            SceneManager.sceneUnloaded += ClearMessages;
        }
        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= ClearMessages;
        }
        private void Update()
        {
            if (Messages.Count > 0) StartCoroutine(LogMessage(Messages.Dequeue()));
        }

        public static void QueueMessage(string message)
        {
            Debug.Log(message);
            Messages.Enqueue(message);
        }
        public static void RemoveMessage(string message)
        {
           var obj = listOfMessages.FirstOrDefault(x => x.GetComponentInChildren<TMP_Text>(true).text == message);
            if (obj != null)
            {
                listOfMessages.Remove(obj);
                Destroy(obj);
            }
        }
        public void CloseMessage(GameObject message)
        {
            if (message == null) return;
            listOfMessages.Remove(message);
            Destroy(message);
        }
        private void ClearMessages(Scene current)
        {
            for (int i = 0; i < listOfMessages.Count; i++)
            {
                CloseMessage(listOfMessages[i]);
            }
        }

        private IEnumerator LogMessage(string message)
        {
            GameObject obj = Instantiate(messagePrefab, parent);
            obj.transform.SetAsFirstSibling();
            listOfMessages.Add(obj);
            TMP_Text LogText = obj.GetComponentInChildren<TMP_Text>();
            LogText.text = message;
            yield return new WaitForSeconds(5.0f);
            if (obj != null) CloseMessage(obj);
        }
    }
}