using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 공격력 증가 버프 스킬
/// </summary>
public class AttackBoostSkill : BaseBuffSkill
{
    private float _appliedBoost = 0f;
    
    protected override void ApplyBuff()
    {

        if (_statSystem == null || _playerStats == null) return;
        if (_buffApplied) return;
        _buffApplied = true;

        float percentage = GetPercentageValue();
        if (percentage <= 0f) return;
        
        // 현재 공격력을 기준으로 퍼센트 증가
        float currentAttack = _statSystem.GetStat(PlayerStatType.Attack);
        _appliedBoost = currentAttack * percentage;

        // 런타임 스탯에 적용 (실제 게임에서 사용되는 스탯)
        _playerStats.ModifyBaseStat(PlayerStatType.Attack, Mathf.RoundToInt(_appliedBoost));
        
        Debug.Log($"[AttackBoost] 공격력 {percentage * 100f}% 증가: +{_appliedBoost}");
    }
    
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;

        if (_playerStats != null && _appliedBoost > 0f)
        {
            _playerStats.ModifyBaseStat(PlayerStatType.Attack, -Mathf.RoundToInt(_appliedBoost));
            _appliedBoost = 0f;
        }
    }
}

