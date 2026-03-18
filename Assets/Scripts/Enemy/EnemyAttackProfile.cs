using System;
using UnityEngine;

[Serializable]
public class EnemyAttackProfile
{
    public AttackType type;

    // 근접공격 OverlapSphere 설정
    public float meleeRadius = 1.2f;
    public LayerMask targetMask;

    // 영역 공격 범위
    public float range = 1.5f;

    public float cooldown = 2f;
    public int damage = 10;

    // 애니메이션 재생 시간(초). 0 이하이면 클립 길이를 사용.
    public float animationDuration = 0f;

    public string animationName;

    // 투사체 공격 설정
    public GameObject projectilePrefab;
    public float projectileLifeTime = 3f;
    public float projectileSpeed = 10f;

    public GameObject castEffect;
    public GameObject hitEffect;
    public AudioClip hitSound;
    

    public GameObject aoePrefab;
    public float aoeRadius = 3f;
    public float aoeDuration = 2f;

    public GameObject summonPrefab;
    public int summonCount = 1;

    public int comboCount = 3;
    public float comboInterval = 0.4f;

    public float weight = 1f;      // 가중치
    public float minDistance = 0f; // 최소 공격거리
    public float maxDistance = 99f;
}