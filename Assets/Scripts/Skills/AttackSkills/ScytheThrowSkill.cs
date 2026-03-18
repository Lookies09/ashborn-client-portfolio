using UnityEngine;

/// <summary>
/// 낫 던지기 스킬: 투사체 투척 (최대 레벨에서 방사형)
/// </summary>
public class ScytheThrowSkill : BaseSkill
{

    public override void Activate()
    {
        if (!IsSkillEnabled) return;

        if (!IsReady) return;

        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;        
        
        int projectileCount = levelData.projectileCount;
        float damage = DamageCalculator.Calculate(levelData.damage, _playerStats);

        Vector3 genPos = _playerTransform.position + Vector3.up + (_playerTransform.forward * 0.8f);

        // 최대 레벨이면 방사형으로 발사
        if (_currentLevel >= 5)
        {
            float angleStep = 360f / projectileCount;
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = i * angleStep;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * _playerTransform.forward;
                SpawnScythe(genPos, direction, damage);
            }
        }
        else
        {
            // 일반적으로는 플레이어가 바라보는 방향으로 발사
            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 direction = _playerTransform.forward;
                // 여러 발일 경우 약간씩 퍼지게
                if (projectileCount > 1)
                {
                    float spreadAngle = (i - (projectileCount - 1) * 0.5f) * 30f;
                    direction = Quaternion.Euler(0, spreadAngle, 0) * direction;
                }
                SpawnScythe(genPos, direction, damage);
            }
        }
        
        // 사운드 재생
        if (_skillData.castSound != null)
        {
            AudioSource.PlayClipAtPoint(_skillData.castSound, _playerTransform.position);
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

    private void SpawnScythe(Vector3 position, Vector3 direction, float damage)
    {
        GameObject scythe;
        
        if (ObjectPoolManager.Instance != null)
        {
            scythe = ObjectPoolManager.Instance.Spawn("Scythe", position, Quaternion.LookRotation(direction));
        }
        else
        {
            // 프리팹이 없으면 기본 구체 생성
            scythe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            scythe.transform.position = position;
            scythe.transform.localScale = Vector3.one * 0.5f;
            scythe.GetComponent<Collider>().isTrigger = true;
        }
        
        // 투사체 컴포넌트 추가
        ScytheProjectile proj = scythe.GetComponent<ScytheProjectile>();
        if (proj == null)
        {
            proj = scythe.AddComponent<ScytheProjectile>();
        }
        
        proj.Initialize(damage, direction, _skillData.hitEffectPrefab, _skillData.hitSound);
    }
}

/// <summary>
/// 낫 투사체
/// </summary>
public class ScytheProjectile : MonoBehaviour
{
    private float _damage;
    private Vector3 _direction;
    private float _speed = 8f;
    private float _lifetime = 5f;
    private float _spawnTime;
    private GameObject _hitEffectPrefab;
    private AudioClip _hitSound;
    private bool _hasHit;


    public void Initialize(float damage, Vector3 direction, GameObject hitEffect, AudioClip hitSound)
    {
        _damage = damage;
        _direction = direction;
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
        
        transform.position += _direction * _speed * Time.deltaTime;
        transform.Rotate(0, 1080f * Time.deltaTime, 0 );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        // 환경(벽/지형/오브젝트)에 닿으면 즉시 파괴
        if (other.CompareTag("Environment"))
        {
            if (_hitEffectPrefab != null)
                Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);

            if (_hitSound != null)
                AudioSource.PlayClipAtPoint(_hitSound, transform.position);

            ObjectPoolManager.Instance.Despawn(gameObject);

            _hasHit = true;
            return;
        }

        // 적군은 관통
        if (other.CompareTag("Enemy"))
        {
            IDamageable enemy = other.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(Mathf.RoundToInt(_damage));

                if (_hitEffectPrefab != null)
                    Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);

                if (_hitSound != null)
                    AudioSource.PlayClipAtPoint(_hitSound, transform.position);
            }

            return;
        }
    }

}

