using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;

/// <summary>
/// 적군 공격: 플레이어가 공격 범위 내에 있으면 공격
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    private EnemyController _enemyController;
    private EnemyAttackProfile _currentAttackProfile;
    private int _currentAttackIndex = -1;
    private float[] _lastAttackTimes = System.Array.Empty<float>();
    private float _lastGlobalAttackTime = -999f;

    private Dictionary<AttackType, BaseStrategy> _strategies = new Dictionary<AttackType, BaseStrategy>();

    [SerializeField] private BaseStrategy[] attackStrategys;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();

        if(attackStrategys != null)
        {
            foreach (var s in attackStrategys)
            {
                _strategies[s.Type] = s;
            }
        }
        
    }

    // 공격 애니메이션 이벤트
    public void OnAttackEvent()
    {
        if (_strategies.TryGetValue(_currentAttackProfile.type, out var strategy))
            strategy.Execute(_currentAttackProfile);
    }
           

    public bool IsCooldownReady(int attackIndex, float cooldown)
    {
        EnsureCooldownSize(attackIndex + 1);
        return Time.time - _lastAttackTimes[attackIndex] >= cooldown;
    }

    public bool IsGlobalCooldownReady(float globalCooldown)
    {
        return Time.time - _lastGlobalAttackTime >= globalCooldown;
    }

    public void MarkAttackUsed(int attackIndex)
    {
        EnsureCooldownSize(attackIndex + 1);
        _lastAttackTimes[attackIndex] = Time.time;
    }

    public void MarkGlobalUsed()
    {
        _lastGlobalAttackTime = Time.time;
    }    

    public void SetCurrentAttackProfile(EnemyAttackProfile p, int index)
    {
        _currentAttackProfile = p;
        _currentAttackIndex = index;
    }

    private void EnsureCooldownSize(int size)
    {
        if (_lastAttackTimes.Length >= size) return;
        System.Array.Resize(ref _lastAttackTimes, size);
    }

}

