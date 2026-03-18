using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlotUI : BaseItemUI, IPointerClickHandler
{
    public override void Refresh(InventoryManager.ItemInstance slotData)
    {
        if (slotData == null || slotData.itemDataSO == null)
        {
            Clear();
            return;
        }

        ItemIcon.sprite = slotData.itemDataSO.itemIcon;
        ItemIcon.enabled = true;

        if (slotData.quantity > 1)
        {
            QuantityText.text = slotData.quantity.ToString();
            QuantityText.gameObject.SetActive(true);
        }
        else
        {
            QuantityText.gameObject.SetActive(false);
        }

        RarityImage.color = GetRarityColor(slotData.itemDataSO.rarity);
        RarityImage.enabled = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (Time.unscaledTime - _lastClickTime < DoubleClickThreshold)
        {
            HandleDoubleClick();
        }

        _lastClickTime = Time.unscaledTime;
    }

    public void HandleDoubleClick()
    {
        if (_parentSlot is QuickSlot slot)
        {
            slot.ClearSlotUI();
        }
    }
}

