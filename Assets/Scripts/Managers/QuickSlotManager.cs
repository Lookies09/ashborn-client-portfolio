using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    private InventoryManager playerInventory;
    private PlayerContext playerContext;

    [SerializeField] private QuickSlot[] quickSlots;
    [SerializeField] private QuickItemBtn[] quickItemBtns;
    private void Awake()
    {
        Instance = this;

        PlayerContext playerContext = FindFirstObjectByType<Player>().GetContext();
        Init(playerContext);
    }

    private void OnEnable()
    {
        playerInventory.OnItemRemoved += HandleItemRemoved;
        playerInventory.OnItemMoved += HandleItemMoved;
    }

    private void OnDisable()
    {
        playerInventory.OnItemRemoved -= HandleItemRemoved;
        playerInventory.OnItemMoved -= HandleItemMoved;
    }
    
    public void Init(PlayerContext playerContext)
    {
        this.playerContext = playerContext;
        playerInventory = playerContext.PlayerInventoryManager;

        if (quickSlots.Length != quickItemBtns.Length)
        {
            Debug.Log("ÇöÀç Äü½½·Ô ¹öÆ°°ú Äü½½·Ô °¹¼ö°¡ ÀÏÄ¡ÇÏÁö ¾Ê½À´Ï´Ù. Äü½½·Ô ¸Þ´ÏÀú È®ÀÎ ¹Ù¶÷");
        }

        int count = Mathf.Min(quickSlots.Length, quickItemBtns.Length);
        for (int i = 0; i < count; i++)
        {
            quickSlots[i].Init(playerInventory, i);
            quickSlots[i].Manager = this;
            quickItemBtns[i].Init(this, i);
        }
    }


    public void BindQuickSlot(int quickSlotIndex)
    {
        RefreshQuickSlotUI(quickSlotIndex);
        RefreshQuickItemBtn(quickSlotIndex);
    }

    public void ClearQuickSlot(int index)
    {
        if (index < 0 || index >= quickSlots.Length) return;

        quickSlots[index].Clear();
        RefreshQuickItemBtn(index);
    }

    public InventoryManager.ItemInstance GetQuickSlotItem(int index)
    {
        if (index < 0 || index >= quickSlots.Length) return null;

        var slot = quickSlots[index];
        if (slot.currentSlotData == null) return null;

        return slot.currentSlotData;
    }

    public void RefreshQuickSlotUI(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= quickSlots.Length) return;

        var slot = quickSlots[quickSlotIndex];
        InventoryManager.ItemInstance data = null;
        if (slot.currentSlotData != null && slot.slotIndex >= 0)
        {
            data = slot.currentSlotData;
        }

        quickSlots[quickSlotIndex].Refresh(data);
    }

    public void RefreshQuickItemBtn(int index)
    {
        if (index < 0 || index >= quickItemBtns.Length) return;

        var slot = quickSlots[index];
        var btn = quickItemBtns[index];

        if (slot == null || btn == null)
            return;

        var instance = slot.ownerInventory.GetInstanceById(slot.referencedInstanceId);
        btn.Refresh(instance);
    }


    public void UseQuickSlot(int slotIndex)
    {
        var slot = quickSlots[slotIndex];
        if (slot == null) return;

        var instanceId = slot.referencedInstanceId;
        if (instanceId == Guid.Empty) return;

        var instance = slot.ownerInventory.GetInstanceById(instanceId);
        if (instance == null) return;

        ItemUseSystem.Use(instance, playerContext);

        BindQuickSlot(slotIndex);
    }

    private void HandleItemRemoved(Guid instanceId)
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i].referencedInstanceId == instanceId)
            {
                ClearQuickSlot(i);
            }
        }
    }

    private void HandleItemMoved(Guid instanceId)
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i].referencedInstanceId == instanceId)
            {
                BindQuickSlot(i);
            }
        }
    }

    public bool IsItemAlreadyBound(Guid instanceId)
    {
        return quickSlots.Any(s => s.referencedInstanceId == instanceId);
    }

    public void ClearBoundItem(Guid instanceId)
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i].referencedInstanceId == instanceId)
            {
                ClearQuickSlot(i);
                return;
            }
        }
    }

    public QuickSlot[] GetQuickSlots() => quickSlots;

    public void RestoreQuickSlot(int quickSlotIndex, int inventoryIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= quickSlots.Length) return;

        var slotData = playerInventory.GetItem(inventoryIndex);

        if (slotData != null && !slotData.IsEmpty)
        {
            quickSlots[quickSlotIndex].SetReference(slotData.item.instanceId);
            RefreshQuickItemBtn(quickSlotIndex);
        }
    }

    public void RestoreFromSession(List<Guid> sessionIds)
    {
        if (sessionIds == null || sessionIds.Count == 0) return;

        if (playerInventory == null && playerContext != null)
        {
            playerInventory = playerContext.PlayerInventoryManager;
        }

        for (int i = 0; i < Mathf.Min(quickSlots.Length, sessionIds.Count); i++)
        {
            if (sessionIds[i] == Guid.Empty) continue;

            quickSlots[i].referencedInstanceId = sessionIds[i];
            var instance = playerInventory.GetInstanceById(sessionIds[i]);
            if (instance != null)
            {
                quickSlots[i].Refresh(instance);
            }

            BindQuickSlot(i);
            RefreshQuickItemBtn(i);
        }
    }

    public void ClearAllQuickSlots()
    {
        if (quickSlots == null) return;

        for (int i = 0; i < quickSlots.Length; i++)
        {
            ClearQuickSlot(i);
            RefreshQuickItemBtn(i);
        }

    }
}

