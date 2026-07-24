using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseItemUI : MonoBehaviour
{
    protected ItemDragHandler _dragHandler;
    protected ItemSlot _parentSlot;
    protected CanvasGroup _canvasGroup;
    protected float _lastClickTime;
    protected const float DoubleClickThreshold = 0.25f;

    public Image ItemIcon;
    public TextMeshProUGUI QuantityText;
    public Image RarityImage;
    public TextMeshProUGUI NameText;

    public virtual void Awake()
    {
        _dragHandler = GetComponent<ItemDragHandler>();
        _parentSlot = GetComponentInParent<ItemSlot>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_dragHandler == null)
        {
            Debug.LogWarning("ItemUI: ItemDragHandler 컴포넌트를 찾을 수 없습니다.");
        }
    }

    protected virtual Color GetRarityColor(ItemEnums.ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemEnums.ItemRarity.Common: return new Color(1f, 1f, 1f, 0.3f);
            case ItemEnums.ItemRarity.Uncommon: return new Color(0f, 1f, 0f, 0.3f);
            case ItemEnums.ItemRarity.Rare: return new Color(0f, 0f, 1f, 0.3f);
            case ItemEnums.ItemRarity.Epic: return new Color(1f, 0f, 1f, 0.3f);
            case ItemEnums.ItemRarity.Legendary: return new Color(1f, 0.5f, 0f, 0.3f);
            default: return new Color(1f, 1f, 1f, 0.3f);
        }
    }

    public virtual void Refresh(InventoryManager.ItemInstance slotData)
    {
        if (slotData == null || slotData.itemDataSO == null)
        {
            Clear();
            return;
        }

        ItemIcon.sprite = slotData.itemDataSO.itemIcon;
        ItemIcon.enabled = true;

        QuantityText.text = slotData.quantity > 1 ? slotData.quantity.ToString() : "";
        QuantityText.gameObject.SetActive(slotData.quantity > 1);

        RarityImage.color = GetRarityColor(slotData.itemDataSO.rarity);
        RarityImage.enabled = true;

        NameText.text = slotData.itemDataSO.name;
        NameText.gameObject.SetActive(true);
    }

    public virtual void Clear()
    {
        ItemIcon.sprite = null;
        ItemIcon.enabled = false;
        QuantityText.gameObject.SetActive(false);
        RarityImage.enabled = false;
        NameText.gameObject.SetActive(false);
    }
}
