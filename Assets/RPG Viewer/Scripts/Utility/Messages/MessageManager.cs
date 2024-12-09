using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG
{
    public class MessageManager : MonoBehaviour
    {
        [SerializeField] private Message messagePrefab;
        [SerializeField] private ConfirmationPanel confirmationPrefab;
        [SerializeField] private Transform messageParent;
        [SerializeField] private Transform confirmationParent;

        static private List<Message> listOfMessages = new List<Message>();
        private static Queue<MessageData> messageQueue = new Queue<MessageData>();
        private static Queue<Confirmation> confirmationQueue = new Queue<Confirmation>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        private void Update()
        {
            // Check if there is any messages to show
            if (messageQueue.Count > 0)
            {
                // Dequeue first message and send it
                MessageData message = messageQueue.Dequeue();
                StartCoroutine(LogMessage(message.message, message.type));
            }

            // Check if there is any confirmations to show
            if (confirmationQueue.Count > 0)
            {
                // Dequeue first confirmation and send it
                Confirmation confirmation = confirmationQueue.Dequeue();
                SendConfirmation(confirmation);
            }
        }

        public static void QueueMessage(string message, MessageType type = MessageType.Info)
        {
            // Add message to queue
            messageQueue.Enqueue(new MessageData { message = message, type = type });
            Debug.Log($"Received new message: {message}");
        }
        public static void AskConfirmation(Confirmation confirmation)
        {
            // Add message to queue
            confirmationQueue.Enqueue(confirmation);
        }
        public static void RemoveMessage(string message)
        {
            // Find correct message, return if no message was found
            Message msg = listOfMessages.FirstOrDefault(item => item.message == message);
            if (msg == null) return;

            // Remove message from list and destroy it
            listOfMessages.Remove(msg);
            msg.Close();
        }
        public void CloseMessage(Message message)
        {
            // Check if message exists
            if (message == null) return;

            // Remove message from list and destroy it
            listOfMessages.Remove(message);
            message.Close();
        }

        private IEnumerator LogMessage(string message, MessageType type)
        {
            // Instantiate new message and attach it to correct parent
            Message msg = Instantiate(messagePrefab, messageParent);
            msg.transform.SetAsFirstSibling();

            // Add message to the list and load its data
            listOfMessages.Add(msg);
            msg.Load(message, type);

            // Close message after 5 seconds
            yield return new WaitForSeconds(5.0f);
            if (msg != null) CloseMessage(msg);
        }
        private void SendConfirmation(Confirmation confirmation)
        {
            // Create new confirmation panel and update its UI
            ConfirmationPanel panel = Instantiate(confirmationPrefab, confirmationParent);
            panel.UpdateUI(confirmation);
        }
    }

    public struct MessageData
    {
        public string message;
        public MessageType type;
    }
}