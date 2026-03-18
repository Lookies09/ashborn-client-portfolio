using System;
using UnityEngine;
using static PlayerEnums;

public class PlayerHealth : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private PlayerRuntimeStats runtimeStat;
    [SerializeField] private int _currentHP = 0;

    public int CurrentHP => _currentHP;
    public bool IsDead => _currentHP <= 0;

    public event Action<int> OnDamaged;
    public event Action<int> OnHealed; 
    public event Action OnKilled;      
    public event Action<int> OnMaxHPChanged;
    public event Action OnRevived;

    private void Start()
    {
        if (runtimeStat == null) return;

        _currentHP = (int)runtimeStat.GetStat(PlayerStatType.MaxHP);

        OnHealed?.Invoke(0);
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        int previousHP = _currentHP;
        _currentHP = Mathf.Clamp(_currentHP + amount, 0, (int)runtimeStat.GetStat(PlayerStatType.MaxHP));
        
        if (_currentHP != previousHP)
        {
            OnHealed?.Invoke(amount);
        }
    }

    public void IncreaseMaxHP(int amount)
    {
        if (IsDead) return;
        runtimeStat.ModifyBaseStat(PlayerStatType.MaxHP, amount);
        OnMaxHPChanged?.Invoke((int)runtimeStat.GetStat(PlayerStatType.MaxHP));

        if (amount <= 0) return;
        Heal(amount);
    }

    public void Kill()
    {
        if (IsDead) return;
        _currentHP = 0;
        OnKilled?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        int previousHP = _currentHP;

        float def = Mathf.Clamp(runtimeStat.GetStat(PlayerStatType.Defense), 0f, 100f);

        amount -= Mathf.RoundToInt(def * amount / 100f);

        //Debug.Log($"플레이어 감소된 데미지: {Mathf.RoundToInt(def * amount / 100f)}");

        amount = Mathf.Max(amount, 0);
        _currentHP = Mathf.Clamp(_currentHP - amount, 0, (int)runtimeStat.GetStat(PlayerStatType.MaxHP));
        
        OnDamaged?.Invoke(amount);
        
        if (_currentHP <= 0 && previousHP > 0)
        {
            OnKilled?.Invoke();
        }
    }

    public void ResetHealth()
    {
        _currentHP = (int)runtimeStat.GetStat(PlayerStatType.MaxHP);
        OnHealed?.Invoke(0);
    }

    public void Revive(float healthPercentage = 1f)
    {
        _currentHP = Mathf.RoundToInt(runtimeStat.GetStat(PlayerStatType.MaxHP) * healthPercentage);
        OnHealed?.Invoke(0);
        OnRevived?.Invoke();
    }



    // ---------------------
    [ContextMenu("Test Heal 50")]
    private void TestHeal()
    {
        Heal(50);
        Debug.Log($"Test Heal: CurrentHP = {_currentHP}");
    }

    [ContextMenu("Test Damage 30")]
    private void TestDamage()
    {        
        TakeDamage(30);
        Debug.Log($"Test Damage: CurrentHP = {_currentHP}");
    }

    [ContextMenu("Test Kill")]
    private void TestKill()
    {
        Kill();
        Debug.Log($"Test Kill: IsDead = {IsDead}");
    }

    [ContextMenu("Test Increase MaxHP 20")]
    private void TestIncreaseMaxHP()
    {
        IncreaseMaxHP(20);
        Debug.Log($"Test IncreaseMaxHP: MaxHP = {(int)runtimeStat.GetStat(PlayerStatType.MaxHP)}, CurrentHP = {_currentHP}");
    }
    
}