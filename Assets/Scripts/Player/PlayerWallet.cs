using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    public event Action OnGoldChanged;
    public event Action<int> OnTempGoldChanged;

    public int Gold { get; private set; }
    public int TempGold { get; private set; }
    public int RewardedGold => (int)(TempGold * 0.5f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void AddTempGold(int amount)
    {
        TempGold += amount;
        OnTempGoldChanged?.Invoke(TempGold);
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke();
        SoundManager.Instance.PlayGoldChangeSound();
    }

    public void EndStage(bool isCleared, bool watchedAd = false)
    {
        if (isCleared)
        {
            int finalReward = TempGold;

            if (watchedAd)
            {
                finalReward += RewardedGold;
            }

            Gold += finalReward;
            OnGoldChanged?.Invoke();
        }

        // 다음 판을 위해 초기화
        TempGold = 0;
        OnTempGoldChanged?.Invoke(TempGold);
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount)
        {
            SoundManager.Instance.NotEnoughGoldSound();
            return false;
        }
        Gold -= amount;
        OnGoldChanged?.Invoke();
        SoundManager.Instance.PlayGoldChangeSound();
        return true;
    }

    public void ResetGold()
    {
        Gold = 0;
        OnGoldChanged?.Invoke();
    }

    public void RefreshGold()
    {
        OnGoldChanged?.Invoke();
    }

    [ContextMenu("GOld + 100000")]
    public void DebugAddGold()
    {
        Gold += 100000;
        OnGoldChanged?.Invoke();
    }
}

