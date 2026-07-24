using UnityEngine;

/// <summary>
/// 정령 불꽃 스킬: 가까운 적을 추적하는 투사체 공격
/// </summary>
public class SpiritFlameSkill : BaseSkill
{

    public override void Activate()
    {
        if (!IsSkillEnabled) return;

        if (!IsReady) return;

        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        int projectileCount = levelData.projectileCount;
        float damage = DamageCalculator.Calculate(levelData.damage, _playerStats);

        // 플레이어 기준 Z축으로 0.8, Y축 1 올림
        Vector3 spawnPosition = _playerTransform.position + Vector3.up + (_playerTransform.forward * 0.8f); 

         

        // 가까운 적 찾기
        Collider[] enemies = Physics.OverlapSphere(_playerTransform.position, 20f);
        System.Collections.Generic.List<Transform> enemyTransforms = new System.Collections.Generic.List<Transform>();

        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy") && col.GetComponent<IDamageable>() != null && !col.GetComponent<IHealth>().IsDead)
            {
                enemyTransforms.Add(col.transform);
            }
        }

        // 적이 없으면 플레이어 앞방향으로 발사
        if (enemyTransforms.Count == 0)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                SpawnProjectile(spawnPosition, _playerTransform.forward, damage, null);
            }
        }
        else
        {
            // 가까운 적들을 거리순으로 정렬
            enemyTransforms.Sort((a, b) =>
                Vector3.Distance(_playerTransform.position, a.position)
                .CompareTo(Vector3.Distance(_playerTransform.position, b.position)));

            // 투사체 발사
            for (int i = 0; i < projectileCount && i < enemyTransforms.Count; i++)
            {
                Vector3 direction = (enemyTransforms[i].position - spawnPosition).normalized;
                SpawnProjectile(spawnPosition, direction, damage, enemyTransforms[i]);
            }

            // 남은 투사체는 랜덤 방향으로
            for (int i = enemyTransforms.Count; i < projectileCount; i++)
            {
                Vector3 randomDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;
                SpawnProjectile(spawnPosition, randomDirection, damage, null);
            }
        }

        // 사운드 재생
        if (_skillData.castSound != null)
        {
            AudioSource.PlayClipAtPoint(_skillData.castSound, _playerTransform.position + Vector3.up);
        }

        timer = 0f;
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

    private void SpawnProjectile(Vector3 position, Vector3 direction, float damage, Transform target)
    {
        GameObject projectile;

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.Spawn("FireballCast", position, Quaternion.LookRotation(direction));
            projectile = ObjectPoolManager.Instance.Spawn("Fireball", position, Quaternion.LookRotation(direction));
        }
        else
        {
            // 프리팹이 없으면 기본 구체 생성
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = position;
            projectile.transform.localScale = Vector3.one * 0.3f;
            projectile.GetComponent<Collider>().isTrigger = true;
        }
        
        // 투사체 컴포넌트 추가
        SpiritFlameProjectile proj = projectile.GetComponent<SpiritFlameProjectile>();
        if (proj == null)
        {
            proj = projectile.AddComponent<SpiritFlameProjectile>();
        }
        
        proj.Initialize(damage, direction, target, _skillData.hitEffectPrefab, _skillData.hitSound);
    }
}

/// <summary>
/// 정령 불꽃 투사체
/// </summary>
public class SpiritFlameProjectile : MonoBehaviour
{
    private float _damage;
    private Vector3 _direction;
    private Transform _target;
    private float _speed = 10f;
    private float _lifetime = 3f;
    private GameObject _hitEffectPrefab;
    private AudioClip _hitSound;
    private float _spawnTime;
    private bool _hasHit;

    public void Initialize(float damage, Vector3 direction, Transform target, GameObject hitEffect, AudioClip hitSound)
    {
        _damage = damage;
        _direction = direction;
        _target = target;
        _hitEffectPrefab = hitEffect;
        _hitSound = hitSound;
        _spawnTime = Time.time;
        _hasHit = false;
    }
    
    private void Update()
    {
        if (Time.time - _spawnTime > _lifetime)
        {
            ObjectPoolManager.Instance.Despawn(gameObject);
            return;
        }

        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            Vector3 targetPos = _target.position + Vector3.up * 1f;
            _direction = (targetPos - transform.position).normalized;
        }

        transform.position += _direction * _speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(_direction);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        if (other.CompareTag("Enemy"))
        {
            IDamageable enemy = other.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(Mathf.RoundToInt(_damage));
                
                if (ObjectPoolManager.Instance != null)
                {
                    ObjectPoolManager.Instance.Spawn("FireballHit", transform.position, Quaternion.identity);
                    
                }
                
                if (_hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(_hitSound, transform.position);
                }
                _hasHit = true;
                ObjectPoolManager.Instance.Despawn(gameObject);
            }
        }

        if (other.CompareTag("Environment"))
        {
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Spawn("FireballHit", transform.position, Quaternion.identity);
                
            }

            if (_hitSound != null)
            {
                AudioSource.PlayClipAtPoint(_hitSound, transform.position);
            }
            ObjectPoolManager.Instance.Despawn(gameObject);
            _hasHit = true;
        }
    }
}

