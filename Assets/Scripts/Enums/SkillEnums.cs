using UnityEngine;

public static class SkillEnums
{
    public enum SkillType
    {
        Attack,
        Buff,
        Utill
    }

    public enum SkillUsage
    {
        Common,
        Unique
    }

    public enum SkillActivationType
    {
        Active,
        Passive
    }

    /// <summary>
    /// 스킬 구현 타입 - 어떤 클래스를 사용할지 결정
    /// </summary>
    public enum SkillImplementationType
    {
        // 공격형
        CrossSlash,      // 십문자 베기
        SpiritFlame,     // 정령 불꽃
        HolyArea,        // 신성 영역
        ScytheThrow,     // 낫 던지기
        SpinningBlade,   // 회전 톱날
        LightningStrike, // 번개 강타

        // 버프형
        AttackBoost,           // 공격력 증가
        CooldownReduction,     // 쿨타임 감소
        MovementSpeedUp,       // 이동 속도 증가
        MaxHPBoost,            // 최대 체력 강화
        DefenseBoost,          // 방어력 강화
        Regeneration,          // 지속 회복
        CriticalRateUp,        // 치명타 확률 강화
        CriticalDamageUp,      // 치명타 데미지 강화

        // 유틸형
        GoldBonus,            // 골드 증가
        ExpBonus              // 경험치 증가
    }
}
