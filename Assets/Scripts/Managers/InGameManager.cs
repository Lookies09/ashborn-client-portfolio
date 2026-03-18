using UnityEngine;
using System;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private float durationMinute = 10;
    public static InGameManager Instance { get; private set; }

    [Header("Player Settings")]
    [SerializeField] private GameObject player;

    [Header("Ad Settings")]
    private bool _hasUsedReviveAd = false;   // 부활 광고 사용했는가?
    private bool _hasUsedGoldBonusAd = false; // 골드 보너스 광고 사용했는가?   

    public float Timer { get; private set; }
    private float accumulatedTime;
    private bool isRunning;
    private PlayerHealth playerHealth;
    public bool CanRevive => !_hasUsedReviveAd;
    public bool CanGetGoldBonus => !_hasUsedGoldBonusAd;
    public bool IsEscapeSuccessful { get; private set; }

    public event Action<float> OnSecondElapsed;

    public event Action<bool> OnTimerEnded;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        StartTimer(durationMinute * 60);
    }

    private void OnEnable()
    {
        IsEscapeSuccessful = false;

        if (playerHealth != null)
        {
            playerHealth.OnKilled += OnPlayerKilled;
        }
        else
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null) playerHealth.OnKilled += OnPlayerKilled;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnKilled -= OnPlayerKilled;
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        Timer -= Time.deltaTime;
        accumulatedTime += Time.deltaTime;

        // 1초마다 이벤트 호출
        if (accumulatedTime >= 1f)
        {
            accumulatedTime -= 1f;
            OnSecondElapsed?.Invoke(Mathf.CeilToInt(Timer));
        }

        if (Timer <= 0f)
        {
            Timer = 0f;
            isRunning = false;
            OnTimeUp();
        }
    }

    public void SetPlayerOnStartPos(Transform pos)
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;

            player.transform.SetPositionAndRotation(pos.position, pos.rotation);

            controller.enabled = true;
        }
        else
        {
            player.transform.position = pos.position;
        }
    }


    public void StartTimer(float duration)
    {
        Timer = duration;
        isRunning = true;
    }

    private void OnTimeUp()
    {
        OnTimerEnded?.Invoke(true);
    }

    private void OnPlayerKilled()
    {
        if (!CanRevive)
        {
            GameManager.Instance.ResetPlayerData();
            PlayerDataManager.Instance.Save();
        }
        GameManager.Instance.OnGameOver(false);
    }

    public void OnEscaped()
    {
        GameManager.Instance.OnGameOver(true, 0f);
        IsEscapeSuccessful = true;
    }

    public void OnPlayerRevived()
    {
        playerHealth.Revive();
        UIManager.Instance.SetGameEndInfoVisible(false, false, 0f);

        // Todo: 이펙트?
    }

    public void UseReviveAd() { _hasUsedReviveAd = true; }
    public void UseGoldBonusAd() { _hasUsedGoldBonusAd = true; }
}
