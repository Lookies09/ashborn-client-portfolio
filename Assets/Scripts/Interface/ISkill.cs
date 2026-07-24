using UnityEngine;

/// <summary>
/// 모든 스킬이 구현해야 하는 인터페이스
/// </summary>
public interface ISkill
{
    bool IsSkillEnabled { get; }

    /// <summary>
    /// 스킬 On/Off 함수
    /// </summary>
    void SetSkillEnable(bool isEnabled);

    /// <summary>
    /// 스킬 초기화
    /// </summary>
    void Initialize(SkillDataSO data, Transform playerTransform, PlayerRuntimeStats playerStats);
    
    /// <summary>
    /// 스킬 활성화 (액티브 스킬의 경우)
    /// </summary>
    void Activate();
    
    /// <summary>
    /// 스킬 업데이트 (패시브 스킬이나 지속 효과의 경우)
    /// </summary>
    void UpdateSkill(float deltaTime);
    
    /// <summary>
    /// 스킬 레벨업
    /// </summary>
    void LevelUp(int newLevel);
    
    /// <summary>
    /// 스킬 정리
    /// </summary>
    void Cleanup();
    
    /// <summary>
    /// 현재 스킬 레벨
    /// </summary>
    int CurrentLevel { get; }
    
    /// <summary>
    /// 스킬 데이터
    /// </summary>
    SkillDataSO SkillData { get; }
    
    /// <summary>
    /// 쿨다운이 끝났는지 확인
    /// </summary>
    bool IsReady { get; }
}

