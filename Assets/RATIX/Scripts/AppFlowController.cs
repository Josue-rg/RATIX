using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Flujo de entrada: splash (después del logo de Unity) → login → lobby.
    /// El lobby ya está montado detrás; estas pantallas lo cubren hasta entrar.
    /// </summary>
    public class AppFlowController : MonoBehaviour
    {
        [Header("Pantallas")]
        [SerializeField] private SplashScreen splash;
        [SerializeField] private LoginScreen login;

        [Header("Opciones")]
        [Tooltip("Si está activo, se salta el splash al dar Play.")]
        [SerializeField] private bool skipSplash;

        private void Awake()
        {
            if (login != null) login.gameObject.SetActive(false);
            if (splash != null) splash.gameObject.SetActive(true);
        }

        private void Start()
        {
            if (splash != null && !skipSplash)
            {
                splash.Finished -= ShowLogin;
                splash.Finished += ShowLogin;
                splash.Play();
            }
            else
            {
                ShowLogin();
            }
        }

        /// <summary>Oculta el splash y muestra el login.</summary>
        public void ShowLogin()
        {
            if (splash != null) splash.Hide();

            if (login != null)
            {
                login.LoggedIn -= OnLoggedIn;
                login.LoggedIn += OnLoggedIn;
                login.Open();
            }
        }

        private void OnLoggedIn(string user)
        {
            Debug.Log("[RATIX] Entrando al lobby como: " + user);
            if (login != null) login.Close();
        }
    }
}
