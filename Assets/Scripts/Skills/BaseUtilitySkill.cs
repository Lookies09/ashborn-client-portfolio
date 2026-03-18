using UnityEngine;

/// <summary>
/// 유틸형 스킬의 베이스 클래스
/// 게임 시스템에 영향을 주는 스킬들 (골드, 경험치 등)
/// </summary>
public abstract class BaseUtilitySkill : BaseSkill
{
    //protected GameRewardSystem _rewardSystem; // 골드 시스템

    public override void Initialize(SkillDataSO data, Transform playerTransform, PlayerRuntimeStats playerStats)
    {
        base.Initialize(data, playerTransform, playerStats);
        
        // GameRewardSystem 찾기 (싱글톤 또는 매니저)
        //_rewardSystem = FindObjectOfType<GameRewardSystem>();
        //if (_rewardSystem == null)
        //{
        //    Debug.LogWarning("GameRewardSystem을 찾을 수 없습니다. 유틸 스킬이 제대로 작동하지 않을 수 있습니다.");
        //}
    }
    
    public override void Activate()
    {
        // 유틸 스킬은 레벨업 시 자동으로 적용됨
        ApplyUtility();
    }
    
    public override void LevelUp(int newLevel)
    {
        base.LevelUp(newLevel);
        //ApplyUtility();
    }
    
    /// <summary>
    /// 유틸 효과 적용 (각 유틸 스킬에서 구현)
    /// </summary>
    protected abstract void ApplyUtility();
    
    /// <summary>
    /// 현재 레벨의 퍼센트 값 가져오기
    /// </summary>
    protected float GetPercentageValue()
    {
        SkillLevelData levelData = GetCurrentLevelData();
        return levelData != null ? levelData.percentageValue : 0f;
    }
}

