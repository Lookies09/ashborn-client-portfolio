using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static InventoryManager;

/// <summary>
/// 장착 아이템을 관리하는 매니저입니다.
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("장착 슬롯들")]
    [Tooltip("모든 장착 슬롯 (Inspector에서 할당)")]
    public List<EquipSlot> equipSlots = new List<EquipSlot>();

    private EquipItemHandler equipHandler;
    private PlayerContext ctx;

    // 장착된 아이템 데이터 저장
    private Dictionary<ItemEnums.ItemEquipSlot, InventoryManager.ItemInstance> equippedItems = new Dictionary<ItemEnums.ItemEquipSlot, InventoryManager.ItemInstance>();

    private void Awake()
    {
        Instance = this;

        PlayerContext playerContext = FindFirstObjectByType<Player>().GetContext();
        Init(playerContext);
    }

    private void Start()
    {
        PlayerDataManager.Instance.LoadSessionData();
    }

    public void Init(PlayerContext playerContext)
    {
        ctx = playerContext;
        equipHandler = new EquipItemHandler();

        // 모든 장착 슬롯에 EquipmentManager 참조 설정
        foreach (EquipSlot slot in equipSlots)
        {
            if (slot != null)
            {
                slot.Init(this);
            }
        }
        UpdateEquipmentUI();
    }

    /// <summary>
    /// 특정 장착 슬롯의 아이템을 가져옵니다.
    /// </summary>
    public InventoryManager.ItemInstance GetEquippedItem(ItemEnums.ItemEquipSlot type)
    {
        if (equippedItems.ContainsKey(type))
        {
            return equippedItems[type];
        }
        return null;
    }

    /// <summary>
    /// 특정 장착 슬롯에 아이템을 장착합니다.
    /// </summary>
    public void SetEquippedItem(InventoryManager.InventorySlotData itemData)
    {
        // 기존 장착 아이템 제거
        if (equippedItems.TryGetValue(itemData.item.itemDataSO.equipSlot, out var oldItem))
        {
            RemoveItemStats(oldItem);
            equippedItems.Remove(itemData.item.itemDataSO.equipSlot);
        }

        // 새 아이템 장착
        Equip(itemData.item.itemDataSO.equipSlot, itemData.item);
    }

    public void Equip(ItemEnums.ItemEquipSlot slotType, InventoryManager.ItemInstance item)
    {
        if (equippedItems.TryGetValue(slotType, out var oldItem))
        {
            RemoveItemStats(oldItem);
        }

        equippedItems[slotType] = item;

        ApplyItemStats(item);

        UpdateEquipmentUI();
    }

    public void Unequip(ItemEnums.ItemEquipSlot slot)
    {
        if (!equippedItems.TryGetValue(slot, out var item))
            return;

        RemoveItemStats(item);
        equippedItems.Remove(slot);
    }

    public void Swap(ItemEnums.ItemEquipSlot a, ItemEnums.ItemEquipSlot b)
    {
        equippedItems.TryGetValue(a, out var itemA);
        equippedItems.TryGetValue(b, out var itemB);

        equippedItems[a] = itemB;
        equippedItems[b] = itemA;
    }


    /// <summary>
    /// 모든 장착 슬롯의 UI를 업데이트합니다.
    /// </summary>
    public void UpdateEquipmentUI()
    {
        foreach (EquipSlot slot in equipSlots)
        {
            if (slot == null) continue;

            var item = GetEquippedItem(slot.slotType);
            slot.Refresh(item);
        }
    }


    /// <summary>
    /// 아이템의 스탯을 플레이어에 적용합니다.
    /// </summary>
    private void ApplyItemStats(ItemInstance item)
    {
        equipHandler.OnEquip(item, ctx);
    }

    /// <summary>
    /// 아이템의 스탯을 플레이어에서 제거합니다.
    /// </summary>
    private void RemoveItemStats(ItemInstance item)
    {
        equipHandler.OnUnequip(item, ctx);
    }

    public void UnequipToInventory(ItemEnums.ItemEquipSlot slotType, ItemSlot targetSlot)
    {
        if (!equippedItems.TryGetValue(slotType, out var item))
            return;

        var inventory = targetSlot.ownerInventory;
        if (inventory == null)
            return;

        // 인벤토리에 추가 시도
        if (!inventory.TryAddInstance(item))
            return; // 인벤 가득 참

        // 스탯 제거
        RemoveItemStats(item);

        // 장착 해제
        equippedItems.Remove(slotType);

        // UI 갱신
        UpdateEquipmentUI();
        inventory.UpdateInventoryUI();
    }

    public void ClearAllEquipments()
    {
        foreach (var item in equippedItems.Values)
        {
            if (item != null)
                RemoveItemStats(item);
        }

        equippedItems.Clear();

        UpdateEquipmentUI();
    }

    public Dictionary<ItemEnums.ItemEquipSlot, InventoryManager.ItemInstance> GetEquippedItems()
    {
        return new Dictionary<ItemEnums.ItemEquipSlot, InventoryManager.ItemInstance>(equippedItems);
    }

    public void EquipFromSave(ItemEnums.ItemEquipSlot slotType, ItemDataSO itemData)
    {
        if (itemData == null) return;

        InventoryManager.ItemInstance newInstance = new InventoryManager.ItemInstance(itemData, 1);

        if (equippedItems.TryGetValue(slotType, out var oldItem))
        {
            RemoveItemStats(oldItem);
        }
        Equip(slotType, newInstance);

        UpdateEquipmentUI();
    }
}

