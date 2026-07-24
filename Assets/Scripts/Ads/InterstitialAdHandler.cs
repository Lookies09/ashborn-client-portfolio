using System;
using Unity.Services.LevelPlay;
using UnityEngine;
using static AdEnums;

public class InterstitialAdHandler : MonoBehaviour
{
    [SerializeField] private string interstitialAdUnitId = "INTERSTITIAL_AD_UNIT_ID";

    private LevelPlayInterstitialAd interstitialAd;
    private bool isInitialized;
    private AdEnums.AdState state = AdEnums.AdState.None;

    public event Action OnClosedCallback;


    public void Init()
    {
        if (isInitialized) return;
        isInitialized = true;

        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
        RegisterEvents();
        Load();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        interstitialAd.OnAdLoaded += OnLoaded;
        interstitialAd.OnAdLoadFailed += OnLoadFailed;
        interstitialAd.OnAdDisplayed += OnDisplayed;
        interstitialAd.OnAdDisplayFailed += OnDisplayFailed;
        interstitialAd.OnAdClosed += OnClosed;
    }

    private void UnregisterEvents()
    {
        if (interstitialAd == null) return;

        interstitialAd.OnAdLoaded -= OnLoaded;
        interstitialAd.OnAdLoadFailed -= OnLoadFailed;
        interstitialAd.OnAdDisplayed -= OnDisplayed;
        interstitialAd.OnAdDisplayFailed -= OnDisplayFailed;
        interstitialAd.OnAdClosed -= OnClosed;
    }

    private void Load()
    {
        if (state == AdState.Loading || state == AdState.Showing)
            return;

        state = AdState.Loading;
        interstitialAd.LoadAd();
    }

    public void Show()
    {
        if (!CanShow())
            return;

        state = AdState.Showing;
        interstitialAd.ShowAd();
    }

    public bool CanShow()
    {
        return interstitialAd != null
            && interstitialAd.IsAdReady()
            && state == AdState.Ready;
    }


    // =========================
    // Callbacks
    // =========================
    private void OnLoaded(LevelPlayAdInfo adInfo)
    {
        state = AdState.Ready;
        Debug.Log("Interstitial Loaded");
    }

    private void OnLoadFailed(LevelPlayAdError error)
    {
        state = AdState.Failed;
        Invoke(nameof(Load), 5f); // 5초 후 재시도
    }

    private void OnDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Interstitial Displayed");
    }

    private void OnDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        state = AdState.Failed;
        Debug.LogError($"Interstitial Display Failed: {error}");
    }

    private void OnClosed(LevelPlayAdInfo adInfo)
    {
        state = AdState.None;
        Load();

        OnClosedCallback?.Invoke();
        OnClosedCallback = null;
    }

}
