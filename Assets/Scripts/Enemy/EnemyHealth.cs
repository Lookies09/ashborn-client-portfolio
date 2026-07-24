using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 적군 체력 시스템: 플레이어와 동일한 인터페이스 사용
/// </summary>
public class EnemyHealth : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHP = 50;
    [SerializeField] private int _currentHP;
    private EnemyController _enemyController;

    public int CurrentHP => _currentHP;
    public bool IsDead => _currentHP <= 0;

    // 이벤트
    public event System.Action OnKilled;
    public event System.Action OnDead;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
        maxHP = _enemyController.Data.MaxHP;
    }

    private void Start()
    {
        _currentHP = maxHP;        
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        _currentHP = Mathf.Clamp(_currentHP + amount, 0, maxHP);
    }

    public void IncreaseMaxHP(int amount)
    {
        if (IsDead) return;
        maxHP += amount;
        Heal(amount);
    }

    public void Kill()
    {
        if (IsDead) return;
        _currentHP = 0;
        OnKilled?.Invoke();
        OnDead?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        _currentHP = Mathf.Clamp(_currentHP - amount, 0, maxHP);

        var agent = _enemyController.BTAgent;

        if (agent.GetVariable<OnDamage>("OnDamaged", out var bbVar))
        {
            OnDamage channel = bbVar.Value;
            channel.SendEventMessage(_enemyController.gameObject);
        }
        else
        {
            Debug.Log("채널 가져오기 실패");
        }



        if (_currentHP <= 0)
        {
            OnDead?.Invoke();
            if (agent.GetVariable<OnDead>("OnDead", out var v))
            {
                OnDead channel = v.Value;
                channel.SendEventMessage();
            }
        }
    }

    public void ResetHealth()
    {
        _currentHP = maxHP;
    }

    public void Revive(float healthPercentage = 1f)
    {
        _currentHP = Mathf.RoundToInt(maxHP * healthPercentage);
    }

}

