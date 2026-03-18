using System.Collections.Generic;
using System;
using UnityEngine;
using static PlayerEnums;

public class PlayerRuntimeStats : MonoBehaviour
{
    private Dictionary<PlayerStatType, float> baseStats
        = new();

    private List<RuntimeStatModifier> modifiers
        = new();

    private Dictionary<PlayerStatType, float> cachedFinalStats
        = new();

    private bool dirty = true;

    public float IngameMoneyBonusRate;
    public float IngameExpBonusRate;

    public event Action<int> OnMaxHPChanged;

    public event Action OnStatsChanged;

    public void Init(PlayerStatSystem statSystem)
    {
        baseStats.Clear();

        foreach (PlayerStatType type in Enum.GetValues(typeof(PlayerStatType)))
        {
            baseStats[type] = statSystem.GetStat(type);
        }

        dirty = true;
    }


    public void AddModifier(ItemStatModifier data, Guid sourceId)
    { 
        modifiers.Add(new RuntimeStatModifier
        {
            statType = data.statType,
            flatValue = data.flatValue,
            percentValue = data.percentValue,
            sourceId = sourceId
        });

        dirty = true;
        Recalculate();
    }

    public void RemoveModifiersBySource(Guid sourceId)
    {
        modifiers.RemoveAll(m => m.sourceId == sourceId);
        dirty = true;
        Recalculate();
    }

    public float GetStat(PlayerStatType type)
    {
        if (dirty)
            Recalculate();

        return cachedFinalStats.TryGetValue(type, out var value)
            ? value
            : 0f;
    }

    public float GetBaseStat(PlayerStatType type)
    {
        return baseStats.TryGetValue(type, out var value)
            ? value
            : 0f;
    }

    private void Recalculate()
    {
        cachedFinalStats.Clear();

        foreach (var pair in baseStats)
        {
            float flat = 0f;
            float percent = 0f;

            foreach (var mod in modifiers)
            {
                if (mod.statType != pair.Key) continue;

                flat += mod.flatValue;
                percent += mod.percentValue;
            }

            cachedFinalStats[pair.Key] =
                (pair.Value + flat) * (1f + percent);
        }

        dirty = false;

        if (baseStats.TryGetValue(PlayerStatType.MaxHP, out float maxHp))
            OnMaxHPChanged?.Invoke(Mathf.RoundToInt(maxHp));

        OnStatsChanged?.Invoke();
    }

    public void ModifyBaseStat(PlayerStatType type, float amount)
    {
        if (!baseStats.ContainsKey(type))
            baseStats[type] = 0f;

        baseStats[type] += amount;
        dirty = true;
    }

    public void OverrideBaseStat(PlayerStatType type, float value)
    {
        baseStats[type] = value;
        dirty = true;
    }

}
