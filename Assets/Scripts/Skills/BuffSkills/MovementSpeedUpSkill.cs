using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 이동 속도 증가 버프 스킬
/// </summary>
public class MovementSpeedUpSkill : BaseBuffSkill
{
    private float _appliedBoost = 0f;
    
    protected override void ApplyBuff()
    {
        if (_statSystem == null || _playerStats == null) return;
        if (_buffApplied) return;
        _buffApplied = true;

        float percentage = GetPercentageValue();
        if (percentage <= 0f) return;
        
        // 현재 이동 속도를 기준으로 퍼센트 증가
        float currentSpeed = _statSystem.GetStat(PlayerStatType.MoveSpeed);
        _appliedBoost = currentSpeed * percentage / 100;

        _playerStats.ModifyBaseStat(PlayerStatType.MoveSpeed, _appliedBoost);
        
        Debug.Log($"[MovementSpeedUp] 이동 속도 {percentage}% 증가: +{_appliedBoost}");
    }
    
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;

        if (_playerStats != null && _appliedBoost > 0f)
        {
            _playerStats.ModifyBaseStat(PlayerStatType.MoveSpeed, -_appliedBoost);
            _appliedBoost = 0f;
        }
    }
}

