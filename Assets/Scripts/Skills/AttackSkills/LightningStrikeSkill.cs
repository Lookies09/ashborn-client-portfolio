using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 번개 강타 스킬: 가까운 적들을 강타
/// </summary>
public class LightningStrikeSkill : BaseSkill
{
    public override void Activate()
    {
        if (!IsReady) return;
        
        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;
        
        //_lastActivateTime = Time.time;
        
        int targetCount = levelData.targetCount;
        float damage = DamageCalculator.Calculate(levelData.damage, _playerStats);
        float range = levelData.range * 0.01f * 10f; // 범위를 실제 거리로 변환
        
        // 가까운 적들 찾기
        Collider[] enemies = Physics.OverlapSphere(_playerTransform.position, range);
        List<Transform> enemyTransforms = new List<Transform>();
        
        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy") && col.GetComponent<IDamageable>() != null)
            {
                enemyTransforms.Add(col.transform);
            }
        }
        
        // 거리순으로 정렬
        enemyTransforms.Sort((a, b) => 
            Vector3.Distance(_playerTransform.position, a.position).CompareTo(
            Vector3.Distance(_playerTransform.position, b.position)));
        
        // 타겟 수만큼 공격
        int attackCount = Mathf.Min(targetCount, enemyTransforms.Count);
        for (int i = 0; i < attackCount; i++)
        {
            Transform target = enemyTransforms[i];
            StrikeEnemy(target, damage);
        }
        
        // 사운드 재생
        if (_skillData.castSound != null)
        {
            AudioSource.PlayClipAtPoint(_skillData.castSound, _playerTransform.position);
        }
    }
    
    private void StrikeEnemy(Transform enemy, float damage)
    {
        // 번개 이펙트 생성
        if (_skillData.skillPrefab != null)
        {
            GameObject lightning = Instantiate(_skillData.skillPrefab, enemy.position + Vector3.up * 2f, Quaternion.identity);
            Destroy(lightning, 1f);
        }
        
        // 데미지 적용
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            ApplyDamageToEnemy(damageable, damage);
        }
        
        // 타격 이펙트
        if (_skillData.hitEffectPrefab != null)
        {
            Instantiate(_skillData.hitEffectPrefab, enemy.position, Quaternion.identity);
        }
        
        // 사운드
        if (_skillData.hitSound != null)
        {
            AudioSource.PlayClipAtPoint(_skillData.hitSound, enemy.position);
        }
        
        // 디버그용 라인
        Debug.DrawLine(_playerTransform.position + Vector3.up, enemy.position + Vector3.up, Color.yellow, 0.5f);
    }
}

