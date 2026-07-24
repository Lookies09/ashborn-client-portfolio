using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 플레이어 상태 컴포넌트와 PlayerHUD UI를 연결해 수치를 갱신합니다.
/// </summary>
public class PlayerHUDController : MonoBehaviour
{
    [SerializeField] private PlayerHUD playerHUD;
    private PlayerHealth _playerHealth;
    private PlayerProgress _playerProgress;
    private PlayerRuntimeStats _runtimeStats;
    private PlayerLevelManager _levelManager;
    private SkillManager _skillManager;
    private PlayerWallet _playerWallet;
    //private float expPollInterval = 0.2f;

    private PlayerContext playerContext;
    private TextMeshProUGUI _skillButtonText;

    private void Awake()
    {
        if(playerHUD == null)
            playerHUD = GetComponentInChildren<PlayerHUD>();

        playerContext = FindFirstObjectByType<Player>().GetContext();
        _playerWallet = FindFirstObjectByType<PlayerWallet>();
        _playerProgress = FindFirstObjectByType<PlayerProgress>();

        _playerHealth = playerContext.Health;
        _runtimeStats = playerContext.RuntimeStats;
        _skillManager = playerContext.SkillManager;

        if (playerHUD != null)
        {
            playerHUD.SetSkillActiveButtonHandler(() => OnSkillActiveButtonClicked());
            HandleSkillStateChangedFromServer(_skillManager.IsGlobalSkillEnabled);
            playerHUD.SetInventoryOpenButtonHandler(() => UIManager.Instance.SetInventoryVisible(true));
        }

    }    

    private void Start()
    {        
        _levelManager = PlayerLevelManager.Instance;
        if (_playerWallet == null) _playerWallet = PlayerWallet.Instance;

        // 초기 수치 세팅
        RefreshAll();
    }

    private void OnEnable()
    {
        SubscribeEvents(true);
    }

    private void OnDisable()
    {
        SubscribeEvents(false);
    }

    private void SubscribeEvents(bool subscribe)
    {
        if (_playerHealth != null)
        {
            if (subscribe)
            {
                _playerHealth.OnDamaged += HandleHPChanged;
                _playerHealth.OnHealed += HandleHPChanged;
                _playerHealth.OnMaxHPChanged += HandleHPChanged;
            }
            else
            {
                _playerHealth.OnDamaged -= HandleHPChanged;
                _playerHealth.OnHealed -= HandleHPChanged;
                _playerHealth.OnMaxHPChanged -= HandleHPChanged;
            }
        }

        if (_runtimeStats != null)
        {
            if (subscribe) _runtimeStats.OnMaxHPChanged += HandleHPChanged;
            else _runtimeStats.OnMaxHPChanged -= HandleHPChanged;
        }

        if (_playerWallet != null)
        {
            if (subscribe) _playerWallet.OnTempGoldChanged += RefreshGold;
            else _playerWallet.OnTempGoldChanged -= RefreshGold;
        }

        if (_playerProgress != null)
        {
            if (subscribe) _playerProgress.OnProgressChanged += RefreshExp;
            else _playerProgress.OnProgressChanged -= RefreshExp;
        }

        if (_skillManager != null)
        {
            if (subscribe)
            {
                _skillManager.OnManaChanged += HandleMPChanged;
                _skillManager.OnSkillStateChanged += HandleSkillStateChangedFromServer;
            }
            else
            {
                _skillManager.OnManaChanged -= HandleMPChanged;
                _skillManager.OnSkillStateChanged -= HandleSkillStateChangedFromServer;
            }
        }

    }

    private void HandleHPChanged(int _) => RefreshHP();
    private void HandleMPChanged(int _) => RefreshMP();

    private void RefreshAll()
    {
        RefreshHP();
        RefreshMP();
        if (_playerProgress != null)
            RefreshExp(_playerProgress.CurrentExp, _playerProgress.CurrentLevel);
        if (_playerWallet != null) RefreshGold(_playerWallet.TempGold);
    }

    private void RefreshHP()
    {
        if (playerHUD == null || _playerHealth == null || _runtimeStats == null) return;
        playerHUD.UpdateHP(_playerHealth.CurrentHP, (int)_runtimeStats.GetStat(PlayerEnums.PlayerStatType.MaxHP));
    }

    private void RefreshMP()
    {
        if (playerHUD == null || _skillManager == null || _runtimeStats == null) return;
        playerHUD.UpdateMP(_skillManager.currentMp, (int)_runtimeStats.GetStat(PlayerEnums.PlayerStatType.MaxMp));
    }

    private void RefreshExp(int currentExp, int currentLevel)
    {
        int needExp = _levelManager.GetRequiredExpToNextLevel(currentLevel);
        playerHUD.UpdateExp(currentExp, needExp, currentLevel);
    }

    private void RefreshGold(int currentGold)
    {
        if (playerHUD != null) playerHUD.UpdateGold(currentGold);
    }

    private void HandleSkillStateChangedFromServer(bool isEnabled)
    {
        // UI 텍스트 업데이트
        if (_skillButtonText == null && playerHUD.SkillActiveBtn != null)
        {
            _skillButtonText = playerHUD.SkillActiveBtn.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (_skillButtonText != null)
        {
            _skillButtonText.text = isEnabled ? "스킬 ON" : "스킬 OFF";
        }
    }

    private void OnSkillActiveButtonClicked()
    {
        if (_skillManager == null) return;
        _skillManager.SetAllSkillsActive(!_skillManager.IsGlobalSkillEnabled);
    }
}


