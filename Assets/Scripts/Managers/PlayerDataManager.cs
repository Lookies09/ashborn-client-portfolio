using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [SerializeField] private ItemDataListSO itemDataList;
    [SerializeField] private CurrentPlayerStateLevelSO playerStateLevelSO;

    // 현재 게임 세션 동안 유지될 데이터 (인스턴스 참조)
    public Dictionary<ItemEnums.ItemEquipSlot, InventoryManager.ItemInstance> SavedEquipments = new();
    private List<InventoryManager.ItemInstance> SavedInventoryItems = new();
    public List<Guid> SavedQuickSlotIds = new();

    private Coroutine _saveCoroutine;

    // 경로 변수
    private string SavePath => Path.Combine(Application.persistentDataPath, "player_data.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnGoldChanged += Save;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSceaneChanged += Save;
        }

        // 게임 시작 시 데이터 로드
        Load();
    }

    private void OnDestroy()
    {
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnGoldChanged -= Save;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSceaneChanged -= Save;
        }
    }

    public void Save()
    {
        if (_saveCoroutine != null) return;

        _saveCoroutine = StartCoroutine(SaveRoutine());
    }

    private IEnumerator SaveRoutine()
    {
        yield return new WaitForEndOfFrame();

        PerformSave();

        yield return new WaitForSecondsRealtime(0.1f);

        _saveCoroutine = null;
    }

    private void PerformSave()
    {
        UpdateSessionData();

        SaveData data = new SaveData();

        // 골드 저장
        data.Gold = PlayerWallet.Instance.Gold;

        // 인벤토리 아이템 저장
        for (int i = 0; i < InventoryManager.PlayerInventory.inventorySize; i++)
        {
            var slot = InventoryManager.PlayerInventory.GetItem(i);
            if (slot != null && !slot.IsEmpty)
            {
                data.InventoryItems.Add(new ItemSaveData
                {
                    ItemId = slot.item.itemDataSO.itemId.ToString(),
                    Amount = slot.item.quantity,
                    SlotIndex = i
                });
            }
        }

        // 퀵슬롯 저장
        var quickSlots = QuickSlotManager.Instance.GetQuickSlots();
        for (int i = 0; i < quickSlots.Length; i++)
        {
            var qs = quickSlots[i];
            if (qs.referencedInstanceId != Guid.Empty)
            {
                int invIdx = InventoryManager.PlayerInventory.GetSlotIndexByInstanceId(qs.referencedInstanceId);

                if (invIdx != -1)
                {
                    data.QuickSlots.Add(new QuickSlotSaveData
                    {
                        QuickSlotIndex = i,
                        InventoryIndex = invIdx,
                        ItemId = qs.ownerInventory.GetInstanceById(qs.referencedInstanceId).itemDataSO.itemId.ToString()
                    });
                }
            }
        }

        // 장착 아이템 저장
        var currentEquips = EquipmentManager.Instance.GetEquippedItems();
        foreach (var kvp in currentEquips)
        {
            data.EquippedItems.Add(new EquipSaveData
            {
                SlotType = kvp.Key,
                ItemId = kvp.Value.itemDataSO.itemId.ToString(),
            });
        }


        // 플레이어 스탯 레벨 저장
        foreach (var statData in playerStateLevelSO.levels)
        {
            data.StatLevels.Add(new StatLevelData
            {
                statType = statData.statType,
                level = statData.level
            });
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

        // 골드 로드
        PlayerWallet.Instance.ResetGold();
        PlayerWallet.Instance.AddGold(data.Gold);

        // 인벤토리 아이템 로드
        SavedInventoryItems.Clear();
        foreach (var itemSave in data.InventoryItems)
        {
            if (int.TryParse(itemSave.ItemId, out int id))
            {
                ItemDataSO so = itemDataList.GetItemById(id);
                if (so != null)
                {
                    var newItm = new InventoryManager.ItemInstance(so, itemSave.Amount);
                    SavedInventoryItems.Add(newItm);
                }
            }
        }

        if (InventoryManager.PlayerInventory != null)
        {
            InventoryManager.PlayerInventory.LoadItemsFromSave(data.InventoryItems);
        }


        // 장착 아이템 로드
        foreach (var equipData in data.EquippedItems)
        {
            if (int.TryParse(equipData.ItemId, out int id))
            {
                ItemDataSO so = itemDataList.GetItemById(id);
                if (so != null)
                {
                    EquipmentManager.Instance.EquipFromSave(equipData.SlotType, so);
                }
            }
        }

        // 퀵슬롯 로드
        foreach (var qsData in data.QuickSlots)
        {
            QuickSlotManager.Instance.RestoreQuickSlot(qsData.QuickSlotIndex, qsData.InventoryIndex);
        }

        // 플레이어 스탯 레벨 로드
        if (playerStateLevelSO != null && data.StatLevels != null)
        {
            foreach (var savedStat in data.StatLevels)
            {
                playerStateLevelSO.SetLevel(savedStat.statType, savedStat.level);
            }
        }
    }

    public void ResetAllData()
    {
        SavedEquipments.Clear();
        SavedInventoryItems.Clear();
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    public void UpdateSessionData()
    {
        // 1. 인벤토리 세션 저장
        SavedInventoryItems.Clear();
        var currentItems = InventoryManager.PlayerInventory.GetItems();
        SavedInventoryItems.AddRange(currentItems);

        // 2. 장착 아이템 세션 저장
        SavedEquipments = EquipmentManager.Instance.GetEquippedItems();

        // 3. 퀵슬롯 세션 저장
        SavedQuickSlotIds.Clear();
        var slots = QuickSlotManager.Instance.GetQuickSlots();
        foreach (var slot in slots)
        {
            SavedQuickSlotIds.Add(slot.referencedInstanceId);
        }
    }

    public void LoadSessionData()
    {
        if (SavedInventoryItems.Count > 0)
        {
            InventoryManager.PlayerInventory.LoadItemsFromSession(SavedInventoryItems);
        }

        foreach (var kvp in SavedEquipments)
        {
            EquipmentManager.Instance.Equip(kvp.Key, kvp.Value);
        }

        if (SavedQuickSlotIds.Count > 0)
        {
            QuickSlotManager.Instance.RestoreFromSession(SavedQuickSlotIds);
        }

        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.RefreshGold();
        }
    }

    [ContextMenu("TestSave")]
    public void TestSave() => Save();

    [ContextMenu("TestLoad")]
    public void TestLoad() => Load();

}