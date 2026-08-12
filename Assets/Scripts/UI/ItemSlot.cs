using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리 슬롯의 부모 오브젝트입니다.
/// 드롭 영역으로 동작하며, 자식 오브젝트인 ItemUI를 관리합니다.
/// </summary>
public class ItemSlot : MonoBehaviour, IDropHandler, IItemInfoProvider
{
    public InventoryManager.ItemInstance currentSlotData { protected set; get; }
    public InventoryManager ownerInventory { protected set; get; }

    // 이 슬롯이 인벤토리 배열의 몇 번째 인덱스인지
    public int slotIndex { protected set; get; }

    [Header("ItemUI 참조")]
    [Tooltip("이 슬롯의 자식 오브젝트인 ItemUI 컴포넌트 (자동으로 찾거나 Inspector에서 할당)")]
    public BaseItemUI ItemUI;

    void Awake()
    {
        // ItemUI를 자동으로 찾기 (자식 오브젝트에서)
        if (ItemUI == null)
        {
            ItemUI = GetComponentInChildren<BaseItemUI>();
        }
    }

    public void Init(InventoryManager inventoryManager, int index)
    {
        ownerInventory = inventoryManager;
        slotIndex = index;
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        ItemDragHandler dragHandler = eventData.pointerDrag.GetComponent<ItemDragHandler>();

        if (dragHandler == null)
        {
            Debug.LogWarning("드래그된 오브젝트에 ItemDragHandler가 없습니다.");
            return;
        }

        ItemSlot sourceSlot = dragHandler.sourceSlot;
        if (sourceSlot == null)
        {
            Debug.LogWarning("드래그된 아이템의 부모 슬롯을 찾을 수 없습니다.");
            return;
        }

        if (ownerInventory == null)
        {
            Debug.LogWarning("대상 슬롯에 InventoryManager가 연결되지 않았습니다.");
            return;
        }

        // EquipSlot에서 ItemSlot(인벤토리)로 드롭하는 경우 (장착 해제)
        if (sourceSlot is EquipSlot sourceEquipSlot)
        {
            // EquipSlot의 RequestUnequip 호출
            sourceEquipSlot.RequestUnequip(this);
            return;
        }

        // QuickSlot에서 ItemSlot(인벤토리)로 드롭하는 경우 (장착 해제)
        if (sourceSlot is QuickSlot sourceQuickSlot)
        {
            sourceQuickSlot.Refresh(null);
            return;
        }

        // 인벤토리 슬롯 간 교환
        int sourceIndex = sourceSlot.slotIndex;
        int targetIndex = this.slotIndex;
        bool moved = false;

        if (sourceSlot.ownerInventory == ownerInventory)
        {
            if (sourceIndex == targetIndex)
                return;

            var sourceItem = ownerInventory.GetItem(sourceIndex);
            var targetItem = ownerInventory.GetItem(targetIndex);

            bool stacked = false;

            // 1️⃣ 스택 가능 여부 체크
            if (sourceItem != null && targetItem != null &&
                !sourceItem.IsEmpty && !targetItem.IsEmpty &&
                sourceItem.item.itemDataSO.itemId == targetItem.item.itemDataSO.itemId &&
                sourceItem.item.itemDataSO.maxStack > 1)
            {
                int canAdd = targetItem.item.itemDataSO.maxStack - targetItem.item.quantity;
                if (canAdd > 0)
                {
                    int moveAmount = Mathf.Min(canAdd, sourceItem.item.quantity);
                    targetItem  .item.quantity += moveAmount;
                    sourceItem.item.quantity -= moveAmount;

                    if (sourceItem.item.quantity <= 0)
                        ownerInventory.RemoveItem(sourceIndex);

                    ownerInventory.UpdateInventoryUI();
                    stacked = true;
                }
            }

            // 2️⃣ 스택 안됐으면 Swap
            if (!stacked)
            {
                ownerInventory.SwapItems(sourceIndex, targetIndex);
            }

            moved = true;
        }
        else if (sourceSlot.ownerInventory != null)
        {
                moved = sourceSlot.ownerInventory.TransferItemTo(
                    ownerInventory,
                    sourceIndex,
                    targetIndex
                );

            // Transfer 실패 시에만 Swap
            if (!moved)
            {
                moved = InventoryManager.SwapBetweenInventories(
                    sourceSlot.ownerInventory,
                    sourceIndex,
                    ownerInventory,
                    targetIndex
                );
            }
        }


        if (moved)
        {
            dragHandler.NotifyDropSuccessful();
        }
    }

    // --- UI 업데이트 함수 ---

    /// <summary>
    /// InventoryManager에서 데이터가 변경되었을 때 호출합니다.
    /// ItemUI에 데이터를 전달하여 UI를 업데이트합니다.
    /// </summary>
    public virtual void Refresh(InventoryManager.ItemInstance slotData)
    {        
        currentSlotData = slotData;
        EnsureItemUI();
        RestoreItemUIState();
        ItemUI.Refresh(slotData);
    }

    private void EnsureItemUI()
    {
        if (ItemUI == null)
            ItemUI = GetComponentInChildren<BaseItemUI>();

        if (ItemUI == null)
        {
            Debug.LogWarning($"ItemSlot {slotIndex}: ItemUI 컴포넌트 없음.");
            return;
        }

        // 드래그 중 부모가 바뀌었으면 원 위치로 복귀
        if (ItemUI.transform.parent != transform)
        {
            ItemUI.transform.SetParent(transform);
            ItemUI.transform.localPosition = Vector3.zero;
            ItemUI.transform.localScale = Vector3.one;
        }
    }

    private void RestoreItemUIState()
    {
        if (ItemUI == null) return;

        var cg = ItemUI.GetComponent<CanvasGroup>();
        if (cg == null) return;

        cg.alpha = 1f;
        cg.blocksRaycasts = true;
    }

    public ItemDataSO GetItemData()
    {
        if (currentSlotData == null || currentSlotData.itemDataSO == null)
            return null;

        return currentSlotData.itemDataSO;
    }
}
