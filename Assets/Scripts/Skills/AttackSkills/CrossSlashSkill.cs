using UnityEngine;

/// <summary>
/// 십문자 베기 스킬: 플레이어가 바라보는 방향으로 2연 공격
/// </summary>
public class CrossSlashSkill : BaseSkill
{
    private float _attackDelay = 0.1f; // 2연 공격 사이의 딜레이

    public override void Activate()
    {
        if (!IsSkillEnabled) return;
        if (!IsReady) return;
        
        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;
                
        // 첫 번째 공격
        PerformSlash(0f, true);
        
        // 두 번째 공격 (약간의 딜레이 후)
        Invoke(nameof(PerformSecondSlash), _attackDelay);
        
        // 사운드 재생
        if (_skillData.castSound != null)
        {
            AudioSource.PlayClipAtPoint(_skillData.castSound, _playerTransform.position);
        }

        timer = 0f;
    }
    
    private void PerformSecondSlash()
    {
        PerformSlash(0f, false);

    }

    public override void UpdateSkill(float deltaTime)
    {
        base.UpdateSkill(deltaTime);

        if (!IsSkillEnabled) return;

        if (IsReady)
        {
            Activate();
        }
    }

    private void PerformSlash(float delay, bool isFirstAttack)
    {
        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        float range = levelData.range;
        float damage = DamageCalculator.Calculate(levelData.damage, _playerStats);

        // 공격 중앙 위치
        Vector3 center = _playerTransform.position +
                         _playerTransform.forward * (range * 0.5f) +
                         Vector3.up * 0.5f;       

        // 공격 반경
        float radius = range * 0.5f;

        if (_skillData.skillPrefab)
        {
            Vector3 spawnPos = center + Vector3.up * 1f;

            Quaternion baseRot = Quaternion.LookRotation(_playerTransform.forward);
            GameObject skillEffect = ObjectPoolManager.Instance.Spawn("SlashEffect", spawnPos, baseRot);
            skillEffect.transform.localScale = new Vector3(radius, radius, radius);
            float zRot = isFirstAttack ? 100f : -100f;            
            skillEffect.transform.Rotate(0f, 0f, zRot, Space.Self);
        }


        // 해당 영역 안의 모든 콜라이더 검색
        Collider[] hits = Physics.OverlapSphere(center, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            IDamageable enemy = hit.GetComponent<IDamageable>();
            if (enemy != null)
            {
                // 데미지 적용
                ApplyDamageToEnemy(enemy, damage);

                // 타격 이펙트
                if (_skillData.hitEffectPrefab != null)
                {
                    Instantiate(
                        _skillData.hitEffectPrefab,
                        hit.ClosestPoint(center),
                        Quaternion.LookRotation(_playerTransform.forward) // 베는 방향
                    );
                }
            }
        }
               

    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        float range = levelData.range;
        float radius = range * 0.5f;

        Vector3 center = _playerTransform.position +
                         _playerTransform.forward * (range * 0.5f) +
                         Vector3.up * 0.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, radius);
    }


}

