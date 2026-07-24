using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "DropTable", menuName = "SO/Item/Drop Table")]
public class ItemDropTableSO : ScriptableObject
{
    [Header("희귀도 가중치")]
    public float commonWeight = 60f;
    public float uncommonWeight = 25f;
    public float rareWeight = 10f;
    public float epicWeight = 4f;
    public float legendaryWeight = 1f;

    [Header("타입 가중치")]
    public float weaponWeight = 15f;
    public float armorWeight = 15f;
    public float accessoryWeight = 10f;
    public float consumableWeight = 25f;
    public float etcWeight = 35f;

    [Header("드랍 개수")]
    public int minDropCount = 1;
    public int maxDropCount = 4;


    public ItemDataSO GetRandomItemByWeightedChance(ItemDataListSO itemDataList)
    {
        // 1) 희귀도 가중치 선택
        ItemEnums.ItemRarity selectedRarity = GetRandomRarity();

        // 2) 타입 가중치 선택
        ItemEnums.ItemType selectedType = GetRandomItemType();

        // 3) 조건에 맞는 목록 필터
        var candidates = itemDataList.items
            .Where(x => x.rarity == selectedRarity && x.itemType == selectedType)
            .ToList();

        // 조건에 맞는 아이템이 없으면 희귀도 고정하고 다시 뽑기
        if (candidates.Count == 0)
        {
            candidates = itemDataList.items
                .Where(x => x.rarity == selectedRarity)
                .ToList();
        }

        // 그래도 없으면 타입 고정하고 다시 뽑기
        if (candidates.Count == 0)
        {
            candidates = itemDataList.items
                .Where(x => x.itemType == selectedType)
                .ToList();
        }

        // 그래도 없으면 null 반환
        if (candidates.Count == 0)
        {
            Debug.Log($"선택한 {selectedRarity} 등급의 {selectedType} 타입 아이템이 없어서 Null");
            return null;
        }

        // 4) 최종 랜덤 픽
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private ItemEnums.ItemRarity GetRandomRarity()
    {
        float total = commonWeight + uncommonWeight + rareWeight +
                      epicWeight + legendaryWeight;

        float roll = UnityEngine.Random.Range(0, total);

        if (roll < commonWeight) return ItemEnums.ItemRarity.Common;
        roll -= commonWeight;

        if (roll < uncommonWeight) return ItemEnums.ItemRarity.Uncommon;
        roll -= uncommonWeight;

        if (roll < rareWeight) return ItemEnums.ItemRarity.Rare;
        roll -= rareWeight;

        if (roll < epicWeight) return ItemEnums.ItemRarity.Epic;
        return ItemEnums.ItemRarity.Legendary;
    }


    private ItemEnums.ItemType GetRandomItemType()
    {
        float total = weaponWeight + armorWeight + accessoryWeight +
                      consumableWeight + etcWeight;

        float roll = UnityEngine.Random.Range(0, total);

        if (roll < weaponWeight) return ItemEnums.ItemType.Weapon;
        roll -= weaponWeight;

        if (roll < armorWeight) return ItemEnums.ItemType.Armor;
        roll -= armorWeight;

        if (roll < accessoryWeight) return ItemEnums.ItemType.Accessory;
        roll -= accessoryWeight;

        if (roll < consumableWeight) return ItemEnums.ItemType.Consumable;
        return ItemEnums.ItemType.Etc;
    }

}