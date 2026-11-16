using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Settings modal: volume sliders, video options and apply/reset actions.
    /// The sliders only drive their value labels for now; no engine settings are
    /// persisted yet.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider master;
        [SerializeField] private Slider music;
        [SerializeField] private Slider effects;
        [SerializeField] private TMP_Text masterValue;
        [SerializeField] private TMP_Text musicValue;
        [SerializeField] private TMP_Text effectsValue;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;

        private bool _wired;

        public void Init(Slider masterSlider, Slider musicSlider, Slider effectsSlider,
            TMP_Text masterLabel, TMP_Text musicLabel, TMP_Text effectsLabel,
            Button apply, Button reset)
        {
            master = masterSlider;
            music = musicSlider;
            effects = effectsSlider;
            masterValue = masterLabel;
            musicValue = musicLabel;
            effectsValue = effectsLabel;
            applyButton = apply;
            resetButton = reset;
        }

        private void Awake()
        {
            Wire();
            RefreshLabels();
        }

        private void Wire()
        {
            if (_wired) return;
            _wired = true;

            if (master != null) master.onValueChanged.AddListener(_ => RefreshLabels());
            if (music != null) music.onValueChanged.AddListener(_ => RefreshLabels());
            if (effects != null) effects.onValueChanged.AddListener(_ => RefreshLabels());
            if (applyButton != null) applyButton.onClick.AddListener(Apply);
            if (resetButton != null) resetButton.onClick.AddListener(ResetDefaults);

            var x = transform.Find("Window/XButton");
            if (x != null)
            {
                var b = x.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(Close);
            }
        }

        private void RefreshLabels()
        {
            if (masterValue != null && master != null) masterValue.text = Mathf.RoundToInt(master.value * 100f) + "%";
            if (musicValue != null && music != null) musicValue.text = Mathf.RoundToInt(music.value * 100f) + "%";
            if (effectsValue != null && effects != null) effectsValue.text = Mathf.RoundToInt(effects.value * 100f) + "%";
        }

        private void ResetDefaults()
        {
            if (master != null) master.value = 0.8f;
            if (music != null) music.value = 0.6f;
            if (effects != null) effects.value = 0.7f;
            RefreshLabels();
        }

        private void Apply()
        {
            Debug.Log($"[RATIX] Settings applied (master {master?.value:0.00}, music {music?.value:0.00}, effects {effects?.value:0.00})");
            Close();
        }

        public void Close() => gameObject.SetActive(false);
    }
}
