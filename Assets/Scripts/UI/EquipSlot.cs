using UnityEngine;
using UnityEngine.EventSystems;

[Tooltip("장비 아이템 넣는 슬롯(아이템이 직접 들어가는 슬롯)")]
public class EquipSlot : ItemSlot
{
    [Header("장착 슬롯 설정")]
    [Tooltip("이 슬롯이 받을 수 있는 아이템 타입")]
    public ItemEnums.ItemEquipSlot slotType;

    [SerializeField] private EquipmentManager equipmentManager;

    public void Init(EquipmentManager equipmentManager)
    {
        this.equipmentManager = equipmentManager;
    }

    public override void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag?.GetComponent<ItemDragHandler>();
        if (drag == null) return;

        // 인벤토리 → 장착
        if (drag.sourceSlot is ItemSlot invSlot && !(drag.sourceSlot is EquipSlot))
        {
            TryEquipFromInventory(invSlot, drag);
            SoundManager.Instance.PlayEquipSound();
            return;
        }

        // 장비 → 장비
        if (drag.sourceSlot is EquipSlot equipSlot)
        {
            TrySwapEquip(equipSlot, drag);
            return;
        }
    }

    private void TryEquipFromInventory(ItemSlot sourceSlot, ItemDragHandler drag)
    {
        var inventory = sourceSlot.ownerInventory;
        if (inventory == null) return;

        var slotData = inventory.GetItem(sourceSlot.slotIndex);
        if (slotData == null || slotData.IsEmpty) return;

        var item = slotData.item;

        if (item.itemDataSO.equipSlot != slotType)
            return;

        // 기존 장비 회수
        var oldItem = equipmentManager.GetEquippedItem(slotType);
        if (oldItem != null)
        {
            if (!inventory.TryAddInstance(oldItem))
                return;
        }

        // 인벤에서 제거
        inventory.RemoveInstance(item);

        // 장착
        equipmentManager.Equip(slotType, item);

        drag.NotifyDropSuccessful();
        equipmentManager.UpdateEquipmentUI();
        inventory.UpdateInventoryUI();
    }

    private void TrySwapEquip(EquipSlot source, ItemDragHandler drag)
    {
        if (source.slotType == slotType)
            return;

        var sourceItem = equipmentManager.GetEquippedItem(source.slotType);
        var targetItem = equipmentManager.GetEquippedItem(slotType);

        if (sourceItem != null && sourceItem.itemDataSO.equipSlot != slotType)
            return;

        if (targetItem != null && targetItem.itemDataSO.equipSlot != source.slotType)
            return;

        equipmentManager.Swap(source.slotType, slotType);

        drag.NotifyDropSuccessful();
        equipmentManager.UpdateEquipmentUI();
    }

    public void RequestUnequip(ItemSlot targetSlot)
    {
        SoundManager.Instance.PlayUnEquipSound();
        equipmentManager.UnequipToInventory(slotType, targetSlot);
    }


    // --- UI 업데이트 오버라이드 ---

    /// <summary>
    /// 장착 슬롯의 UI를 업데이트합니다.
    /// </summary>
    public override void Refresh(InventoryManager.ItemInstance slotData)
    {        
            // 부모 클래스의 Refresh 호출
            base.Refresh(slotData);

        // 장착 슬롯 특수 처리 (예: 테두리 색상 변경 등)
        // 필요시 여기에 추가 로직 구현
    }
}

