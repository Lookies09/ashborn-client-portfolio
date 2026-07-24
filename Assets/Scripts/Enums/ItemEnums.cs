
public static class ItemEnums 
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Scroll,
        Etc
    }

    public enum ItemUseType
    {
        None,
        Instant,   
        Buff,      
        Cast,      
        Toggle,    
        Script      
    }


    public enum ItemEquipSlot
    {
        None,
        Head,
        Chest,
        Boots,
        WeaponMainHand,
        Accessory,
        Consuming,
    }
    public enum EquipmentApplyType
    {
        None,
        Stat,       // 스탯만 적용
        Buff,       // 버프 형태
        StatAndBuff,
        Script      // 특수 효과
    }


    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum ItemAcquisitionType
    {
        DropOnly,
        PurchaseOnly,
        DropAndPurchase
    }

    public enum ConsumableEffectType
    {
        None,
        RecoverHp,      // 즉시 체력 회복
        RecoverMp,      // 즉시 마나 회복
        Cleanse,        // 디버프 제거
        TemporaryBuff,  // 일시적 능력치 상승 (예: 10초간 공격력 증가)
        ExpGain         // 경험치 획득
    }
}
