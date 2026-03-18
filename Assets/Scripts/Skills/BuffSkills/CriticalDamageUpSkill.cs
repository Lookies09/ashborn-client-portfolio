using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 치명타 데미지 강화 버프 스킬
/// </summary>
public class CriticalDamageUpSkill : BaseBuffSkill
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
        _playerStats.ModifyBaseStat(PlayerStatType.CritDamage, _appliedBoost);

        
        Debug.Log($"[CriticalDamageUp] 치명타 데미지 {percentage * 100f}% 증가: +{_appliedBoost}");
    }
    
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;

        if (_playerStats != null && _appliedBoost > 0f)
        {
            _playerStats.ModifyBaseStat(PlayerStatType.CritDamage, -_appliedBoost);
            _appliedBoost = 0f;
        }
    }
}

