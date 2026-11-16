using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Store panel: category tabs, equipped column, item grid and a details pane.
    /// Card "View" buttons call Select(index); "Buy" buttons log a purchase until
    /// the economy backend exists.
    /// </summary>
    public class StorePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text detailsName;
        [SerializeField] private TMP_Text detailsRarity;
        [SerializeField] private TMP_Text detailsDescription;
        [SerializeField] private TMP_Text detailsStats;
        [SerializeField] private TMP_Text detailsPrice;
        [SerializeField] private TMP_Text status;
        [SerializeField] private Image detailsIcon;
        [SerializeField] private Button detailsBuy;

        [SerializeField] private string[] names;
        [SerializeField] private string[] rarities;
        [SerializeField] private string[] descriptions;
        [SerializeField] private string[] prices;
        [SerializeField] private string[] stats;
        [SerializeField] private Color[] rarityColors;

        private int selected = -1;
        private bool _wired;

        public void Init(TMP_Text nameLabel, TMP_Text rarityLabel, TMP_Text descriptionLabel,
            TMP_Text statsLabel, TMP_Text priceLabel, TMP_Text statusLabel, Image icon, Button buy,
            string[] itemNames, string[] itemRarities, string[] itemDescriptions,
            string[] itemPrices, string[] itemStats, Color[] itemRarityColors)
        {
            detailsName = nameLabel;
            detailsRarity = rarityLabel;
            detailsDescription = descriptionLabel;
            detailsStats = statsLabel;
            detailsPrice = priceLabel;
            status = statusLabel;
            detailsIcon = icon;
            detailsBuy = buy;
            names = itemNames;
            rarities = itemRarities;
            descriptions = itemDescriptions;
            prices = itemPrices;
            stats = itemStats;
            rarityColors = itemRarityColors;
        }

        private void Awake() => Wire();

        private void Wire()
        {
            if (_wired) return;
            _wired = true;
            if (detailsBuy != null) detailsBuy.onClick.AddListener(BuySelected);
            if (detailsBuy != null)
            {
                var label = detailsBuy.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = "BUY";
            }
        }

        public void Select(int index)
        {
            if (names == null || index < 0 || index >= names.Length) return;
            selected = index;

            if (detailsName != null) detailsName.text = names[index];
            if (detailsRarity != null)
            {
                detailsRarity.text = rarities != null && index < rarities.Length ? rarities[index] : string.Empty;
                if (rarityColors != null && index < rarityColors.Length) detailsRarity.color = rarityColors[index];
            }
            if (detailsDescription != null)
                detailsDescription.text = descriptions != null && index < descriptions.Length ? descriptions[index] : string.Empty;
            if (detailsStats != null)
                detailsStats.text = stats != null && index < stats.Length ? stats[index] : string.Empty;
            if (detailsPrice != null)
                detailsPrice.text = prices != null && index < prices.Length ? prices[index] + " coins" : string.Empty;
            if (detailsIcon != null && rarityColors != null && index < rarityColors.Length)
                detailsIcon.color = Color.Lerp(new Color(0.09f, 0.12f, 0.18f), rarityColors[index], 0.45f);
            if (status != null) status.text = string.Empty;
        }

        public void Buy(int index)
        {
            if (names == null || index < 0 || index >= names.Length) return;
            Debug.Log($"[RATIX] Store purchase: {names[index]} ({prices?[Mathf.Min(index, prices.Length - 1)]})");
            if (status != null) status.text = "Purchased: " + names[index];
        }

        public void BuySelected()
        {
            if (selected >= 0) Buy(selected);
        }

        public void Close() => gameObject.SetActive(false);
    }
}
