using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Small two-column modal used by the hall to kick a player or invite a friend.
    /// Clicking a row selects it; the action button confirms and closes.
    /// </summary>
    public class HallActionModal : MonoBehaviour
    {
        [SerializeField] private string actionName = "KICK";
        [SerializeField] private TMP_Text selectedLabel;
        [SerializeField] private Button actionButton;

        private static readonly Color RowEven = new Color(0.067f, 0.106f, 0.169f);
        private static readonly Color RowOdd = new Color(0.086f, 0.133f, 0.212f);
        private static readonly Color RowSelected = new Color(0.13f, 0.30f, 0.24f);

        private Button _selected;
        private bool _wired;

        public void Init(string action, Button button, TMP_Text selected)
        {
            actionName = action;
            actionButton = button;
            selectedLabel = selected;
        }

        private void Awake() => EnsureWired();

        public void Open()
        {
            EnsureWired();
            var rt = (RectTransform)transform;
            rt.anchoredPosition = Vector2.zero;
            gameObject.SetActive(true);
            rt.SetAsLastSibling();
            ClearSelection();
        }

        public void Close() => gameObject.SetActive(false);

        private void EnsureWired()
        {
            if (_wired) return;
            _wired = true;

            if (actionButton != null) actionButton.onClick.AddListener(Execute);

            var x = transform.Find("Window/XButton");
            if (x != null)
            {
                var b = x.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(Close);
            }

            foreach (var b in GetComponentsInChildren<Button>(true))
            {
                if (!b.name.StartsWith("Row")) continue;
                var captured = b;
                b.onClick.AddListener(() => Select(captured));
            }
        }

        private void Select(Button row)
        {
            if (_selected != null) Paint(_selected, false);
            _selected = row;
            Paint(row, true);

            if (selectedLabel != null)
            {
                var t = row.GetComponentInChildren<TMP_Text>();
                selectedLabel.text = "Selected: " + (t != null ? t.text : "?");
            }
        }

        private void ClearSelection()
        {
            _selected = null;
            foreach (var b in GetComponentsInChildren<Button>(true))
                if (b.name.StartsWith("Row")) Paint(b, false);
            if (selectedLabel != null) selectedLabel.text = "Selected: none";
        }

        private static void Paint(Button row, bool selected)
        {
            var img = row.GetComponent<Image>();
            if (img == null) return;
            img.color = selected ? RowSelected : (row.transform.GetSiblingIndex() % 2 == 0 ? RowEven : RowOdd);
        }

        private void Execute()
        {
            string name = _selected != null ? _selected.GetComponentInChildren<TMP_Text>().text : "none";
            Debug.Log($"[RATIX] {actionName}: {name}");
            Close();
        }
    }
}
