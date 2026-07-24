using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 스킬들을 관리하는 매니저
/// </summary>
public class SkillManager : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private PlayerRuntimeStats _playerStats;
    [SerializeField] private SkillDataSO startSkill;

    [Header("Mana Settings")]
    [SerializeField] private float manaRegenRate = 5;
    [SerializeField] private float manaCost = 20;
    public int currentMp { get; private set; }
    private int _lastMana = -1;
    private float _manaCounter;
    private int _maxMana = 100;

    private Dictionary<SkillDataSO, ISkill> _activeSkills = new Dictionary<SkillDataSO, ISkill>();
    private Dictionary<SkillEnums.SkillImplementationType, System.Type> _skillTypeMap;

    public bool IsGlobalSkillEnabled { get; private set; } = false;


    public System.Action<bool> OnSkillStateChanged;
    public System.Action<int> OnManaChanged;

    private void Awake()
    {
        InitializeSkillTypeMap();
        
        if (_playerTransform == null)
        {
            _playerTransform = transform;
        }
        
        if (_playerStats == null)
        {
            _playerStats = GetComponent<PlayerRuntimeStats>();
        }                
    }

    private void Start()
    {
        PlayerLevelManager.Instance.SetSkillManager(this);

        // 초기 마나 세팅
        _maxMana = (int)_playerStats.GetStat(PlayerEnums.PlayerStatType.MaxMp);
        currentMp = _maxMana;
        _manaCounter = currentMp;
        _lastMana = currentMp;
        OnManaChanged?.Invoke(currentMp);

        AddSkill(startSkill);
    }

    private void Update()
    {
        float time = Time.deltaTime;

        UpdateMana(time);

        // 모든 액티브 스킬 업데이트
        foreach (var skill in _activeSkills.Values)
        {
            skill.UpdateSkill(time);
        }
    }

    private void UpdateMana(float time)
    {
        if (IsGlobalSkillEnabled)
        {
            _manaCounter -= (manaCost * time);            

            if (currentMp <= 0)
            {
                // 마나 소진 시 모든 스킬 비활성화
                _manaCounter = 0;
                SetAllSkillsActive(false);
            }
        }
        else
        {
            if (_manaCounter < _maxMana)
            {
                _manaCounter += (manaRegenRate * time);
            }                
        }

        _manaCounter = Mathf.Clamp(_manaCounter, 0, _maxMana);
        currentMp = (int)_manaCounter;

        if (_lastMana != currentMp)
        {
            OnManaChanged?.Invoke(currentMp);
            _lastMana = currentMp;
        }
    }

    private void InitializeSkillTypeMap()
    {
        _skillTypeMap = new Dictionary<SkillEnums.SkillImplementationType, System.Type>
        {
            // 공격형
            { SkillEnums.SkillImplementationType.CrossSlash, typeof(CrossSlashSkill) },
            { SkillEnums.SkillImplementationType.SpiritFlame, typeof(SpiritFlameSkill) },
            { SkillEnums.SkillImplementationType.HolyArea, typeof(HolyAreaSkill) },
            { SkillEnums.SkillImplementationType.ScytheThrow, typeof(ScytheThrowSkill) },
            { SkillEnums.SkillImplementationType.SpinningBlade, typeof(SpinningBladeSkill) },
            { SkillEnums.SkillImplementationType.LightningStrike, typeof(LightningStrikeSkill) },
            
            // 버프형
            { SkillEnums.SkillImplementationType.AttackBoost, typeof(AttackBoostSkill) },
            { SkillEnums.SkillImplementationType.CooldownReduction, typeof(CooldownReductionSkill) },
            { SkillEnums.SkillImplementationType.MovementSpeedUp, typeof(MovementSpeedUpSkill) },
            { SkillEnums.SkillImplementationType.MaxHPBoost, typeof(MaxHPBoostSkill) },
            { SkillEnums.SkillImplementationType.DefenseBoost, typeof(DefenseBoostSkill) },
            { SkillEnums.SkillImplementationType.Regeneration, typeof(RegenerationSkill) },
            { SkillEnums.SkillImplementationType.CriticalRateUp, typeof(CriticalRateUpSkill) },
            { SkillEnums.SkillImplementationType.CriticalDamageUp, typeof(CriticalDamageUpSkill) },
            
            // 유틸형
            { SkillEnums.SkillImplementationType.GoldBonus, typeof(GoldBonusSkill) },
            { SkillEnums.SkillImplementationType.ExpBonus, typeof(ExpBonusSkill) }
        };
    }
    
    
    
    public void AddSkill(SkillDataSO skillData)
    {
        if (skillData == null) return;
        
        // 이미 있는 스킬이면 레벨업
        if (_activeSkills.ContainsKey(skillData))
        {
            int nextLevel = _activeSkills[skillData].CurrentLevel + 1;
            _activeSkills[skillData].LevelUp(nextLevel);
            return;
        }
        
        // 새 스킬 생성
        ISkill skill = CreateSkillInstance(skillData);
        if (skill != null)
        {
            skill.Initialize(skillData, _playerTransform, _playerStats);
            skill.LevelUp(1);

            skill.SetSkillEnable(IsGlobalSkillEnabled);

            // 패시브 스킬은 즉시 활성화
            if (skillData.activationType == SkillEnums.SkillActivationType.Passive)
            {
                if (skill.IsSkillEnabled)
                {
                    skill.Activate();
                }
            }
            
            _activeSkills[skillData] = skill;
        }
    }
    
    /// <summary>
    /// 스킬 인스턴스 생성
    /// </summary>
    private ISkill CreateSkillInstance(SkillDataSO skillData)
    {
        if (!_skillTypeMap.TryGetValue(skillData.implementationType, out System.Type skillType))
        {
            Debug.LogError($"스킬 타입 {skillData.implementationType}에 대한 구현 클래스를 찾을 수 없습니다.");
            return null;
        }
        
        // GameObject에 컴포넌트로 추가
        GameObject skillObject = new GameObject($"Skill_{skillData.skillNameEN}");
        skillObject.transform.SetParent(transform);
        
        ISkill skill = skillObject.AddComponent(skillType) as ISkill;
        
        return skill;
    }
    
    /// <summary>
    /// 스킬 제거
    /// </summary>
    public void RemoveSkill(SkillDataSO skillData)
    {
        if (_activeSkills.TryGetValue(skillData, out ISkill skill))
        {
            skill.Cleanup();
            _activeSkills.Remove(skillData);
        }
    }
    
    /// <summary>
    /// 모든 스킬 정리
    /// </summary>
    public void ClearAllSkills()
    {
        foreach (var skill in _activeSkills.Values)
        {
            skill.Cleanup();
        }
        _activeSkills.Clear();
    }
    
    /// <summary>
    /// 현재 활성화된 스킬 목록 반환
    /// </summary>
    public IEnumerable<ISkill> GetActiveSkills()
    {
        return _activeSkills.Values;
    }

    public int GetSkillLevel(SkillDataSO skillData)
    {
        if (skillData == null) return 0;
        if (_activeSkills.TryGetValue(skillData, out ISkill skill))
            return skill.CurrentLevel;

        return 0;
    }

    /// <summary>
    /// 특정 스킬 하나를 끄거나 켭니다.
    /// </summary>
    public void SetSkillActive(SkillDataSO skillData, bool isEnabled)
    {
        if (skillData == null) return;

        // 딕셔너리에 해당 스킬이 있는지 확인
        if (_activeSkills.TryGetValue(skillData, out ISkill skill))
        {
            // 인터페이스에 정의한 메서드 호출
            skill.SetSkillEnable(isEnabled);

            Debug.Log($"[SkillManager] {skillData.skillNameEN} 스킬 상태 변경: {isEnabled}");
        }
        else
        {
            Debug.LogWarning($"[SkillManager] 활성화된 스킬 목록에 {skillData.skillNameEN}가 없습니다.");
        }
    }

    /// <summary>
    /// 현재 배운 모든 스킬을 끄거나 켭니다.
    /// </summary>
    public void SetAllSkillsActive(bool isEnabled)
    {
        if (IsGlobalSkillEnabled == isEnabled) return;

        IsGlobalSkillEnabled = isEnabled;
        
        foreach (var skill in _activeSkills.Values)
        {
            skill.SetSkillEnable(isEnabled);
        }

        OnSkillStateChanged?.Invoke(isEnabled);

    }

    public void AddMana(int amount)
    {
        _manaCounter += amount;
        _manaCounter = Mathf.Clamp(_manaCounter, 0, _maxMana);
        currentMp = (int)_manaCounter;
        OnManaChanged.Invoke(currentMp);
    }
}

