using UnityEngine;

/// <summary>
/// 버프형 스킬의 베이스 클래스
/// 플레이어 스탯을 영구적으로 증가시키는 스킬들
/// </summary>
public abstract class BaseBuffSkill : BaseSkill
{
    protected PlayerStatSystem _statSystem;
    protected PlayerHealth _playerHealth;
    protected float _baseStatValue; // 초기 스탯 값 (레벨업 시 계산용)
    protected bool _buffApplied = false;


    public override void Initialize(SkillDataSO data, Transform playerTransform, PlayerRuntimeStats playerStats)
    {
        base.Initialize(data, playerTransform, playerStats);
        
        _statSystem = playerTransform.GetComponent<PlayerStatSystem>();
        _playerHealth = playerTransform.GetComponent<PlayerHealth>();
    }
    
    public override void Activate()
    {
        // 버프 스킬은 레벨업 시 자동으로 적용됨
        ApplyBuff();
    }
    
    public override void LevelUp(int newLevel)
    {
        // 이전 레벨의 버프 제거
        RemoveBuff();
        
        base.LevelUp(newLevel);
        
        // 새 레벨의 버프 적용
        ApplyBuff();
    }
    
    public override void Cleanup()
    {
        RemoveBuff();
        base.Cleanup();
    }
    
    /// <summary>
    /// 버프 적용 (각 버프 스킬에서 구현)
    /// </summary>
    protected abstract void ApplyBuff();
    
    /// <summary>
    /// 버프 제거 (각 버프 스킬에서 구현)
    /// </summary>
    protected abstract void RemoveBuff();
    
    /// <summary>
    /// 현재 레벨의 퍼센트 값 가져오기
    /// </summary>
    protected float GetPercentageValue()
    {
        SkillLevelData levelData = GetCurrentLevelData();
        return levelData != null ? levelData.percentageValue : 0f;
    }
    
    /// <summary>
    /// 현재 레벨의 절대값 가져오기
    /// </summary>
    protected float GetAbsoluteValue()
    {
        SkillLevelData levelData = GetCurrentLevelData();
        return levelData != null ? levelData.absoluteValue : 0f;
    }
}

