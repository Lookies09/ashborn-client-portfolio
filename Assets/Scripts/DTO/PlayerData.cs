using System.Collections.Generic;
using System;

[Serializable]
public class SaveData
{
    // 1. 재화 및 기본 정보
    public int Gold;

    // 2. 플레이어 스탯 레벨 (StatType, Level)
    public List<StatLevelData> StatLevels = new List<StatLevelData>();

    // 3. 아이템 본체 데이터 (인벤토리)
    public List<ItemSaveData> InventoryItems = new List<ItemSaveData>();

    // 4. 장착 아이템 (슬롯 타입, 인벤토리 인덱스 혹은 아이템 ID)
    public List<EquipSaveData> EquippedItems = new List<EquipSaveData>();

    // 5. 퀵슬롯 (퀵슬롯 인덱스, 인벤토리 인덱스)
    // 인벤토리의 몇 번째 칸 아이템이 등록되어 있는지 저장
    public List<QuickSlotSaveData> QuickSlots = new();
}

[Serializable]
public class ItemSaveData
{
    public string ItemId;
    public int Amount;   
    public int SlotIndex;
}


[Serializable]
public class EquipSaveData
{
    public ItemEnums.ItemEquipSlot SlotType;
    public string ItemId; // 혹은 인벤토리 인덱스
}

[Serializable]
public class QuickSlotSaveData
{
    public int QuickSlotIndex; // 퀵슬롯 번호 (0~3번 등)
    public int InventoryIndex;  // 인벤토리의 몇 번째 칸 아이템인지
    public string ItemId;      // 안전 장치용 아이템 ID
}