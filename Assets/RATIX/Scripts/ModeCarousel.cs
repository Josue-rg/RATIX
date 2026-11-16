using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RATIX
{
    /// <summary>
    /// Simple carousel: cycles through a list of sub-mode names with previous/next
    /// buttons. The list and label are configured from the Inspector.
    /// </summary>
    public class ModeCarousel : MonoBehaviour
    {
        [SerializeField] private string[] items;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        private int index;

        private void Start()
        {
            if (prevButton != null) prevButton.onClick.AddListener(Prev);
            if (nextButton != null) nextButton.onClick.AddListener(Next);
            UpdateLabel();
        }

        public void Next()
        {
            if (items == null || items.Length == 0) return;
            index = (index + 1) % items.Length;
            UpdateLabel();
        }

        public void Prev()
        {
            if (items == null || items.Length == 0) return;
            index = (index - 1 + items.Length) % items.Length;
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (label != null && items != null && items.Length > 0)
                label.text = items[index];
        }
    }
}
