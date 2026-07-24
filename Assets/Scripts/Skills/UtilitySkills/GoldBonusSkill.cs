using UnityEngine;

/// <summary>
/// 골드 증가 유틸 스킬
/// </summary>
public class GoldBonusSkill : BaseUtilitySkill
{
    private float _goldBonusMultiplier = 1f;
    
    protected override void ApplyUtility()
    {
        if (_playerStats == null) return;

        float percentage = GetPercentageValue();
        if (percentage <= 0f) return;

        // 경험치 보너스 배율 설정 (예: 5% = 1.05)
        _goldBonusMultiplier = 1f + percentage;
        _playerStats.IngameMoneyBonusRate = _goldBonusMultiplier;

    }


    public override void Cleanup()
    {
        if (_playerStats != null)
        {
            _playerStats.IngameMoneyBonusRate = 1f;
        }
        base.Cleanup();
    }
}

