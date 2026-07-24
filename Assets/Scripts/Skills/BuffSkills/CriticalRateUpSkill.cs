using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 치명타 확률 강화 버프 스킬
/// </summary>
public class CriticalRateUpSkill : BaseBuffSkill
{
    private float _appliedBoost = 0f;
    
    protected override void ApplyBuff()
    {
        if (_statSystem == null || _playerStats == null) return;
        if (_buffApplied) return;
        _buffApplied = true;

        float percentage = GetPercentageValue();
        if (percentage <= 0f) return;

        _appliedBoost = percentage;
        _playerStats.ModifyBaseStat(PlayerStatType.CritRate, _appliedBoost);

        if (_playerStats.GetStat(PlayerStatType.CritRate) > 1f)
        {
            _appliedBoost -= (_playerStats.GetStat(PlayerStatType.CritRate) - 1f);
            _playerStats.OverrideBaseStat(PlayerStatType.CritRate, 1f);
        }

        Debug.Log($"[CriticalRateUp] 치명타 확률 {percentage * 100f}% 증가: +{_appliedBoost}");
    }
    
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;
        if (_playerStats != null && _appliedBoost > 0f)
        {
            _playerStats.ModifyBaseStat(PlayerStatType.CritRate, -_appliedBoost);
            _appliedBoost = 0f;
        }
    }
}

