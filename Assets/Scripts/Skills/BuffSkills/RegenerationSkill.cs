using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 지속 회복 버프 스킬
/// </summary>
public class RegenerationSkill : BaseBuffSkill
{
    private float _appliedRegen = 0f;
    private float _regenAccumulator = 0f;

    protected override void ApplyBuff()
    {
        if (_statSystem == null || _playerStats == null) return;
        if (_buffApplied) return;
        _buffApplied = true;

        float regenValue = GetPercentageValue();
        if (regenValue <= 0f) return;
                
        _appliedRegen = _playerStats.GetStat(PlayerStatType.MaxHP) * regenValue / 100;

        _playerStats.ModifyBaseStat(PlayerStatType.Regen, _appliedRegen);
        
        Debug.Log($"[Regeneration] 초당 {_appliedRegen} HP 회복 추가");
    }
    
    public override void UpdateSkill(float deltaTime)
    {
        if (_playerHealth == null || _appliedRegen <= 0f) return;

        _regenAccumulator += _appliedRegen * deltaTime;

        if (_regenAccumulator >= 1f)
        {
            int healAmount = Mathf.FloorToInt(_regenAccumulator);
            _playerHealth.Heal(healAmount);
            _regenAccumulator -= healAmount;
        }
    }
    
    protected override void RemoveBuff()
    {
        if (!_buffApplied) return;
        _buffApplied = false;

        if (_playerStats != null && _appliedRegen > 0f)
        {
            _playerStats.ModifyBaseStat(PlayerStatType.Regen, -_appliedRegen);
            _appliedRegen = 0f;
        }
    }
}

