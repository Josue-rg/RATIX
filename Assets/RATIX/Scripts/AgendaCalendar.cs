using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Calendario real: toma el mes actual del sistema, coloca los días en la
    /// rejilla (7 columnas) y marca el día de hoy. Se adapta a 4, 5 o 6 semanas.
    /// </summary>
    public class AgendaCalendar : MonoBehaviour
    {
        [SerializeField] private TMP_Text monthLabel;
        [SerializeField] private RectTransform grid;
        [SerializeField] private Color dayColor = new Color(0.067f, 0.106f, 0.169f, 1f);
        [SerializeField] private Color todayColor = new Color(0.122f, 0.38f, 0.50f, 1f);
        [SerializeField] private Color dayTextColor = new Color(0.568f, 0.635f, 0.71f, 1f);
        [SerializeField] private Color todayTextColor = new Color(0.95f, 0.97f, 1f, 1f);

        private const float CellWidth = 90f;
        private const float CellHeight = 90f;
        private const float StepX = 110f;
        private const float StepY = 96f;

        private static readonly string[] MonthsEn =
        {
            "JANUARY", "FEBRUARY", "MARCH", "APRIL", "MAY", "JUNE",
            "JULY", "AUGUST", "SEPTEMBER", "OCTOBER", "NOVEMBER", "DECEMBER"
        };

        private static readonly string[] MonthsEs =
        {
            "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO",
            "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE"
        };

        private static readonly string[] DaysEn = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
        private static readonly string[] DaysEs = { "DOM", "LUN", "MAR", "MIÉ", "JUE", "VIE", "SÁB" };

        private void Awake() => Resolve();

        private void OnEnable()
        {
            L.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            L.LanguageChanged -= Refresh;
        }

        private void Resolve()
        {
            if (monthLabel == null)
            {
                var t = transform.Find("Window/Month");
                if (t != null) monthLabel = t.GetComponent<TMP_Text>();
            }

            if (grid == null)
            {
                var t = transform.Find("Window/Grid");
                if (t != null) grid = t as RectTransform;
            }
        }

        /// <summary>Redibuja el calendario con el mes y día actuales.</summary>
        public void Refresh()
        {
            Resolve();
            if (grid == null) return;

            var today = System.DateTime.Now;
            var first = new System.DateTime(today.Year, today.Month, 1);

            int offset = ((int)first.DayOfWeek + 6) % 7; // lunes primero
            int daysInMonth = System.DateTime.DaysInMonth(today.Year, today.Month);
            int rows = Mathf.CeilToInt((offset + daysInMonth) / 7f);
            int visibleCells = rows * 7;

            bool spanish = L.Current == "es";
            if (monthLabel != null)
            {
                string month = (spanish ? MonthsEs : MonthsEn)[today.Month - 1];
                monthLabel.text = month + " " + today.Year;
            }

            var cells = grid.GetComponentsInChildren<Transform>(true);
            int index = 0;

            foreach (var cell in cells)
            {
                if (cell == null || cell == grid) continue;
                if (!cell.name.StartsWith("Day")) continue;

                var rect = cell as RectTransform;
                if (rect == null) continue;

                bool visible = index < visibleCells;
                cell.gameObject.SetActive(visible);

                if (visible)
                {
                    int column = index % 7;
                    int row = index / 7;
                    rect.anchoredPosition = new Vector2(column * StepX, grid.rect.height - CellHeight - row * StepY);
                    rect.sizeDelta = new Vector2(CellWidth, CellHeight);

                    int dayNumber = index - offset + 1;
                    bool hasDay = dayNumber >= 1 && dayNumber <= daysInMonth;
                    bool isToday = hasDay && dayNumber == today.Day;

                    var label = cell.GetComponentInChildren<TMP_Text>(true);
                    if (label != null)
                    {
                        label.text = hasDay
                            ? (spanish ? DaysEs : DaysEn)[(int)first.AddDays(dayNumber - 1).DayOfWeek] + " " + dayNumber
                            : string.Empty;
                        label.color = isToday ? todayTextColor : dayTextColor;
                        label.fontStyle = isToday ? FontStyles.Bold : FontStyles.Normal;
                        label.alignment = TextAlignmentOptions.Center;
                    }

                    var image = cell.GetComponent<Image>();
                    if (image != null) image.color = isToday ? todayColor : dayColor;
                }

                index++;
            }
        }
    }
}
