using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Selector de idioma del panel de Ajustes. No aplica el idioma al pulsar:
    /// deja un valor pendiente que SettingsModalController aplica al confirmar.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LanguageSwitch : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        /// <summary>Idioma elegido, pendiente de aplicar con APPLY.</summary>
        public string Pending { get; private set; }

        private void Awake()
        {
            if (label == null)
            {
                var t = transform.Find("Label");
                if (t != null) label = t.GetComponent<TMP_Text>();
            }
        }

        private void OnEnable() => ResetPending();

        private void Start()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveListener(Toggle);
                button.onClick.AddListener(Toggle);
            }

            Refresh();
        }

        /// <summary>Vuelve al idioma aplicado actualmente.</summary>
        public void ResetPending()
        {
            Pending = L.Current;
            Refresh();
        }

        public void Toggle()
        {
            Pending = Pending == "es" ? "en" : "es";
            Refresh();
        }

        private void Refresh()
        {
            if (label == null) return;
            label.text = Pending == "es" ? "ESPAÑOL" : "ENGLISH";
        }
    }
}
