using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 쿨타임 감소 버프 스킬
/// </summary>
public class CooldownReductionSkill : BaseBuffSkill
{
    
    protected override void ApplyBuff()
    {
        if (_statSystem == null || _playerStats == null) return;

        if (_buffApplied) return;
        _buffApplied = true;

        float percentage = GetPercentageValue();
        if (percentage <= 0f) return;

        _playerStats.ModifyBaseStat(PlayerStatType.CooldownReduction, percentage);
        
        // 최대치 제한
        if (_playerStats.GetStat(PlayerStatType.CooldownReduction) > 0.7f)
        {
            _playerStats.OverrideBaseStat(PlayerStatType.CooldownReduction, 0.7f);
        }
        
        Debug.Log($"[CooldownReduction] 쿨타임 {percentage * 100f}% 감소");
    }


    // TODO: 쿨타임 감소 버프 제거 로직 필요할까?
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;

    }
}

