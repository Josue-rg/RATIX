using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Small custom dropdown used by the room settings (3 options, no scroll needed).
    /// Clicking the box opens a list; picking an option updates the caption.
    /// </summary>
    public class MiniDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Text caption;
        [SerializeField] private GameObject list;
        [SerializeField] private Button[] options;
        [SerializeField] private string[] values;
        [SerializeField] private int value;

        public int Value => value;

        private void Awake() => Wire();

        public void Init(TMP_Text label, GameObject listRoot, Button[] optionButtons, string[] optionValues)
        {
            caption = label;
            list = listRoot;
            options = optionButtons;
            values = optionValues;
            Apply(Value);
        }

        private void Wire()
        {
            var root = GetComponent<Button>();
            if (root != null)
            {
                root.onClick.RemoveListener(Toggle);
                root.onClick.AddListener(Toggle);
            }

            if (options != null)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i] == null) continue;
                    int index = i;
                    options[i].onClick.RemoveAllListeners();
                    options[i].onClick.AddListener(() => Select(index));
                }
            }

            if (list != null) list.SetActive(false);
            Apply(Value);
        }

        public void Toggle()
        {
            if (list == null) return;
            bool open = !list.activeSelf;
            if (open) transform.SetAsLastSibling(); // keep the popup above the other fields
            list.SetActive(open);
        }

        public void Select(int index)
        {
            value = index;
            Apply(index);
            if (list != null) list.SetActive(false);
        }

        private void Apply(int index)
        {
            if (caption != null && values != null && index >= 0 && index < values.Length)
                caption.text = values[index];
        }
    }
}
