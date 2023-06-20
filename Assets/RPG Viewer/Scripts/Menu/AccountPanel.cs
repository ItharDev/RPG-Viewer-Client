using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;

namespace RPG
{
    public class AccountPanel : MonoBehaviour
    {
        [SerializeField] private MenuHandler menu;
        [SerializeField] private GameObject signInButton;
        [SerializeField] private GameObject registerButton;
        [SerializeField] private GameObject signOutButton;
        [SerializeField] private GameObject accountInfo;
        [SerializeField] private TMP_Text infoText;

        [Header("Sign in")]
        [SerializeField] private GameObject signInPanel;
        [SerializeField] private TMP_InputField signInEmail;
        [SerializeField] private TMP_InputField signInPassword;

        [Header("Register")]
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private TMP_InputField registerEmail;
        [SerializeField] private TMP_InputField registerPassword;
        [SerializeField] private TMP_InputField registerVerification;
        [SerializeField] private TMP_InputField registerName;

        private RectTransform rect;

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();

            // Add event listeners
            Events.OnSignIn.AddListener(SignIn);
            Events.OnSignOut.AddListener(SignOut);
            Events.OnRegister.AddListener(SignIn);
            Events.OnConnected.AddListener(AutomaticSignIn);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSignIn.RemoveListener(SignIn);
            Events.OnSignOut.RemoveListener(SignOut);
            Events.OnRegister.RemoveListener(SignIn);
            Events.OnConnected.RemoveListener(AutomaticSignIn);
        }

        public void OpenSignIn()
        {
            // Close panel if it's open
            if (rect.sizeDelta.x != 0 && signInPanel.activeInHierarchy)
            {
                ClosePanel();
                return;
            }

            // Close registeration panel
            if (registerPanel.activeInHierarchy) LeanTween.size(rect, new Vector2(0.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                // Open sign in panel
                registerPanel.SetActive(false);
                signInPanel.SetActive(true);
                LeanTween.size(rect, new Vector2(340.0f, 150.0f), 0.2f);
                menu.OpenSignIn?.Invoke();
            });
            else
            {
                // Open sign in panel
                signInPanel.SetActive(true);
                LeanTween.size(rect, new Vector2(340.0f, 150.0f), 0.2f);
                menu.OpenSignIn?.Invoke();
            }
        }
        public void OpenRegister()
        {
            // Close panel if it's open
            if (rect.sizeDelta.x != 0 && registerPanel.activeInHierarchy)
            {
                ClosePanel();
                return;
            }

            // Close sign in panel
            if (signInPanel.activeInHierarchy) LeanTween.size(rect, new Vector2(0.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                // Open register panel
                registerPanel.SetActive(true);
                signInPanel.SetActive(false);
                LeanTween.size(rect, new Vector2(340.0f, 230.0f), 0.2f);
                menu.OpenSignIn?.Invoke();
            });
            else
            {
                // Open register panel
                registerPanel.SetActive(true);
                LeanTween.size(rect, new Vector2(340.0f, 230.0f), 0.2f);
                menu.OpenRegister?.Invoke();
            }
        }
        public void ClosePanel()
        {
            // Close panel
            LeanTween.size(rect, new Vector2(0.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                // Hide panels
                registerPanel.SetActive(false);
                signInPanel.SetActive(false);
            });
        }

        private void SignIn(string id, string name)
        {
            // Update user data
            GameData.User = new UserInfo(id, name);
            PlayerPrefs.SetString("user id", id);

            // Reset input fields
            signInEmail.text = "";
            signInPassword.text = "";
            registerEmail.text = "";
            registerPassword.text = "";
            registerVerification.text = "";
            registerName.text = "";
            infoText.text = name;

            // Enable / disable buttons
            signInButton.SetActive(false);
            signOutButton.SetActive(true);
            registerButton.SetActive(false);
            accountInfo.SetActive(true);

            ClosePanel();
        }
        private void SignOut()
        {
            // Reset user data
            GameData.User = default;
            PlayerPrefs.SetString("user id", "");

            // Reset input fields
            signInEmail.text = "";
            signInPassword.text = "";
            registerEmail.text = "";
            registerPassword.text = "";
            registerVerification.text = "";
            registerName.text = "";
            infoText.text = "";

            // Enable / disable buttons
            if (SocketManager.IsConnected) signInButton.SetActive(true);
            if (SocketManager.IsConnected) registerButton.SetActive(true);
            accountInfo.SetActive(false);
            signOutButton.SetActive(false);
        }

        private void AutomaticSignIn()
        {
            signInButton.SetActive(true);
            registerButton.SetActive(true);
            // Fetch stored user id
            string userId = PlayerPrefs.GetString("user id");

            // Check if previous login information exists
            if (string.IsNullOrEmpty(userId)) return;

            SocketManager.EmitAsync("sign-in", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Read account data
                    string name = callback.GetValue(1).GetString();
                    string id = callback.GetValue(2).GetString();

                    MessageManager.QueueMessage($"Signed in as {name}");

                    // Send sign in event
                    Events.OnSignIn?.Invoke(id, name);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, userId, "", "");
        }

        public void SignIn()
        {
            // Check if sign in information is valid
            if (string.IsNullOrEmpty(signInEmail.text) || string.IsNullOrEmpty(signInPassword.text)) return;

            SocketManager.EmitAsync("sign-in", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Read account data
                    string name = callback.GetValue(1).GetString();
                    string id = callback.GetValue(2).GetString();

                    PlayerPrefs.SetString("user id", id);

                    MessageManager.QueueMessage($"Signed in as {name}");

                    // Send sign in event
                    Events.OnSignIn?.Invoke(id, name);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, "", signInEmail.text, signInPassword.text);
        }
        public void SignOutButton()
        {
            SocketManager.EmitAsync("sign-out", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    MessageManager.QueueMessage("Signed out");

                    // Send sign out event
                    Events.OnSignOut?.Invoke();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        public void Register()
        {
            // Check if registeration information is valid
            if (string.IsNullOrEmpty(registerEmail.text) || string.IsNullOrEmpty(registerName.text) || string.IsNullOrEmpty(registerPassword.text)) return;

            // Check if passwords match
            if (registerPassword.text != registerVerification.text)
            {
                MessageManager.QueueMessage("Passwords do not match");
                return;
            }

            SocketManager.EmitAsync("register", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    MessageManager.QueueMessage($"Account registered successfully");

                    // Send registeration event
                    Events.OnRegister?.Invoke();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, registerEmail.text, registerName.text, registerPassword.text);
        }
    }
}