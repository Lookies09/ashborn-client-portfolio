using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점에서 판매할 아이템과 구매 로직을 관리하는 컨트롤러입니다.
/// 재화 차감과 플레이어 인벤토리 지급을 한곳에서 처리합니다.
/// </summary>
public class ShopController : MonoBehaviour
{
    [System.Serializable]
    public class ShopItemEntry
    {
        [Tooltip("에디터/디버그용 식별자 (중복 허용)")]
        public string displayName;

        [Tooltip("판매할 아이템 데이터")]
        public ItemDataSO itemData;

        [Tooltip("-1이면 무한 판매, 0 이상이면 재고 수량")]
        public int initialStock = -1;

        [HideInInspector] public int runtimeStock;

        public bool HasInfiniteStock => initialStock < 0;

        public int CurrentStock => HasInfiniteStock ? -1 : runtimeStock;

        public void ResetRuntimeStock()
        {
            runtimeStock = Mathf.Max(initialStock, 0);
        }

        public bool HasEnoughStock(int requestedAmount)
        {
            if (requestedAmount <= 0) return false;
            if (HasInfiniteStock) return true;
            return runtimeStock >= requestedAmount;
        }

        public void ConsumeStock(int amount)
        {
            if (HasInfiniteStock) return;
            runtimeStock = Mathf.Max(runtimeStock - amount, 0);
        }
    }

    [Header("참조")]
    [SerializeField] private InventoryManager playerInventory;
    [SerializeField] private LobbyUIManager lobbyUIManager;

    [Header("상점 설정")]
    [SerializeField] private List<ShopItemEntry> shopItems = new List<ShopItemEntry>();

    [Header("자동 생성 설정")]
    [SerializeField] private bool autoFillFromSO = false;
    [SerializeField] private ItemDataListSO masterItemDataList;

    [Header("UI 설정")]
    [SerializeField] private GameObject shopItemSlotPrefab;
    [SerializeField] private Transform shopItemSlotParent;
    [SerializeField] private ShopItemModal shopItemModal;
    [SerializeField] private ShopSellModal shopSellModal;


    public IReadOnlyList<ShopItemEntry> Items => shopItems;

    private void Awake()
    {        
        ResetStocks();
    }

    private void Start()
    {
        InitializeShopUI();
    }

    public void InitializeShopUI()
    {
        if (autoFillFromSO)
        {
            FillShopItemsFromSO();
        }

        GenerateSlots();
    }

    public void GenerateSlots()
    {
        if (shopItemSlotPrefab == null || shopItemSlotParent == null)
        {
            Debug.LogWarning("ShopController: 상점 UI 프리팹 또는 부모가 설정되지 않았습니다.");
            return;
        }

        foreach (Transform child in shopItemSlotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < shopItems.Count; i++)
        {
            var entry = shopItems[i];
            GameObject slotObj = Instantiate(shopItemSlotPrefab, shopItemSlotParent);
            ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();
            if (slot != null)
            {
                slot.Bind(this, entry, i);
            }
        }
    }

    private void FillShopItemsFromSO()
    {
        if (masterItemDataList == null) return;

        shopItems.Clear(); // 기존 수동 리스트 초기화

        foreach (var item in masterItemDataList.items)
        {
            // 구매 가능한 아이템만 상점에 등록
            if (item.canPurchase)
            {
                shopItems.Add(new ShopItemEntry
                {
                    displayName = item.itemNameKR,
                    itemData = item,
                    initialStock = -1 
                });
            }
        }

        ResetStocks(); 
    }

    /// <summary>
    /// 특정 인덱스 아이템 구매 시도
    /// </summary>
    public bool TryPurchaseByIndex(int index, int quantity = 1)
    {
        if (index < 0 || index >= shopItems.Count)
        {
            Debug.LogWarning("ShopController: 잘못된 상품 인덱스입니다.");
            return false;
        }

        return TryPurchaseItem(shopItems[index], quantity);
    }

    /// <summary>
    /// displayName으로 아이템을 찾아 구매 시도
    /// </summary>
    public bool TryPurchaseByName(string name, int quantity = 1)
    {
        ShopItemEntry entry = shopItems.Find(item => item.displayName == name);
        if (entry == null)
        {
            Debug.LogWarning($"ShopController: '{name}' 상품을 찾을 수 없습니다.");
            return false;
        }

        return TryPurchaseItem(entry, quantity);
    }

    public bool TryPurchaseItem(ShopItemEntry entry, int quantity)
    {
        if (entry == null || entry.itemData == null || quantity <= 0) return false;

        if (!entry.HasEnoughStock(quantity))
        {
            Debug.LogWarning("ShopController: 재고가 부족합니다.");
            return false;
        }

        if (!playerInventory.HasSpaceForItem(entry.itemData, quantity))
        {
            Debug.LogWarning("인벤토리 공간 부족");
            // 여기서 유저에게 알림 UI(Toast 등)를 띄워주면 좋습니다.
            return false;
        }

        int totalPrice = entry.itemData.purchasePrice * quantity;

        bool isSuccess = PlayerWallet.Instance.SpendGold(totalPrice);

        if (isSuccess)
        {
            entry.ConsumeStock(quantity);

            for (int i = 0; i < quantity; i++)
            {
                playerInventory.AddItem(entry.itemData, 1);
            }

            return true;
        }
        else
        {
            lobbyUIManager.NotifyGoldShortage();
            return false;
        }
    }


    [ContextMenu("재고 초기화")]
    public void ResetStocks()
    {
        foreach (var entry in shopItems)
        {
            if (entry == null) continue;
            entry.ResetRuntimeStock();
        }
    }

    public void SetPlayerInventory(InventoryManager inventory)
    {
        playerInventory = inventory;
    }

    public void OnShopItemSlotClicked(ShopItemEntry shopItem)
    {
        if (shopItemModal != null)
            shopItemModal.Open(this, shopItem);
    }

    public void OpenSellModal(InventoryManager.ItemInstance item)
    {
        if (!item.itemDataSO.canSell) return;
        shopSellModal.Open(this, item);
    }

    public void ExecuteSell(InventoryManager.ItemInstance item , int qty, int finalPrice)
    {
        InventoryManager.PlayerInventory.RemoveItemByGuid(item.instanceId, qty);

        PlayerWallet.Instance.AddGold(finalPrice);
    }
}

