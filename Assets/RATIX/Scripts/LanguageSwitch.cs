using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Selector de idioma del panel de Ajustes: alterna EN/ES y muestra el
    /// idioma activo. El cambio es inmediato (sin recargar escena).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LanguageSwitch : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private void Awake()
        {
            if (label == null)
            {
                var t = transform.Find("Label");
                if (t != null) label = t.GetComponent<TMP_Text>();
            }
        }

        private void OnEnable()
        {
            L.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            L.LanguageChanged -= Refresh;
        }

        private void Start()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveListener(Toggle);
                button.onClick.AddListener(Toggle);
            }
        }

        public void Toggle() => L.Toggle();

        public void Refresh()
        {
            if (label == null) return;
            label.text = L.Current == "es" ? "ESPAÑOL" : "ENGLISH";
        }
    }
}
