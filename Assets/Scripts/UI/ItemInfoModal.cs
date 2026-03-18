using DG.Tweening;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InventoryManager;
using static ItemEnums;
using static StatNameResolver;
using static UnityEngine.EventSystems.EventTrigger;

public class ItemInfoModal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button closeBarrierButton;

    [SerializeField] private Color32 plusColor;
    [SerializeField] private Color32 minusColor;

    [SerializeField] private RectTransform modalWindow;
    private ItemDataSO _currentItemData;

    private void Awake()
    {
        if (closeBarrierButton != null)
            closeBarrierButton.onClick.AddListener(Close);
    }

    public void Open(ItemDataSO item)
    {
        gameObject.SetActive(true);

        modalWindow.localScale = Vector3.one * 0.8f;
        modalWindow.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

        Bind(item);
    }

    private void Bind(ItemDataSO item)
    {
        if (item == null)
        {
            Debug.LogError("Modal Bind 실패: item null");
            return;
        }

        if (item != null)
        {
            if (iconImage != null)
            {
                iconImage.enabled = item.itemIcon != null;
                iconImage.sprite = item.itemIcon;
            }

            nameText.text = item.itemNameKR;
            descText.text = item.descriptionKR;
            statsText.text = BuildStatsText(item);
        }

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

    public void Close()
    {
        modalWindow.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
}
