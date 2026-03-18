using UnityEngine;
using static PlayerEnums;

/// <summary>
/// 모든 스킬의 베이스 클래스
/// </summary>
public abstract class BaseSkill : MonoBehaviour, ISkill
{
    protected SkillDataSO _skillData;
    protected Transform _playerTransform;
    protected PlayerRuntimeStats _playerStats;
    protected int _currentLevel = 1;
    protected float timer = 0f;
    protected bool _isSkillEnabled = true;
    public bool IsSkillEnabled => _isSkillEnabled;

    public int CurrentLevel => _currentLevel;
    public SkillDataSO SkillData => _skillData;
    
    public bool IsReady
    {
        get
        {
            if (_skillData == null || _currentLevel < 1 || _currentLevel > _skillData.levelData.Count)
                return false;
                
            SkillLevelData levelData = _skillData.levelData[_currentLevel - 1];
            float cooldown = levelData.cooldown;

           
            // 플레이어의 쿨다운 감소 적용
            if (_playerStats != null)
            {
                cooldown *= (1f - _playerStats.GetStat(PlayerStatType.CooldownReduction));
            }

            return timer >= cooldown;
        }
    }
    
    public virtual void Initialize(SkillDataSO data, Transform playerTransform, PlayerRuntimeStats playerStats)
    {
        _skillData = data;
        _playerTransform = playerTransform;
        _playerStats = playerStats;
        _currentLevel = 0;
        timer = 0;
    }
    
    public abstract void Activate();
    
    public virtual void UpdateSkill(float deltaTime)
    {
        if (!_isSkillEnabled) return;

        timer += deltaTime;
    }
    
    public virtual void LevelUp(int newLevel)
    {
        if (_currentLevel >= newLevel) return;
        _currentLevel = Mathf.Clamp(newLevel, 1, _skillData.levelData.Count);
        Debug.Log($"현제 {SkillData.skillNameKR} 레벨: {_currentLevel}");
    }
    
    public virtual void Cleanup()
    {
        // 스킬 정리 로직
        Destroy(gameObject);
    }
    
    protected SkillLevelData GetCurrentLevelData()
    {
        if (_skillData == null || _currentLevel < 1 || _currentLevel > _skillData.levelData.Count)
            return null;
            
        return _skillData.levelData[_currentLevel - 1];
    }

    public virtual void SetSkillEnable(bool isEnabled)
    {
        if (_isSkillEnabled == isEnabled) return;

        _isSkillEnabled = isEnabled;

        if (_isSkillEnabled)
            OnSkillEnabled();
        else
            OnSkillDisabled();
    }

    protected virtual void OnSkillEnabled() { }
    protected virtual void OnSkillDisabled() { }

    protected void ApplyDamageToEnemy(IDamageable enemy, float damage)
    {
        if (enemy != null)
        {
            enemy.TakeDamage(Mathf.RoundToInt(damage));
        }
    }
}

