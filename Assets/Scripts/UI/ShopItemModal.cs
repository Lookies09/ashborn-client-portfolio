using DG.Tweening;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InventoryManager;
using static ItemEnums;
using static StatNameResolver;

/// <summary>
/// 상점 아이템 상세 모달.
/// </summary>
public class ShopItemModal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI TotalPriceText;
    [SerializeField] private Button qtyIncButton;
    [SerializeField] private Button qtyDescButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private RectTransform modalWindow;

    [SerializeField] private Color32 plusColor;
    [SerializeField] private Color32 minusColor;

    private ShopController _shopController;
    private ShopController.ShopItemEntry _entry;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        if (qtyIncButton != null)
            qtyIncButton.onClick.AddListener(OnIncreaseQuantityClicked);

        if (qtyDescButton != null)
            qtyDescButton.onClick.AddListener(OnDecreaseQuantityClicked);

    }

    /// <summary>
    /// 모달 오픈 + 데이터 바인딩
    /// </summary>
    public void Open(ShopController controller, ShopController.ShopItemEntry entry)
    {
        _shopController = controller;
        _entry = entry;
        gameObject.SetActive(true);

        modalWindow.localScale = Vector3.one * 0.8f;
        modalWindow.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

        Bind(entry);
    }

    /// <summary>
    /// 실제 UI 업데이트
    /// </summary>
    private void Bind(ShopController.ShopItemEntry entry)
    {
        if (entry == null)
        {
            Debug.LogError("Modal Bind 실패: entry null");
            return;
        }

        ItemDataSO data = entry.itemData;

        if (data != null)
        {
            if (iconImage != null)
            {
                iconImage.enabled = data.itemIcon != null;
                iconImage.sprite = data.itemIcon;
            }

            nameText.text = data.itemNameKR;
            priceText.text = $"{data.purchasePrice}G";
            descText.text = data.descriptionKR;
            statsText.text = BuildStatsText(data);
        }

        quantityText.text = "1";
        TotalPriceText.text = data != null ? data.purchasePrice.ToString() : "0";
    }

    private string BuildStatsText(ItemDataSO data)
    {
        if (data == null) return "";

        StringBuilder sb = new StringBuilder();
        string plusHex = "#" + ColorUtility.ToHtmlStringRGB(plusColor);
        string minusHex = "#" + ColorUtility.ToHtmlStringRGB(minusColor);

        // 1. 장비 아이템인 경우 (스탯 수정자 출력)
        if (data.itemType == ItemType.Weapon || data.itemType == ItemType.Armor || data.itemType == ItemType.Accessory)
        {
            if (data.baseModifiers == null || data.baseModifiers.Count == 0) return "능력치 변화 없음";

            foreach (var mod in data.baseModifiers)
            {
                string statName = StatNameResolver.Get(mod.statType);

                // 고정 수치(Flat) 처리
                if (mod.flatValue != 0)
                {
                    string colorTag = mod.flatValue > 0 ? plusHex : minusHex;
                    string sign = mod.flatValue > 0 ? "+" : "";
                    string formatted = StatValueFormatter.FormatStatValue(mod.statType, mod.flatValue);
                    sb.AppendLine($"{statName} <color={colorTag}>{sign}{formatted}</color>");
                }

                // 퍼센트 수치(Percent) 처리
                if (mod.percentValue != 0)
                {
                    string colorTag = mod.percentValue > 0 ? plusHex : minusHex;
                    string sign = mod.percentValue > 0 ? "+" : "";
                    string formatted = StatValueFormatter.FormatStatValue(mod.statType, mod.percentValue);
                    sb.AppendLine($"{statName} <color={colorTag}>{sign}{formatted}</color>");
                }
            }
        }
        // 2. 소모 아이템인 경우 (회복/버프 효과 출력)
        else if (data.itemType == ItemType.Consumable)
        {
            if (data.consumableEffects.Count == 0) return "특수 효과 없음";

            foreach (var effect in data.consumableEffects)
            {
                string effectName = ConsumableEffectTypeNameResolver.Get(effect.effectType);
                sb.AppendLine($"{effectName} <color={plusHex}>+{effect.value}</color>");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private void OnBuyClicked()
    {
        if (_entry == null || _shopController == null)
            return;

        if (!int.TryParse(quantityText.text, out int qty) || qty <= 0)
            qty = 1;

        bool result = _shopController.TryPurchaseItem(_entry, qty);

        if (result)
            Close();
    }

    public void Close()
    {
        modalWindow.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {            
            gameObject.SetActive(false);
        });
    }

    public void OnIncreaseQuantityClicked()
    {
        if (quantityText != null)
        {
            if (int.TryParse(quantityText.text, out int qty))
            {
                qty++;
                quantityText.text = qty.ToString();
                if (TotalPriceText != null && _entry != null)
                    TotalPriceText.text = (qty * _entry.itemData.purchasePrice).ToString();
            }
        }
    }

    public void OnDecreaseQuantityClicked()
    {
        if (quantityText != null)
        {
            if (int.TryParse(quantityText.text, out int qty))
            {
                qty--;
                quantityText.text = qty.ToString();
                if (TotalPriceText != null && _entry != null)
                    TotalPriceText.text = (qty * _entry.itemData.purchasePrice).ToString();
            }
        }
    }
}
