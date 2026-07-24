using Unity.VisualScripting;
using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 방어력 강화 버프 스킬
/// </summary>
public class DefenseBoostSkill : BaseBuffSkill
{
    private float _appliedBoost = 0f;
    
    protected override void ApplyBuff()
    {
        if (_statSystem == null || _playerStats == null) return;
        if (_buffApplied) return;
        _buffApplied = true;

        float percentage = GetPercentageValue();
        if (percentage <= 0f) return;
        
        // 현재 방어력을 기준으로 퍼센트 증가
        float currentDefense = _statSystem.GetStat(PlayerStatType.Defense);
        _appliedBoost = currentDefense * percentage;

        float rounded = Mathf.Round(_appliedBoost * 10f) / 10f;
        _appliedBoost = rounded;

        _playerStats.ModifyBaseStat(PlayerStatType.Defense, _appliedBoost);

        Debug.Log($"[DefenseBoost] 방어력 {percentage * 100f}% 증가: +{_appliedBoost}");
    }
    
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;

        if (_playerStats != null && _appliedBoost > 0f)
        {
            _playerStats.ModifyBaseStat(PlayerStatType.Defense, -_appliedBoost);
            _appliedBoost = 0f;
        }
    }
}

