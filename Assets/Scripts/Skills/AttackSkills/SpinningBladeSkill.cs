using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

/// <summary>
/// 회전 톱날 스킬: 플레이어 주변을 회전하는 톱날들
/// </summary>
public class SpinningBladeSkill : BaseSkill
{
    private List<GameObject> _blades = new List<GameObject>();
    private float _rotationSpeed = 180f; // 초당 회전 각도

    public override void Activate()
    {
        if (_blades.Count > 0)
        {
            UpdateBlades();
            return;
        }

        SkillLevelData levelData = GetCurrentLevelData();
        if (levelData == null) return;

        CreateBlades(levelData);
    }


    private void CreateBlades(SkillLevelData levelData)
    {
        int bladeCount = levelData.projectileCount;
        float radius = 2f; // 플레이어로부터의 거리
        
        for (int i = 0; i < bladeCount; i++)
        {
            float angle = (360f / bladeCount) * i;

            Vector3 basePos = _playerTransform.position + Vector3.up * 1;
            Vector3 position = basePos + 
                Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;

            
            GameObject blade;
            if (_skillData.skillPrefab != null)
            {
                blade = Instantiate(_skillData.skillPrefab, position, Quaternion.identity);
            }
            else
            {
                blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.transform.localScale = Vector3.one * 0.5f;
                blade.GetComponent<Collider>().isTrigger = true;
            }
            
            blade.transform.SetParent(_playerTransform);
            SpinningBlade bladeComponent = blade.GetComponent<SpinningBlade>();
            if (bladeComponent == null)
            {
                bladeComponent = blade.AddComponent<SpinningBlade>();
            }
            
            bladeComponent.Initialize(
                levelData.damage,
                radius,
                angle,
                _rotationSpeed,
                _skillData.hitEffectPrefab,
                _skillData.hitSound
                , _playerStats
            );
            
            _blades.Add(blade);
        }
    }
    
    public override void LevelUp(int newLevel)
    {
        base.LevelUp(newLevel);
        UpdateBlades();
    }

    private void UpdateBlades()
    {
        SkillLevelData levelData = GetCurrentLevelData();
        int requiredCount = levelData.projectileCount;
        float baseRadius = 1.5f;
        float radius = baseRadius * levelData.range;

        // 부족하면 생성
        while (_blades.Count < requiredCount)
        {
            _blades.Add(CreateBladeInstance());
        }

        for (int i = 0; i < _blades.Count; i++)
        {
            bool isRequired = i < requiredCount;
            GameObject blade = _blades[i];

            if (isRequired && IsSkillEnabled)
            {
                float angle = (360f / requiredCount) * i;

                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
                //blade.transform.position = _playerTransform.position + offset;
                //blade.transform.rotation = Quaternion.Euler(0, angle + 90f, 0);

                SpinningBlade comp = blade.GetComponent<SpinningBlade>();
                comp.Initialize(
                    levelData.damage,
                    radius,
                    angle,
                    _rotationSpeed,
                    _skillData.hitEffectPrefab,
                    _skillData.hitSound,
                    _playerStats
                );

                blade.SetActive(true);
            }
            else
            {
                blade.SetActive(false);
            }
        }
    }

    private GameObject CreateBladeInstance()
    {
        GameObject blade;

        if (_skillData.skillPrefab != null)
            blade = Instantiate(_skillData.skillPrefab);
        else
            blade = GameObject.CreatePrimitive(PrimitiveType.Cube);

        blade.transform.SetParent(_playerTransform);
        blade.SetActive(false);

        if (!blade.TryGetComponent(out SpinningBlade comp))
            comp = blade.AddComponent<SpinningBlade>();

        return blade;
    }


    public override void Cleanup()
    {
        foreach (var blade in _blades)
        {
            if (blade != null)
                blade.SetActive(false);
        }
    }

    protected override void OnSkillDisabled()
    {
        foreach (var blade in _blades)
        {
            if (blade != null)
            {
                blade.SetActive(false);
            }
        }
    }

    protected override void OnSkillEnabled()
    {
        UpdateBlades();
    }

   
}

/// <summary>
/// 회전하는 톱날
/// </summary>
public class SpinningBlade : MonoBehaviour
{
    private float _baseDamage;
    private float _radius;
    private float _angle;
    private float _rotationSpeed;
    private Transform _playerTransform;
    private GameObject _hitEffectPrefab;
    private AudioClip _hitSound;
    private PlayerRuntimeStats _playerStats;
    
    public void Initialize(float baseDamage, float radius, float startAngle, float rotationSpeed, 
        GameObject hitEffect, AudioClip hitSound, PlayerRuntimeStats stats)
    {
        _baseDamage = baseDamage;
        _radius = radius;
        _angle = startAngle;
        _rotationSpeed = rotationSpeed;
        _hitEffectPrefab = hitEffect;
        _hitSound = hitSound;
        _playerTransform = transform.parent;
        _playerStats = stats;
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        
        // 각도 업데이트
        _angle += _rotationSpeed * Time.deltaTime;
        if (_angle >= 360f) _angle -= 360f;
        
        // 위치 업데이트
        Vector3 offset = Quaternion.Euler(0, _angle, 0) * Vector3.forward * _radius;
        transform.position = _playerTransform.position + Vector3.up + offset;
        transform.rotation = Quaternion.Euler(0, _angle + 90f, 0); // 톱날이 회전하는 방향으로
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IDamageable enemy = other.GetComponent<IDamageable>();
            if (enemy != null)
            {
                float finalDamage = DamageCalculator.Calculate(_baseDamage, _playerStats);

                enemy.TakeDamage(Mathf.RoundToInt(finalDamage));

                if (_hitEffectPrefab != null)
                {
                    Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
                }

                if (_hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(_hitSound, transform.position);
                }            

            }
        }
    }
}

