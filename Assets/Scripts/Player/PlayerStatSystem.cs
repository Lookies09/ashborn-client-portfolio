using System;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEnums;


public class PlayerStatSystem : MonoBehaviour
{

    [SerializeField] private PlayerStatsSO _playerStatsSO;
    [SerializeField] private CurrentPlayerStateLevelSO _currentPlayerStateLevelSO;
    [SerializeField] private PlayerStatUpgradeConfigSO _playerStatUpgradeConfigSO;

    [SerializeField] private PlayerRuntimeStats _PlayerRuntimeStats;

    private Dictionary<PlayerEnums.PlayerStatType, System.Func<float>> statGetters;


    public readonly int MAX_HP_LIMIT = 9999;
    public readonly int MAX_ATTACK_LIMIT = 999;
    public readonly float MAX_MOVESPEED_LIMIT = 15f;

    public readonly float MAX_CRITRATE_LIMIT = 1f;
    public readonly float MAX_CRITDAMAGE_LIMIT = 5f;

    public readonly int MAX_DEFENSE_LIMIT = 500;
    public readonly float MAX_LUCK_LIMIT = 0.4f;
    public readonly float MAX_CDR_LIMIT = 0.7f;
    public readonly float MAX_REGEN_LIMIT = 30f;

    private void Awake()
    {
        InitDictionaries();

        if (_PlayerRuntimeStats == null)
        {
            _PlayerRuntimeStats.GetComponent<PlayerRuntimeStats>();
        }

        _PlayerRuntimeStats.Init(this);
    }

    private void InitDictionaries()
    {
        statGetters = new Dictionary<PlayerStatType, Func<float>>
        {
            { PlayerStatType.MaxHP, () => _playerStatsSO.maxHP + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.MaxHP).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.MaxHP)) },
            { PlayerStatType.Attack, () => _playerStatsSO.attack + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.Attack).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.Attack))},
            { PlayerStatType.MoveSpeed, () => _playerStatsSO.moveSpeed + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.MoveSpeed).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.MoveSpeed))},
            { PlayerStatType.CritRate, () => _playerStatsSO.critRate + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.CritRate).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.CritRate))},
            { PlayerStatType.CritDamage, () => _playerStatsSO.critDamage + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.CritDamage).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.CritDamage))},
            { PlayerStatType.Defense, () => _playerStatsSO.defense + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.Defense).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.Defense))},
            { PlayerStatType.Luck, () => _playerStatsSO.luck + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.Luck).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.Luck)) },
            { PlayerStatType.CooldownReduction, () => _playerStatsSO.cooldownReduction + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.CooldownReduction).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.CooldownReduction))},
            { PlayerStatType.Regen, () => _playerStatsSO.regenPerSecond + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.Regen).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.Regen))},
            { PlayerStatType.MaxMp, () => _playerStatsSO.maxMp + _playerStatUpgradeConfigSO.GetConfig(PlayerStatType.MaxMp).GetTotalValueIncreaseByLevels(0, _currentPlayerStateLevelSO.GetCurrentLevel(PlayerStatType.MaxMp))}
        };
    }

    public float GetStat(PlayerStatType statType)
    {
        if (statGetters.TryGetValue(statType, out var getter))
            return getter();
        Debug.LogWarning($"Getter not found for stat {statType}");
        return 0f;
    }

}
