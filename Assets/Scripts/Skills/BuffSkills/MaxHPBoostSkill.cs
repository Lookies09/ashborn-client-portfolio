using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 최대 체력 강화 버프 스킬
/// </summary>
public class MaxHPBoostSkill : BaseBuffSkill
{
    private int _appliedHP = 0;
    
    protected override void ApplyBuff()
    {
        if (_statSystem == null || _playerStats == null || _playerHealth == null) return;
        if (_buffApplied) return;
        _buffApplied = true;

        float percentage = GetPercentageValue();
        float absolute = GetAbsoluteValue();
        
        int boostAmount = 0;
        
        // 퍼센트와 절대값 중 하나 사용(현재는 절대값으로 증가)
        if (percentage > 0f)
        {
            int currentMaxHP = (int)_statSystem.GetStat(PlayerEnums.PlayerStatType.MaxHP);
            boostAmount = Mathf.RoundToInt(currentMaxHP * percentage);
        }
        else if (absolute > 0f)
        {
            boostAmount = Mathf.RoundToInt(absolute);
        }
        
        if (boostAmount > 0)
        {
            _appliedHP = boostAmount;
            _playerHealth.IncreaseMaxHP(_appliedHP);
            Debug.Log($"[MaxHPBoost] 최대 체력 +{_appliedHP} 증가");
        }
    }
    
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;

        if (_playerHealth != null && _appliedHP > 0)
        {
            float absolute = GetAbsoluteValue();
            if (absolute <= 0f)
            {
                // 최대 체력 감소 (현재 체력은 유지)
                _playerHealth.IncreaseMaxHP(-_appliedHP);
                _appliedHP = 0;
            }  
           
        }
    }
}

