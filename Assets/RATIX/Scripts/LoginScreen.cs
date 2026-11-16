using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Pantalla de login. Por ahora es visual (sin servidor): valida que el
    /// usuario no esté vacío y avisa con <see cref="LoggedIn"/> para entrar al lobby.
    /// </summary>
    public class LoginScreen : MonoBehaviour
    {
        [SerializeField] private TMP_InputField userInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button guestButton;
        [SerializeField] private TMP_Text errorLabel;
        [Tooltip("Posición en la vista al mostrarse (se edita estacionado).")]
        [SerializeField] private Vector2 homePosition = Vector2.zero;

        /// <summary>Se dispara con el nombre de usuario cuando el login es válido.</summary>
        public event Action<string> LoggedIn;

        private bool _wired;

        private void Awake()
        {
            Resolve();
            Wire();
        }

        private void Resolve()
        {
            if (userInput == null) userInput = FindInput("Window/UserInput");
            if (passwordInput == null) passwordInput = FindInput("Window/PasswordInput");
            if (loginButton == null) loginButton = FindButton("Window/LoginButton");
            if (guestButton == null) guestButton = FindButton("Window/GuestButton");
            if (errorLabel == null)
            {
                var t = transform.Find("Window/Error");
                if (t != null) errorLabel = t.GetComponent<TMP_Text>();
            }
        }

        private TMP_InputField FindInput(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<TMP_InputField>() : null;
        }

        private Button FindButton(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private void Wire()
        {
            if (_wired) return;
            _wired = true;

            if (loginButton != null) loginButton.onClick.AddListener(Login);
            if (guestButton != null) guestButton.onClick.AddListener(PlayAsGuest);
            if (userInput != null) userInput.onSubmit.AddListener(_ => Login());
            if (passwordInput != null) passwordInput.onSubmit.AddListener(_ => Login());
            if (passwordInput != null) passwordInput.contentType = TMP_InputField.ContentType.Password;
        }

        /// <summary>Muestra el login desde el principio.</summary>
        public void Open()
        {
            Resolve();
            Wire();

            if (transform is RectTransform rt)
            {
                rt.anchoredPosition = homePosition;
                rt.SetAsLastSibling();
            }

            gameObject.SetActive(true);

            if (passwordInput != null) passwordInput.text = string.Empty;
            SetError(string.Empty);

            if (userInput != null) userInput.Select();
        }

        public void Close() => gameObject.SetActive(false);

        public void Login()
        {
            string user = userInput != null ? userInput.text.Trim() : string.Empty;

            if (string.IsNullOrEmpty(user))
            {
                SetError("Escribe tu usuario para continuar.");
                return;
            }

            Debug.Log("[RATIX] Login: " + user);
            SetError(string.Empty);
            LoggedIn?.Invoke(user);
            Close();
        }

        public void PlayAsGuest()
        {
            Debug.Log("[RATIX] Login: invitado");
            LoggedIn?.Invoke("Guest");
            Close();
        }

        private void SetError(string message)
        {
            if (errorLabel == null) return;
            errorLabel.text = message;
        }
    }
}
