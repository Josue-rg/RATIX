using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Ajustes del lobby: los cambios (idioma incluido) no se aplican al pulsar,
    /// sino al pulsar APPLY y confirmar en el mini modal.
    /// </summary>
    public class SettingsModalController : MonoBehaviour
    {
        [SerializeField] private LanguageSwitch languageSwitch;
        [SerializeField] private Button applyButton;
        [SerializeField] private ConfirmModal confirmModal;

        private bool wired;

        private void Awake()
        {
            Resolve();
            Wire();
        }

        private void OnEnable()
        {
            Resolve();
            if (languageSwitch != null) languageSwitch.ResetPending();
        }

        private void Resolve()
        {
            if (languageSwitch == null) languageSwitch = GetComponentInChildren<LanguageSwitch>(true);
            if (applyButton == null)
            {
                var t = transform.Find("Window/ApplyButton");
                if (t != null) applyButton = t.GetComponent<Button>();
            }
            if (confirmModal == null)
            {
                var t = GameObject.Find("LobbyCanvas")?.transform.Find("ConfirmModal");
                if (t != null) confirmModal = t.GetComponent<ConfirmModal>();
            }
        }

        private void Wire()
        {
            if (wired) return;
            wired = true;

            if (applyButton != null)
            {
                applyButton.onClick.RemoveListener(AskApply);
                applyButton.onClick.AddListener(AskApply);
            }
        }

        /// <summary>Pide confirmación antes de aplicar los cambios.</summary>
        public void AskApply()
        {
            Resolve();

            if (confirmModal == null)
            {
                ApplyConfirmed();
                return;
            }

            confirmModal.Show(
                L.Get("confirm.title", "ARE YOU SURE?"),
                L.Get("settings.confirm_apply", "Apply all settings changes?"),
                ApplyConfirmed);
        }

        private void ApplyConfirmed()
        {
            if (languageSwitch != null) L.SetLanguage(languageSwitch.Pending);

            Debug.Log("[RATIX] Settings applied.");
            gameObject.SetActive(false);
        }
    }
}
