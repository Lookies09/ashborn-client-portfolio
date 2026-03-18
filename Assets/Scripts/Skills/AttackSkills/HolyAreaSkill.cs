using UnityEngine;

/// <summary>
/// 신성 영역 스킬: 넓은 범위의 지속 데미지 (패시브)
/// </summary>
public class HolyAreaSkill : BaseSkill
{
    private GameObject _areaEffect;
    private float _damageInterval = 1f;
    private float _lastDamageTime = 0f;
    private float _baseRange = 1f;

    public override void Activate()
    {
        // 패시브 스킬이므로 Activate는 한 번만 호출되어 영역 생성
        if (_areaEffect != null) return;

        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        // 영역 이펙트 생성
        if (_skillData.skillPrefab != null)
        {
            _areaEffect = Instantiate(_skillData.skillPrefab, _playerTransform.position, Quaternion.identity);
            _areaEffect.transform.SetParent(_playerTransform, worldPositionStays: true);
            _areaEffect.AddComponent<HollyArea>();
        }
        else
        {
            // 프리팹이 없으면 기본 원형 생성
            _areaEffect = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _areaEffect.transform.SetParent(_playerTransform);
            _areaEffect.transform.localPosition = Vector3.zero;
            _areaEffect.GetComponent<Collider>().isTrigger = true;

            // 투명하게 만들기
            Renderer renderer = _areaEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                Color color = mat.color;
                color.a = 0.3f;
                mat.color = color;
            }
        }

        UpdateAreaSize();
    }

    public override void UpdateSkill(float deltaTime)
    {
        base.UpdateSkill(deltaTime);

        if (!IsSkillEnabled) return;

        if (_areaEffect == null) return;

        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        // 주기적으로 데미지 적용
        if (Time.time >= _lastDamageTime + _damageInterval)
        {
            _lastDamageTime = Time.time;
            ApplyAreaDamage(levelData);
        }
    }

    private void ApplyAreaDamage(SkillLevelData levelData)
    {
        float range = _baseRange * levelData.range;
        float damage = DamageCalculator.Calculate(levelData.damage, _playerStats);

        Collider[] enemies = Physics.OverlapSphere(_playerTransform.position, range);

        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy"))
            {
                IDamageable enemy = col.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    ApplyDamageToEnemy(enemy, damage);

                    // 타격 이펙트 (간헐적으로)
                    if (_skillData.hitEffectPrefab != null && Random.value < 0.3f)
                    {
                        Instantiate(_skillData.hitEffectPrefab, col.transform.position, Quaternion.identity);
                    }
                }
            }
        }
    }

    public override void LevelUp(int newLevel)
    {
        base.LevelUp(newLevel);
        UpdateAreaSize();
    }

    private void UpdateAreaSize()
    {
        if (_areaEffect == null) return;
        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        float range = _baseRange * levelData.range;
        _areaEffect.transform.localScale = new Vector3(range * 2f, _areaEffect.transform.localScale.y, range * 2f);

    }

    public override void Cleanup()
    {
        if (_areaEffect != null)
        {
            Destroy(_areaEffect);
        }
        base.Cleanup();
    }

    protected override void OnSkillDisabled()
    {
        // 스킬이 꺼지면 이펙트(장판)를 비활성화
        if (_areaEffect != null)
        {
            _areaEffect.SetActive(false);
        }
    }

    protected override void OnSkillEnabled()
    {
        if (_areaEffect == null)
        {
            Activate(); // 최초 생성 로직
        }
        else
        {
            _areaEffect.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_playerTransform == null) return;

        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        float range = _baseRange * levelData.range;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(_playerTransform.position, range);
    }

}


public class HollyArea : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward);
    }
}
