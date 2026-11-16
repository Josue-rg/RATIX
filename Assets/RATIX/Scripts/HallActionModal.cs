using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Small two-column modal used by the hall to kick players or invite friends.
    /// Clicking a row toggles it: se pueden seleccionar varias filas y el botón
    /// de acción actúa sobre todas las elegidas.
    /// </summary>
    public class HallActionModal : MonoBehaviour
    {
        [SerializeField] private string actionName = "KICK";
        [SerializeField] private TMP_Text selectedLabel;
        [SerializeField] private Button actionButton;

        private static readonly Color RowEven = new Color(0.067f, 0.106f, 0.169f);
        private static readonly Color RowOdd = new Color(0.086f, 0.133f, 0.212f);
        private static readonly Color RowSelected = new Color(0.13f, 0.30f, 0.24f);

        private readonly List<Button> _selected = new List<Button>();
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
                b.onClick.AddListener(() => Toggle(captured));
            }
        }

        /// <summary>Marca o desmarca una fila. Se pueden elegir varias.</summary>
        private void Toggle(Button row)
        {
            if (_selected.Contains(row))
            {
                _selected.Remove(row);
                Paint(row, false);
            }
            else
            {
                _selected.Add(row);
                Paint(row, true);
            }

            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (selectedLabel == null) return;

            if (_selected.Count == 0)
            {
                selectedLabel.text = "Selected: none";
                return;
            }

            if (_selected.Count == 1)
            {
                selectedLabel.text = "Selected: " + ReadName(_selected[0]);
                return;
            }

            string noun = IsInvite ? "friends" : "players";
            selectedLabel.text = "Selected: " + _selected.Count + " " + noun;
        }

        private bool IsInvite =>
            !string.IsNullOrEmpty(actionName) &&
            actionName.ToUpperInvariant().Contains("INVITE");

        private string ReadName(Button row)
        {
            var text = row.GetComponentInChildren<TMP_Text>();
            return text != null ? text.text : "?";
        }

        private void ClearSelection()
        {
            _selected.Clear();
            RepaintAll();
            UpdateLabel();
        }

        private void RepaintAll()
        {
            foreach (var b in GetComponentsInChildren<Button>(true))
                if (b.name.StartsWith("Row")) Paint(b, false);
        }

        private static void Paint(Button row, bool selected)
        {
            var img = row.GetComponent<Image>();
            if (img == null) return;
            img.color = selected ? RowSelected : (row.transform.GetSiblingIndex() % 2 == 0 ? RowEven : RowOdd);
        }

        private void Execute()
        {
            if (_selected.Count == 0)
            {
                Debug.Log($"[RATIX] {actionName}: none");
                Close();
                return;
            }

            var names = new StringBuilder();
            for (int i = 0; i < _selected.Count; i++)
            {
                if (i > 0) names.Append(", ");
                names.Append(ReadName(_selected[i]));
            }

            Debug.Log($"[RATIX] {actionName} ({_selected.Count}): {names}");
            Close();
        }
    }
}
