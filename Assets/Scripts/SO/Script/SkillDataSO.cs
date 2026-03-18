using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skills/SkillData")]
public class SkillDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public Sprite skillIcon;
    public string skillNameKR;
    public string skillNameJP;
    public string skillNameEN;

    [Header("스킬 레벨 설명 템플릿")]
    public string descriptionFormatKR;
    public string descriptionFormatEN;
    public string descriptionFormatJP;

    [Header("스킬 타입")]
    public SkillEnums.SkillType skillType = SkillEnums.SkillType.Attack;
    public SkillEnums.SkillUsage usage = SkillEnums.SkillUsage.Common;
    public SkillEnums.SkillActivationType activationType = SkillEnums.SkillActivationType.Active;
    
    [Header("레벨별 스탯")]
    public List<SkillLevelData> levelData = new List<SkillLevelData>();
    
    [Header("스킬 효과 설명")]
    [TextArea(2, 4)]
    public string skillDescription;
    
    [Header("스킬 구현 타입")]
    public SkillEnums.SkillImplementationType implementationType;
    
    [Header("프리팹/이펙트")]
    public GameObject skillPrefab; // 투사체나 이펙트 프리팹
    public GameObject hitEffectPrefab; // 타격 이펙트
    public GameObject castEffectPrefab; // 시전 이펙트

    [Header("오디오")]
    public AudioClip castSound;
    public AudioClip hitSound;

    public string GetFormattedDescription(int level, string lang = "KR")
    {
        SkillLevelData data = levelData[level - 1];        

        string template = descriptionFormatKR;

        return template
            .Replace("{damage}", (data.damage * 100f).ToString())
            .Replace("{cooldown}", data.cooldown.ToString("0.0"))
            .Replace("{range}", data.range.ToString())
            .Replace("{projectile}", data.projectileCount.ToString())
            .Replace("{duration}", data.duration.ToString())
            .Replace("{percent}", (data.percentageValue * 100f).ToString())
            .Replace("{value}", data.absoluteValue.ToString());
    }
}

[System.Serializable]
public class SkillLevelData
{
    public int level;
    public float damage; // 기본 데미지에 대한 배율(예: 5% = 0.05)
    public float cooldown;
    public float range; // 범위 (퍼센트 또는 절대값)
    public int projectileCount; // 투사체 개수
    public float duration; // 지속 시간
    public int targetCount; // 타겟 수
    
    // 버프/유틸 스킬용 필드
    public float percentageValue; // 퍼센트 증가값 (예: 5% = 0.05)
    public float absoluteValue; // 절대값 증가 (예: 최대 체력 +20)

    
}


