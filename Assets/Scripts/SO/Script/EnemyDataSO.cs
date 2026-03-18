using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적군 공통 데이터를 담는 ScriptableObject.
/// 프리팹마다 이 자산을 참조해 스탯과 보상값을 공유합니다.
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "SO/Enemy/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string EnemyName = "Enemy";

    [Header("최대 체력 스탯")]
    public int MaxHP = 50;

    [Header("이동 스탯")]
    public float MaxMoveSpeed = 5f;
    public float WalkSpeed = 5f;
    public float RunSpeed = 5f;

    [Header("추격 및 탐지 스탯")]
    public float ChaseRange = 20f;
    public float DetectionRange = 8f;
    public float SuspiciousRange = 15f;
    public float SightAngl = 60f;

    [Header("가능한 공격 설정")]
    public float BaseAttackRange = 2f;
    public float GlobalAttackCooldown = 0.5f;
    [SerializeField] public List<EnemyAttackProfile> Attacks = new();

    [Header("보상")]
    public int ExpReward = 20;
    public int dropGold;

}


