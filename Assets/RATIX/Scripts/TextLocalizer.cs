using TMPro;
using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Marca un texto estático con un ID y lo actualiza cuando cambia el idioma.
    /// No se usa en datos dinámicos (nombres, chats, contadores).
    /// </summary>
    [DisallowMultipleComponent]
    public class TextLocalizer : MonoBehaviour
    {
        [SerializeField] private string key;
        [SerializeField] private TMP_Text target;

        public string Key => key;

        private void Awake()
        {
            if (target == null) target = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            L.LanguageChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            L.LanguageChanged -= Apply;
        }

        public void Apply()
        {
            if (target == null) target = GetComponent<TMP_Text>();
            if (target == null || string.IsNullOrEmpty(key)) return;

            target.text = L.Get(key, target.text);
        }

        /// <summary>Para la herramienta de editor.</summary>
        public void SetKey(string value) => key = value;
    }
}
