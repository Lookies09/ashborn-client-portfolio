using UnityEngine;

/// <summary>
/// 플레이어 인벤토리와 상자 인벤토리 UI를 동시에 관리하는 컨트롤러
/// </summary>
public class InventoryWindowController : MonoBehaviour
{
    [Header("인벤토리 UI 패널")]
    [Tooltip("플레이어 인벤토리 UI 루트")]
    [SerializeField] private GameObject playerInventoryRoot;
    
    [Tooltip("상자 인벤토리 UI 루트")]
    [SerializeField] private GameObject chestInventoryRoot;
    [SerializeField] private Transform chestSlotParent;

    [Header("인벤토리 매니저 참조")]
    [Tooltip("플레이어 인벤토리 매니저")]
    [SerializeField] private InventoryManager playerInventory;

    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private QuickSlotManager quickSlotManager;

    public InventoryManager PlayerInventory => playerInventory;
    public InventoryManager CurrentChestInventory { get; private set; }
    public ChestInteractable CurrentChest { get; private set; }


    /// <summary>
    /// 상자 인벤토리가 현재 열려있는지 확인
    /// </summary>
    public bool IsChestOpen => CurrentChestInventory != null;



    /// <summary>
    /// 상자 인벤토리를 엽니다 (플레이어 인벤토리도 함께 표시)
    /// </summary>
    public void OpenChest(InventoryManager chestInventory, ChestInteractable chest)
    {
        if (chestInventory != null)
            CurrentChestInventory = chestInventory;
        if (chest != null)
            CurrentChest = chest;

        if (chestSlotParent == null)
        {
            Debug.LogWarning("ChestSlotParent가 설정되지 않았습니다.");
        }
        else
        {
            CurrentChestInventory.InitializeOrRebindSlots(chestSlotParent);
        }

        SoundManager.Instance.PlayChestOpen();

        UIManager.Instance.SetPause(true);

        // 플레이어 인벤토리 UI 활성화
        if (playerInventoryRoot != null)
        {
            playerInventoryRoot.SetActive(true);
        }

        // 상자 인벤토리 UI 활성화
        if (chestInventoryRoot != null)
        {
            chestInventoryRoot.SetActive(true);
        }

        // 두 인벤토리 UI 업데이트
        if (playerInventory != null)
        {
            playerInventory.UpdateInventoryUI();
        }
        
        if (CurrentChestInventory != null)
        {
            CurrentChestInventory.UpdateInventoryUI();
        }
    }

    /// <summary>
    /// 상자 인벤토리를 닫습니다 (플레이어 인벤토리는 유지)
    /// </summary>
    public void CloseChest()
    {
        if (CurrentChest != null)
        {
            CurrentChest.CloseFromUI();
            CurrentChest = null;
        }
        SoundManager.Instance.PlayChestClose();
        UIManager.Instance.SetPause(false);

        if (chestInventoryRoot != null)
            chestInventoryRoot.SetActive(false);

        CurrentChestInventory = null;
    }

    /// <summary>
    /// 플레이어 인벤토리만 토글
    /// </summary>
    public void TogglePlayerInventory()
    {
        if (playerInventoryRoot != null)
        {
            bool isActive = playerInventoryRoot.activeSelf;
            playerInventoryRoot.SetActive(!isActive);

            if (!isActive && playerInventory != null)
            {
                playerInventory.UpdateInventoryUI();
            }
        }
    }

    /// <summary>
    /// 모든 인벤토리 UI를 닫습니다
    /// </summary>
    public void CloseAllInventories()
    {
        SoundManager.Instance.PlayInventoryCloseSound();

        CloseChest();
        
        if (playerInventoryRoot != null)
        {
            playerInventoryRoot.SetActive(false);
        }
    }
    /// <summary>
    /// 인벤토리 내 모든 아이템을 플레이어 인벤토리로 옮깁니다.
    /// </summary>
    public void TransferAllItemsToPlayerInventory()
    {
        if (!CurrentChestInventory)
            return;

        CurrentChestInventory.TransferAllItemsTo(playerInventory);
    }

    public InventoryManager GetOtherInventory(InventoryManager self)
    {
        if (self == playerInventory)
            return CurrentChestInventory;

        return playerInventory;
    }
}
