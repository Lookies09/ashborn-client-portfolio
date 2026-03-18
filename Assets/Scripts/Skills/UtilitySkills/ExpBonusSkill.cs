using UnityEngine;

/// <summary>
/// 경험치 증가 유틸 스킬
/// </summary>
public class ExpBonusSkill : BaseUtilitySkill
{
    private float _expBonusMultiplier = 1f;

    protected override void ApplyUtility()
    {
        if (_playerStats == null) return;

        float percentage = GetPercentageValue();
        if (percentage <= 0f) return;

        // 경험치 보너스 배율 설정 (예: 5% = 1.05)
        _expBonusMultiplier = 1f + percentage;
        _playerStats.IngameExpBonusRate = _expBonusMultiplier;

    }


    public override void Cleanup()
    {
        if (_playerStats != null)
        {
            _playerStats.IngameExpBonusRate = 1f;
        }
        base.Cleanup();
    }
}

