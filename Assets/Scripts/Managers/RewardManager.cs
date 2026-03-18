using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("Runtime Modifier")]
    private PlayerRuntimeStats _RuntimeStats;
    private PlayerWallet _playerWallet;
    private PlayerProgress _playerProgress;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _RuntimeStats = FindFirstObjectByType<PlayerRuntimeStats>();
        _playerWallet = FindFirstObjectByType<PlayerWallet>();
        _playerProgress = FindFirstObjectByType<PlayerProgress>();
    }

    public void Grant(EnemyDataSO enemyData)
    {
        if (enemyData == null) return;

        GrantGold(enemyData.dropGold);
        GrantExp(enemyData.ExpReward);
    }

    private void GrantGold(int baseGold)
    {
        if (baseGold <= 0) return;

        int finalGold = Mathf.RoundToInt(baseGold * _RuntimeStats.IngameMoneyBonusRate);
        _playerWallet.AddTempGold(finalGold);
    }

    private void GrantExp(int baseExp)
    {
        if (baseExp <= 0) return;
        int finalExp = Mathf.RoundToInt(baseExp * _RuntimeStats.IngameExpBonusRate);

        _playerProgress.AddExp(finalExp);
    }

}
