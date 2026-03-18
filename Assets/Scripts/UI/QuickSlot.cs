using UnityEngine;
using UnityEngine.EventSystems;
using System;

[Tooltip("소비 아이템만 넣는 슬롯(아이템이 직접 들어가는게 아닌 참조만 하는 슬롯)")]
public class QuickSlot : ItemSlot
{
    public QuickSlotManager Manager;
    public Guid referencedInstanceId;

    public override void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag.GetComponent<ItemDragHandler>();
        if (drag == null) return;

        var sourceSlot = drag.sourceSlot;
        if (sourceSlot == null) return;

        if (sourceSlot is QuickSlot sourceQuick)
        {
            var temp = referencedInstanceId;
            referencedInstanceId = sourceQuick.referencedInstanceId;
            sourceQuick.referencedInstanceId = temp;

            Manager?.BindQuickSlot(slotIndex);
            Manager?.BindQuickSlot(sourceQuick.slotIndex);

            sourceQuick.RefreshFromInventory();
            RefreshFromInventory();

            drag.NotifyDropSuccessful();

            SoundManager.Instance.PlayQuickItemAddSound();
            return;
        }

        var data = sourceSlot.ownerInventory.GetItem(sourceSlot.slotIndex);
        if (data == null || data.IsEmpty) return;

        var instance = sourceSlot.currentSlotData;
        if (instance == null) return;       

        if (sourceSlot.ownerInventory != ownerInventory) return;
        if (data.item.itemDataSO.equipSlot != ItemEnums.ItemEquipSlot.Consuming) return;

        if (Manager.IsItemAlreadyBound(instance.instanceId))
        {
            Manager.ClearBoundItem(instance.instanceId);
        }

        // 참조 바인딩
        referencedInstanceId = instance.instanceId;

        SoundManager.Instance.PlayQuickItemAddSound();
        Manager?.BindQuickSlot(slotIndex);
        RefreshFromInventory();
        drag.NotifyDropSuccessful();
    }

    public void Clear()
    {
        referencedInstanceId = Guid.Empty;
        Refresh(null);
        ItemUI.Clear();
    }

    public void ClearSlotUI()
    {
        Refresh(null);
    }

    public void RefreshFromInventory()
    {
        if (ownerInventory == null || referencedInstanceId == Guid.Empty)
        {
            Clear();
            return;
        }

        var instance = ownerInventory.GetInstanceById(referencedInstanceId);

        if (instance == null || instance.quantity <= 0)
        {
            Clear();
            return;
        }

        ItemUI.Refresh(instance);
    }

    public void SetReference(Guid instanceId)
    {
        referencedInstanceId = instanceId;
        RefreshFromInventory(); 
    }
}

