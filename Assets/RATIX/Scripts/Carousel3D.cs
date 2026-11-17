using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Carrusel visual 3D por CLIC: la tarjeta del centro va al frente (escala y opacidad
    /// completas) y las demÃ¡s quedan a los lados, mÃ¡s pequeÃ±as y desvanecidas. Al hacer
    /// clic sobre una tarjeta, esta pasa al frente y las demÃ¡s se recorren con animaciÃ³n
    /// suave. No usa botones.
    /// </summary>
    public class Carousel3D : MonoBehaviour
    {
        [SerializeField] private RectTransform[] cards;
        [SerializeField] private CanvasGroup[] groups;
        [SerializeField] private float spacing = 170f;
        [SerializeField] private float lerpSpeed = 6f;
        [SerializeField] private TMP_Text selectionLabel;
        [SerializeField] private string[] selectionNames;

        private int index;

        /// <summary>Índice de la tarjeta que está al frente.</summary>
        public int SelectedIndex => index;

        /// <summary>Nombre de la tarjeta seleccionada (vacío si no hay).</summary>
        public string SelectedName =>
            selectionNames != null && index >= 0 && index < selectionNames.Length ? selectionNames[index] : string.Empty;

        private Image[] rings;

        private void Awake()
        {
            CacheRings();
            WireClicks();
        }

        private void CacheRings()
        {
            rings = new Image[cards != null ? cards.Length : 0];
            for (int i = 0; i < rings.Length; i++)
            {
                if (cards[i] == null) continue;
                var ring = cards[i].Find("Ring");
                if (ring != null) rings[i] = ring.GetComponent<Image>();
            }
        }

        private void Start()
        {
            Snap();
        }

        private void OnEnable()
        {
            L.LanguageChanged += UpdateSelectionLabel;
        }

        private void OnDisable()
        {
            L.LanguageChanged -= UpdateSelectionLabel;
        }

        private void WireClicks()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;
                int captured = i;

                var btn = cards[i].GetComponent<Button>();
                if (btn == null)
                {
                    var img = cards[i].GetComponent<Image>();
                    if (img == null)
                        img = cards[i].gameObject.AddComponent<Image>();
                    img.color = new Color(1f, 1f, 1f, 0f); // transparente, solo para captar clics
                    img.raycastTarget = true;
                    btn = cards[i].gameObject.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btn.transition = Selectable.Transition.None;
                }
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Select(captured));

                // Children (icon/photo) must not block the card click.
                var childImgs = cards[i].GetComponentsInChildren<Image>(true);
                foreach (var c in childImgs)
                    if (c.transform != cards[i].transform) c.raycastTarget = false;
            }
        }

        private void Update()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;
                int d = Wrap(i - index);
                int abs = d < 0 ? -d : d;

                float targetScale = abs == 0 ? 1f : (abs == 1 ? 0.82f : 0.66f);
                float targetAlpha = abs == 0 ? 1f : (abs == 1 ? 0.4f : 0.12f);
                float tx = d * spacing;

                var rt = cards[i];
                rt.anchoredPosition = new Vector2(
                    Mathf.Lerp(rt.anchoredPosition.x, tx, Time.deltaTime * lerpSpeed),
                    rt.anchoredPosition.y);
                float s = Mathf.Lerp(rt.localScale.x, targetScale, Time.deltaTime * lerpSpeed);
                rt.localScale = new Vector3(s, s, 1f);
                if (groups[i] != null)
                    groups[i].alpha = Mathf.Lerp(groups[i].alpha, targetAlpha, Time.deltaTime * lerpSpeed);
                if (rings != null && i < rings.Length && rings[i] != null)
                    rings[i].enabled = abs == 0;
            }
        }

        /// <summary>Brings the given card to the front.</summary>
        public void Select(int i)
        {
            if (cards == null || i < 0 || i >= cards.Length) return;
            index = i;
            UpdateSelectionLabel();
        }

        private void UpdateSelectionLabel()
        {
            if (selectionLabel == null || selectionNames == null) return;
            if (index < 0 || index >= selectionNames.Length) return;

            string name = selectionNames[index];
            selectionLabel.text = L.Get(ValueKey(name), name);
        }

        /// <summary>Clave de traducción para nombres de modo/mapa del carrusel.</summary>
        public static string ValueKey(string value)
        {
            var sb = new StringBuilder();
            bool lastWasSeparator = false;

            foreach (char c in (value ?? string.Empty).ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    lastWasSeparator = false;
                }
                else if (!lastWasSeparator)
                {
                    sb.Append('_');
                    lastWasSeparator = true;
                }
            }

            return "value." + sb.ToString().Trim('_');
        }

        private int Wrap(int d)
        {
            int half = cards.Length / 2;
            if (d > half) d -= cards.Length;
            if (d < -half) d += cards.Length;
            return d;
        }

        private void Snap()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;
                int d = Wrap(i - index);
                int abs = d < 0 ? -d : d;
                float s = abs == 0 ? 1f : (abs == 1 ? 0.82f : 0.66f);
                float a = abs == 0 ? 1f : (abs == 1 ? 0.4f : 0.12f);
                cards[i].anchoredPosition = new Vector2(d * spacing, cards[i].anchoredPosition.y);
                cards[i].localScale = new Vector3(s, s, 1f);
                if (groups[i] != null) groups[i].alpha = a;
                if (rings != null && i < rings.Length && rings[i] != null) rings[i].enabled = abs == 0;
            }
            UpdateSelectionLabel();
        }
    }
}
