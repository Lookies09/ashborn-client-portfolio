using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemEnums;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager PlayerInventory { get; private set; }

    [Header("Inventory Settings")]
    public int inventorySize = 60;
    [SerializeField] private bool isChestInventory = false;

    [Header("UI Settings")]
    public Transform slotsParent;
    public GameObject slotPrefab;

    [Header("Item Data")]
    public ItemDataListSO itemDataList;
    public ItemDropTableSO dropTable;

    [SerializeField] private InventorySlotData[] inventoryData;
    private bool hasBoundSlots = false;
    public List<ItemSlot> slots = new List<ItemSlot>();

    public bool IsFull => inventoryData.All(slot => slot != null && !slot.IsEmpty);

    public event Action OnInventoryChanged;
    public event Action<Guid> OnItemRemoved;
    public event Action<Guid> OnItemMoved;

    [Serializable]
    public class ItemInstance
    {
        public Guid instanceId;
        public ItemDataSO itemDataSO;
        public int quantity;

        public ItemInstance(ItemDataSO data, int quantity = 1)
        {
            instanceId = Guid.NewGuid();
            this.itemDataSO = data;
            this.quantity = quantity;
        }
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public ItemInstance item;
        public bool IsEmpty => item == null || item.quantity <= 0;
    }

    private void Awake()
    {
        if (!isChestInventory)
        {
            PlayerInventory = this;
        }

        OnInventoryChanged += UpdateInventoryUI;
    }

    private void Start()
    {
        EnsureInventoryData();

        if (!isChestInventory)
        { 
            if (slotsParent == null || slotPrefab == null)
            {
                Debug.LogWarning($"{name}: Inventory UI is not assigned.");
                return;
            }
            InitializeOrRebindSlots(slotsParent);
        }
        else
        {            
            ApplyRandomItems();
        }
    }

    public void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }


    private void ApplyRandomItems()
    {
        if (itemDataList == null || itemDataList.items.Count == 0 || dropTable == null)
            return;

        int dropCount = UnityEngine.Random.Range(dropTable.minDropCount, dropTable.maxDropCount + 1);

        for (int i = 0; i < dropCount; i++)
        {
            ItemDataSO randomItem = dropTable.GetRandomItemByWeightedChance(itemDataList);
            if (randomItem == null) continue;

            int qty = Mathf.Clamp(UnityEngine.Random.Range(1, randomItem.maxStack + 1), 1, randomItem.maxStack);
            AddItem(randomItem, qty);

            if (IsFull) break;
        }
    }    


    private void EnsureInventoryData()
    {
        if (inventoryData == null || inventoryData.Length != inventorySize)
        {
            inventoryData = new InventorySlotData[inventorySize];
        }
    }

    public void InitializeOrRebindSlots(Transform parent)
    {
        if (parent == null || slotPrefab == null)
        {
            Debug.LogWarning($"{name}: Slot UI not assigned.");
            return;
        }

        EnsureInventoryData();
        slotsParent = parent;
        int childCount = slotsParent.childCount;
        slots = new List<ItemSlot>(inventorySize);

        for (int i = 0; i < inventorySize; i++)
        {
            ItemSlot slotScript = null;

            if (i < childCount)
                slotScript = slotsParent.GetChild(i).GetComponent<ItemSlot>();
            else
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsParent);
                slotScript = slotObj.GetComponent<ItemSlot>();
                childCount++;
            }

            if (slotScript == null) continue;

            slots.Add(slotScript);
            slotScript.gameObject.SetActive(true);
            slotScript.Init(this, i);
            var slotData = inventoryData[i];
            slotScript.Refresh(slotData?.item);

        }

        // Disable extra slots
        for (int i = inventorySize; i < childCount; i++)
        {
            slotsParent.GetChild(i).gameObject.SetActive(false);
        }

        hasBoundSlots = true;
        NotifyChanged();
    }

    public void SwapItems(int sourceIndex, int targetIndex)
    {
        if (!IsValidIndex(sourceIndex) || !IsValidIndex(targetIndex))
        {
            Debug.LogError("Invalid index for SwapItems.");
            return;
        }

        var temp = inventoryData[sourceIndex];
        inventoryData[sourceIndex] = inventoryData[targetIndex];
        inventoryData[targetIndex] = temp;

        NotifyChanged();
    }

    public bool TransferItemTo(InventoryManager targetManager, int sourceIndex, int targetIndex)
    {
        if (targetManager == null || targetManager == this) return false;
        if (!IsValidIndex(sourceIndex) || !targetManager.IsValidIndex(targetIndex)) return false;

        InventorySlotData slotData = inventoryData[sourceIndex];
        if (slotData == null || slotData.IsEmpty) return false;

        InventorySlotData targetItem = targetManager.inventoryData[targetIndex];

        if (targetItem != null && !targetItem.IsEmpty)
        {
            // Try stacking
            bool canStack = targetItem.item.itemDataSO != null && slotData.item.itemDataSO != null &&
                            targetItem.item.itemDataSO.itemId == slotData.item.itemDataSO.itemId &&
                            targetItem.item.itemDataSO.maxStack > 1;
            
            if (canStack)
            {
                Debug.Log("갯수 증가");
                int moveAmount = Mathf.Min(slotData.item.quantity, targetItem.item.itemDataSO.maxStack - targetItem.item.quantity);
                targetItem.item.quantity += moveAmount;
                slotData.item.quantity -= moveAmount;

                if (slotData.item.quantity <= 0)
                {
                    inventoryData[sourceIndex] = null;
                    OnItemMoved?.Invoke(slotData.item.instanceId);
                }

                NotifyChanged();
                targetManager.NotifyChanged();
                return true;
            }

            // Swap if not stackable
            inventoryData[sourceIndex] = targetItem;
            targetManager.inventoryData[targetIndex] = slotData;
        }
        else
        {
            targetManager.inventoryData[targetIndex] = slotData;
            inventoryData[sourceIndex] = null;
        }

        var sourceId = slotData.item.instanceId;
        if (targetItem != null && targetItem.item != null)
        {
            var targetId = targetItem.item.instanceId;
            targetManager.OnItemMoved?.Invoke(targetId);
        }
        OnItemMoved?.Invoke(sourceId);

        NotifyChanged();
        targetManager.NotifyChanged();
        return true;
    }

    private bool IsValidIndex(int index) => index >= 0 && index < inventorySize;

    [ContextMenu("Compact Inventory")]
    public void CompactInventory()
    {
        var nonNullItems = inventoryData.Where(item => item != null && !item.IsEmpty).ToList();
        inventoryData = new InventorySlotData[inventorySize];

        for (int i = 0; i < nonNullItems.Count && i < inventorySize; i++)
        {
            inventoryData[i] = nonNullItems[i];
        }

        NotifyChanged();
    }

    public bool AddItem(ItemDataSO itemDataSO, int quantity = 1)
    {
        EnsureInventoryData();
        if (itemDataSO == null) return false;

        // Try stacking
        if (itemDataSO.maxStack > 1)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                if (inventoryData[i] != null && !inventoryData[i].IsEmpty &&
                    inventoryData[i].item.itemDataSO.itemId == itemDataSO.itemId)
                {

                    Debug.Log("갯수 증가");
                    int addAmount = Mathf.Min(
                        quantity,
                        itemDataSO.maxStack - inventoryData[i].item.quantity
                    );

                    inventoryData[i].item.quantity += addAmount;
                    quantity -= addAmount;

                    if (quantity <= 0)
                    {
                        NotifyChanged();
                        return true;
                    }
                }
            }
        }

        // Add to empty slot
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryData[i] == null || inventoryData[i].IsEmpty)
            {
                inventoryData[i] = new InventorySlotData
                {
                    item = new ItemInstance(itemDataSO, quantity) // 여기서 ID 생성
                };
                NotifyChanged();
                return true;
            }
        }

        Debug.LogWarning("Inventory is full!");
        return false;
    }

    public void UpdateInventoryUI()
    {
        if (!hasBoundSlots || slotsParent == null) return;

        for (int i = 0; i < inventorySize; i++)
        {
            var slotData = inventoryData[i];
            slots[i].Refresh(slotData?.item);
        }
    }

    public void UpdateSlotUI(int index)
    {
        if (!hasBoundSlots) return;
        if (!IsValidIndex(index) || index >= slots.Count) return;
        var slotData = inventoryData[index];
        slots[index].Refresh(slotData?.item);
    }

    public bool TrySetItemAt(int index, ItemDataSO itemData, int quantity)
    {
        if (index < 0 || index >= inventoryData.Length)
            return false;

        var slot = inventoryData[index];

        // 비어있지 않으면 실패
        if (slot != null && !slot.IsEmpty)
            return false;

        inventoryData[index] = new InventorySlotData
        {
            item = new ItemInstance(itemData, quantity) 
        };
        NotifyChanged();
        return true;
    }

    public int FindFirstEmptySlot()
    {
        for (int i = 0; i < inventoryData.Length; i++)
        {
            if (inventoryData[i] == null || inventoryData[i].IsEmpty)
                return i;
        }
        return -1;
    }


    public InventorySlotData GetItem(int index)
    {
        EnsureInventoryData();
        return IsValidIndex(index) ? inventoryData[index] : null;
    }

    public void RemoveItem(int index, int quantity = 1)
    {
        EnsureInventoryData();
        if (!IsValidIndex(index)) return;

        var slotData = inventoryData[index];
        if (slotData == null || slotData.IsEmpty) return;

        int removeAmount = Mathf.Min(quantity, slotData.item.quantity);
        slotData.item.quantity -= removeAmount;

        if (slotData.item.quantity <= 0)
        {
            var removedId = slotData.item.instanceId;
            inventoryData[index] = null;
            OnItemRemoved?.Invoke(removedId);
        }

        NotifyChanged();

    }

    public void RemoveItemByGuid(Guid instanceId, int quantity = 1)
    {
        int index = GetSlotIndexByInstanceId(instanceId);

        if (index != -1)
        {
            RemoveItem(index, quantity);
        }
    }

    public void TransferAllItemsTo(InventoryManager targetInventory)
    {
        for (int i = 0; i < inventoryData.Length; i++)
        {
            var slotData = inventoryData[i];
            if (slotData == null || slotData.IsEmpty || slotData.item == null) continue;

            if (targetInventory.TryAddInstance(slotData.item))
            {
                inventoryData[i] = null;
                OnItemMoved?.Invoke(slotData.item.instanceId);
            }
            else
            {
                break;
            }

            if (targetInventory.IsFull)
                break;
        }

        NotifyChanged();
        targetInventory.NotifyChanged();
    }

    public void ForceAddInstance(ItemInstance instance)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryData[i] == null || inventoryData[i].IsEmpty)
            {
                inventoryData[i] = new InventorySlotData { item = instance };
                return;
            }
        }
    }

    public bool TransferToFirstEmpty(InventoryManager target, int sourceIndex)
    {
        if (!IsValidIndex(sourceIndex))
            return false;

        var sourceItem = inventoryData[sourceIndex];
        if (sourceItem == null || sourceItem.IsEmpty)
            return false;

        if (sourceItem.item.itemDataSO.maxStack > 1)
        {
            for (int i = 0; i < target.inventorySize; i++)
            {
                var targetItem = target.inventoryData[i];
                if (targetItem == null || targetItem.IsEmpty)
                    continue;

                if (targetItem.item.itemDataSO.itemId == sourceItem.item.itemDataSO.itemId &&
                    targetItem.item.quantity < targetItem.item.itemDataSO.maxStack)
                {
                    return TransferItemTo(target, sourceIndex, i);
                }
            }
        }

        for (int i = 0; i < target.inventorySize; i++)
        {
            if (target.IsSlotEmpty(i))
            {
                return TransferItemTo(target, sourceIndex, i);
            }
        }

        return false;
    }


    public bool IsSlotEmpty(int index)
    {
        if (index < 0 || index >= inventoryData.Length)
            return true;

        return inventoryData[index] == null || inventoryData[index].IsEmpty;
    }

    public static bool SwapBetweenInventories(
    InventoryManager fromInv, int fromIndex,
    InventoryManager toInv, int toIndex)
    {
        if (fromInv == null || toInv == null)
            return false;

        if (!fromInv.IsValidIndex(fromIndex) || !toInv.IsValidIndex(toIndex))
            return false;

        var fromData = fromInv.inventoryData[fromIndex];
        var toData = toInv.inventoryData[toIndex];

        if (fromData == null || fromData.IsEmpty)
            return false;

        // 스왑
        fromInv.inventoryData[fromIndex] = toData;
        toInv.inventoryData[toIndex] = fromData;

        if (fromData?.item != null)
            fromInv.OnItemMoved?.Invoke(fromData.item.instanceId);

        if (toData?.item != null)
            toInv.OnItemMoved?.Invoke(toData.item.instanceId);               

        // UI 갱신
        fromInv.UpdateSlotUI(fromIndex);
        toInv.UpdateSlotUI(toIndex);

        return true;
    }

    public bool TryAddInstance(ItemInstance instance)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryData[i] == null || inventoryData[i].IsEmpty)
            {
                inventoryData[i] = new InventorySlotData { item = instance };
                return true;
            }
        }
        return false;
    }

    public void RemoveInstance(ItemInstance instance)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryData[i]?.item == instance)
            {
                inventoryData[i] = null;
                OnItemRemoved?.Invoke(instance.instanceId);
                return;
            }
        }
    }

    public ItemInstance GetInstanceById(Guid instanceId)
    {
        if (instanceId == Guid.Empty)
            return null;

        for (int i = 0; i < inventorySize; i++)
        {
            var instance = inventoryData[i];
            if (instance == null)
                continue;

            if (instance.item.instanceId == instanceId)
                return instance.item;
        }

        return null;
    }

    public void Consume(ItemInstance item, int amount =1)
    {
        if (item.itemDataSO.itemType != ItemType.Consumable) return;

        item.quantity -= amount;

        if (item.quantity <= 0)
        {
            RemoveInstance(item);
        }

        NotifyChanged();
    }

    public bool HasSpaceForItem(ItemDataSO itemData, int quantity)
    {
        EnsureInventoryData();
        int remainingToFit = quantity;

        if (itemData.maxStack > 1)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                var slot = inventoryData[i];
                if (slot != null && !slot.IsEmpty && slot.item.itemDataSO.itemId == itemData.itemId)
                {
                    int possibleAmount = itemData.maxStack - slot.item.quantity;
                    remainingToFit -= possibleAmount;

                    if (remainingToFit <= 0) return true;
                }
            }
        }

        int emptySlotsNeeded = Mathf.CeilToInt((float)remainingToFit / itemData.maxStack);
        int availableEmptySlots = 0;

        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryData[i] == null || inventoryData[i].IsEmpty)
            {
                availableEmptySlots++;
                if (availableEmptySlots >= emptySlotsNeeded) return true;
            }
        }

        return false;
    }

    public void LoadItemsFromSave(List<ItemSaveData> loadedItems)
    {
        EnsureInventoryData();

        for (int i = 0; i < inventorySize; i++)
        {
            inventoryData[i] = null;
        }
        foreach (var itemSave in loadedItems)
        {
            if (int.TryParse(itemSave.ItemId, out int id))
            {
                ItemDataSO so = itemDataList.GetItemById(id);
                if (so != null)
                {
                    if (IsValidIndex(itemSave.SlotIndex))
                    {
                        inventoryData[itemSave.SlotIndex] = new InventorySlotData
                        {
                            item = new ItemInstance(so, itemSave.Amount)
                        };
                    }
                    else
                    {
                        AddItem(so, itemSave.Amount);
                    }
                }
            }
        }

        NotifyChanged();
    }

    public void LoadItemsFromSession(List<ItemInstance> sessionItems)
    {
        EnsureInventoryData();

        for (int i = 0; i < inventorySize; i++)
            inventoryData[i] = null;

        for (int i = 0; i < sessionItems.Count; i++)
        {
            if (i < inventorySize)
            {
                inventoryData[i] = new InventorySlotData { item = sessionItems[i] };
            }
        }

        NotifyChanged();
    }

    public List<ItemInstance> GetItems()
    {
        EnsureInventoryData();
        List<ItemInstance> items = new List<ItemInstance>();

        foreach (var slot in inventoryData)
        {
            if (slot != null && !slot.IsEmpty && slot.item != null)
            {
                items.Add(slot.item);
            }
        }

        return items;
    }

    public int GetSlotIndexByInstanceId(Guid instanceId)
    {
        for (int i = 0; i < inventoryData.Length; i++)
        {
            if (inventoryData[i]?.item?.instanceId == instanceId) return i;
        }
        return -1;
    }

    public void ClearInventory()
    {
        EnsureInventoryData();

        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryData[i] != null && !inventoryData[i].IsEmpty)
            {
                var instanceId = inventoryData[i].item.instanceId;
                OnItemRemoved?.Invoke(instanceId);
            }

            inventoryData[i] = null;
        }

        NotifyChanged();
    }

}
