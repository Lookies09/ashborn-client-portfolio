using System.Collections.Generic;
using UnityEngine;
using static ItemEnums;
using static PlayerEnums;

public static class StatNameResolver
{
    private static readonly Dictionary<PlayerEnums.PlayerStatType, string> Map = new()
    {
        { PlayerStatType.MaxHP, "최대 체력" },
        { PlayerStatType.Attack, "공격력" },
        { PlayerStatType.MoveSpeed, "이동 속도" },
        { PlayerStatType.CritRate, "크리티컬 확률" },
        { PlayerStatType.CritDamage, "크리티컬 데미지" },
        { PlayerStatType.Defense, "방어력" },
        { PlayerStatType.Luck, "행운" },
        { PlayerStatType.CooldownReduction, "쿨타임 감소율" },
        { PlayerStatType.Regen, "체력 재생률" },
        { PlayerStatType.MaxMp, "최대 마나" },

    };

    public static string Get(PlayerEnums.PlayerStatType type)
        => Map.TryGetValue(type, out var name) ? name : type.ToString();
}

public static class StatValueFormatter
{
    private static bool IsPercentStat(PlayerStatType statType)
    {
        return statType == PlayerEnums.PlayerStatType.CritRate
            || statType == PlayerEnums.PlayerStatType.CritDamage
            || statType == PlayerEnums.PlayerStatType.Luck
            || statType == PlayerEnums.PlayerStatType.CooldownReduction;
    }

    public static string FormatStatValue(PlayerStatType statType, float value)
    {
        if (IsPercentStat(statType))
            return (value * 100f).ToString("0") + "%";

        return value.ToString("0");
    }
}

public static class ConsumableEffectTypeNameResolver
{
    private static readonly Dictionary<ConsumableEffectType, string> Map = new()
    {
        { ConsumableEffectType.RecoverHp, "체력 회복" },
        { ConsumableEffectType.RecoverMp, "마나 회복" },
        { ConsumableEffectType.TemporaryBuff, "일시적 버프" },
        { ConsumableEffectType.Cleanse, "디버프 제거" },
        { ConsumableEffectType.ExpGain, "경험치 획득" },

    };

    public static string Get(ConsumableEffectType type)
        => Map.TryGetValue(type, out var name) ? name : type.ToString();
}


[System.Serializable]
public class StatUpgradeConfig
{
    public PlayerEnums.PlayerStatType statType;    
    public int maxLevel = 5;
    public int[] upgradeCostPerLevel;
    public float[] valueIncreasePerLevel;
    public string StatName => StatNameResolver.Get(statType);

    public float GetValueIncreaseByLevel(int level)
    {
        if (level < 1 || level > maxLevel) return 0f;
        return valueIncreasePerLevel[level - 1];
    }

    public float GetTotalValueIncreaseByLevels(int startLevel, int levelsToUpgrade)
    {
        float totalIncrease = 0f;
        int endLevel = Mathf.Min(startLevel + levelsToUpgrade, maxLevel);
        for (int i = startLevel+1; i <= endLevel; i++)
        {
            totalIncrease += GetValueIncreaseByLevel(i);
        }
        return totalIncrease;
    }

    public int GetUpgradeCostByLevel(int level)
    {
        if (level < 1 || level > maxLevel) return 0;
        return upgradeCostPerLevel[level - 1];
    }

    public int GetTotalCostByLevels(int startLevel, int levelsToUpgrade)
    {
        int totalCost = 0;
        int endLevel = Mathf.Min(startLevel + levelsToUpgrade, maxLevel);
        for (int i = startLevel + 1; i <= endLevel; i++)
        {
            totalCost += GetUpgradeCostByLevel(i);
        }
        return totalCost;
    }
}

[CreateAssetMenu(menuName = "Stats/Player Stat Upgrade Config")]
public class PlayerStatUpgradeConfigSO : ScriptableObject
{
    public List<StatUpgradeConfig> configs;
    
    public StatUpgradeConfig GetConfig(PlayerEnums.PlayerStatType type)
    {
        return configs.Find(c => c.statType == type);
    }
}