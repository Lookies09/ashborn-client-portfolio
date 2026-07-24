using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemEnums;

[Serializable]
public class ItemStatModifier
{
    public PlayerEnums.PlayerStatType statType;
    public float flatValue;
    public float percentValue;
}

[Serializable]
public class ConsumableEffect
{
    public ConsumableEffectType effectType; 
    public float value;                     
    public float duration;                  // 지속 시간 (즉발성 아이템은 0)    
}

[Serializable]
public class ItemLevelData
{
    public int level = 0;
    public int upgradeCost;
    public int sellPrice;
    public List<ItemStatModifier> modifiers = new List<ItemStatModifier>();
    [TextArea] public string description;
}

[CreateAssetMenu(fileName = "ItemData", menuName = "SO/Item/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public int itemId;
    public string itemNameKR;
    public string itemNameEN;
    [TextArea] public string descriptionKR;
    [TextArea] public string descriptionEN;
    public Sprite itemIcon;

    [Header("분류")]
    public ItemType itemType;
    public ItemUseType useType = ItemUseType.None;
    public ItemEquipSlot equipSlot = ItemEquipSlot.None;
    public EquipmentApplyType equipmentApplyType = EquipmentApplyType.None;
    public ItemRarity rarity;
    public ItemAcquisitionType acquisitionType;
    public bool canSell = true;
    public bool canPurchase = false;
    public int sellPrice;
    public int purchasePrice;

    [Header("스택(겹칠 수 있는 수)/소지 제한")]
    public int maxStack = 1;
    public int inventoryLimit = 99;

    [Header("효과 설정")]
    // 장비 아이템일 때 사용하는 스탯 수정자 (영구 적용)
    public List<ItemStatModifier> baseModifiers = new List<ItemStatModifier>();
    // 소모 아이템일 때 사용하는 즉시 효과 (1회성 적용)
    public List<ConsumableEffect> consumableEffects = new List<ConsumableEffect>();
    public AudioClip consumableUseSound;
}
