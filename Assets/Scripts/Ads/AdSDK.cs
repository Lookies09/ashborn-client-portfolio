using Unity.Services.LevelPlay;
using UnityEngine;

public class AdSDK : MonoBehaviour
{
    public static AdSDK Instance;

    [SerializeField] private RewardedAdHandler rewardedHandler;
    [SerializeField] private InterstitialAdHandler interstitialHandler;
    [SerializeField] private string androidAppKey = "ANDROID_APP_KEY";
    [SerializeField] private string userId;

    [Header("Retry Value When Init Failed")]
    [SerializeField] private int maxInitRetry = 3;
    [SerializeField] private float retryDelay = 5f;
    private int retryCount;

    public bool IsInitialized { get; private set; }

    public RewardedAdHandler RewardedAdHandler => rewardedHandler;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

        if (string.IsNullOrEmpty(userId) || userId == "User00")
        {
            userId = SystemInfo.deviceUniqueIdentifier;
        }

        LevelPlay.Init(androidAppKey, userId);

    }

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
    }

    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        IsInitialized = true;

        rewardedHandler?.Init();
        interstitialHandler?.Init();
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        IsInitialized = false;
        Debug.LogError($"LevelPlay Init Failed: {error}");

        if (retryCount >= maxInitRetry)
            return;

        retryCount++;
        Invoke(nameof(RetryInit), retryDelay);

    }

    private void RetryInit()
    {
        LevelPlay.Init(androidAppKey, userId);
    }


    public bool TryShowInterstitial()
    {
        if (!IsInitialized)
            return false;

        if (!interstitialHandler.CanShow())
            return false;

        interstitialHandler.Show();
        return true;
    }


    public bool TryShowReward()
    {
        if (!IsInitialized)
            return false;

        if (!rewardedHandler.CanShow())
            return false;

        rewardedHandler.Show();
        return true;
    }

    public void SetInterstitialClosedCallback(System.Action callback)
    {
        interstitialHandler.OnClosedCallback -= callback;
        interstitialHandler.OnClosedCallback += callback;
    }

}
