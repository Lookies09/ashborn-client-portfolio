using System;
using Unity.Services.LevelPlay;
using UnityEngine;
using static AdEnums;

public class RewardedAdHandler : MonoBehaviour
{
    [SerializeField] private string rewardedAdUnitId = "REWARDED_AD_UNIT_ID";

    private LevelPlayRewardedAd rewardedAd;
    private bool isInitialized;
    private bool rewardGranted;
    private AdState state = AdState.None;

    public event Action OnRewardGranted;

    public void Init()
    {
        if (isInitialized) return;
        isInitialized = true;

        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
        RegisterEvents();
        Load();
    }


    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        rewardedAd.OnAdLoaded += OnLoaded;
        rewardedAd.OnAdLoadFailed += OnLoadFailed;
        rewardedAd.OnAdDisplayed += OnDisplayed;
        rewardedAd.OnAdDisplayFailed += OnDisplayFailed;
        rewardedAd.OnAdRewarded += OnRewarded;
        rewardedAd.OnAdClosed += OnClosed;
    }

    private void UnregisterEvents()
    {
        if (rewardedAd == null) return;

        rewardedAd.OnAdLoaded -= OnLoaded;
        rewardedAd.OnAdLoadFailed -= OnLoadFailed;
        rewardedAd.OnAdDisplayed -= OnDisplayed;
        rewardedAd.OnAdDisplayFailed -= OnDisplayFailed;
        rewardedAd.OnAdRewarded -= OnRewarded;
        rewardedAd.OnAdClosed -= OnClosed;
    }

    private void Load()
    {
        if (state == AdState.Loading || state == AdState.Showing)
            return;

        state = AdState.Loading;
        rewardedAd.LoadAd();
    }

    public void Show()
    {
        if (!CanShow())
            return;

        state = AdState.Showing;
        rewardedAd.ShowAd();
    }

    public bool CanShow()
    {
        return rewardedAd != null
            && rewardedAd.IsAdReady()
            && state == AdState.Ready;
    }


    // =========================
    // Callbacks
    // =========================
    private void OnLoaded(LevelPlayAdInfo adInfo)
    {
        state = AdState.Ready;
        Debug.Log("Rewarded Loaded");
    }

    private void OnLoadFailed(LevelPlayAdError error)
    {
        state = AdState.Failed;
        Invoke(nameof(Load), 5f);
    }

    private void OnDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Displayed");
    }

    private void OnDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        state = AdState.Failed;
        Debug.LogError($"Rewarded Display Failed: {error}");
    }

    private void OnRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        if (rewardGranted) return;
        rewardGranted = true;

        OnRewardGranted?.Invoke();
    }

    private void OnClosed(LevelPlayAdInfo adInfo)
    {
        rewardGranted = false;
        state = AdState.None;
        Load();
    }
}
